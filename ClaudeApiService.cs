using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Anthropic;
using Anthropic.Core;
using Anthropic.Exceptions;
using Anthropic.Helpers;
using Anthropic.Models.Files;
using Anthropic.Models.Messages;
using D365ClaudeAnalyzer.Models;

namespace D365ClaudeAnalyzer.Services;

/// <summary>
/// What came back from a Claude analysis run.
/// </summary>
public sealed record AnalysisResult(
    string Text,
    IReadOnlyList<string> DownloadedFiles,
    long InputTokens,
    long OutputTokens,
    long CacheReadTokens,
    decimal EstimatedCostUsd,
    string? StopReason);

/// <summary>
/// Sends the D365 export to Claude using the official Anthropic C# SDK (NuGet: Anthropic).
///
/// The flow:
///   1. Upload the workbook with the Files API (with an auto-expiration, because this is security data)
///   2. Stream a Messages request: container_upload block + code execution tool, Opus 5.5 with adaptive thinking
///   3. If the API pauses a long code-execution turn (stop_reason "pause_turn"), hand the turn back so Claude keeps going
///   4. Download every file Claude saved to $OUTPUT_DIR in its sandbox (the Excel and checklist outputs)
///   5. Clean up the uploaded and generated files
///
/// No more hand-rolled HttpClient, JSON payloads, or beta headers: the SDK handles auth, retries,
/// timeouts and serialization, and the Files API and code execution are both GA now.
/// </summary>
public sealed class ClaudeApiService : IDisposable
{
    // Appended to every prompt so Claude saves its deliverables where the API can hand them back to us.
    // Only files at the top level of $OUTPUT_DIR come back as downloadable file IDs.
    private const string OutputInstructions = """

        Output files: save every file you create for me (for example .xlsx, .csv, .md or .png) to the top
        level of the $OUTPUT_DIR directory, and run `ls "$OUTPUT_DIR"` in the same command so you can confirm
        the files are there. Files saved anywhere else won't be returned to me. After the files are saved,
        reply with a concise summary of your key findings and the names of the files you created.
        """;

    private readonly ClaudeConfig _config;
    private readonly AnthropicClient _client;
    private readonly Effort _effort;
    private readonly Display _thinkingDisplay;

    public ClaudeApiService(ClaudeConfig config)
    {
        _config = config;

        // With no ApiKey, the SDK reads ANTHROPIC_API_KEY (and ANTHROPIC_BASE_URL) from the environment.
        // The SDK also retries 408/409/429/5xx and connection errors with backoff (2 retries by default).
        _client = string.IsNullOrWhiteSpace(config.ApiKey)
            ? new AnthropicClient()
            : new AnthropicClient { ApiKey = config.ApiKey };

        _effort = Enum.TryParse<Effort>(config.Effort, ignoreCase: true, out var effort)
            ? effort
            : throw new ArgumentException($"Claude:Effort '{config.Effort}' isn't valid. Use low, medium, high, xhigh or max.");

        _thinkingDisplay = Enum.TryParse<Display>(config.ThinkingDisplay, ignoreCase: true, out var display)
            ? display
            : throw new ArgumentException($"Claude:ThinkingDisplay '{config.ThinkingDisplay}' isn't valid. Use summarized or omitted.");
    }

    public async Task<AnalysisResult> AnalyzeWithFileAsync(
        string systemPrompt,
        string userPrompt,
        string filePath,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        var fileIdsToDelete = new List<string>();

        try
        {
            // ── Step 1: upload the workbook ────────────────────────────────────────────────
            var uploaded = await UploadFileAsync(filePath, cancellationToken);
            fileIdsToDelete.Add(uploaded.ID);

            // ── Step 2: build the conversation ─────────────────────────────────────────────
            // The container_upload block mounts the file inside the code execution sandbox,
            // where Claude can open it with pandas/openpyxl instead of us converting it to text.
            var messages = new List<MessageParam>
            {
                new UserMessageParam
                {
                    Content = new List<ContentBlockParam>
                    {
                        new ContainerUploadBlockParam(uploaded.ID),
                        new TextBlockParam { Text = userPrompt + OutputInstructions }
                    }
                }
            };

            Console.WriteLine($"Sending request to Claude (model: {_config.Model}, effort: {_config.Effort.ToLowerInvariant()})...\n");

            var allTurns = new List<Message>();
            string? containerId = null;
            Message response;

            // ── Step 3: stream, continuing paused turns ───────────────────────────────────
            var continuation = 0;
            while (true)
            {
                response = await StreamTurnAsync(BuildParams(systemPrompt, messages, containerId), cancellationToken);
                allTurns.Add(response);

                // The container holds the uploaded file and anything Claude wrote; reuse it on continuation.
                containerId = response.Container?.ID ?? containerId;

                if (response.StopReason?.Raw() != "pause_turn")
                    break;

                if (++continuation > _config.MaxContinuations)
                {
                    WriteColor($"\n⚠ Stopped after {_config.MaxContinuations} continuations (Claude:MaxContinuations).", ConsoleColor.Yellow);
                    break;
                }

                // pause_turn means the API paused a long-running turn. Send Claude's partial turn back
                // exactly as received (thinking blocks included, unmodified) and it picks up where it left off.
                WriteColor("\n↻ Long-running turn paused by the API; continuing...\n", ConsoleColor.DarkYellow);
                messages.Add(new AssistantMessageParam { Content = ToContentParams(response.Content) });
            }

            ReportStopReason(response);

            // ── Step 4: download what Claude produced ─────────────────────────────────────
            var generatedFileIds = allTurns.SelectMany(GetGeneratedFileIds).Distinct().ToList();
            fileIdsToDelete.AddRange(generatedFileIds);
            var downloads = await DownloadFilesAsync(generatedFileIds, outputDirectory, cancellationToken);

            // ── Usage & cost ──────────────────────────────────────────────────────────────
            var inputTokens = allTurns.Sum(m => m.Usage.InputTokens + (m.Usage.CacheCreationInputTokens ?? 0));
            var cacheReadTokens = allTurns.Sum(m => m.Usage.CacheReadInputTokens ?? 0);
            var outputTokens = allTurns.Sum(m => m.Usage.OutputTokens);
            // Cache reads bill at roughly a tenth of the base input rate.
            var cost = (inputTokens * _config.InputPricePerMTok
                        + cacheReadTokens * _config.InputPricePerMTok * 0.1m
                        + outputTokens * _config.OutputPricePerMTok) / 1_000_000m;

            Console.WriteLine();
            Console.WriteLine("✓ Analysis complete!");
            Console.WriteLine($"  Requests:      {allTurns.Count}");
            Console.WriteLine($"  Input tokens:  {inputTokens:N0}" + (cacheReadTokens > 0 ? $" (+{cacheReadTokens:N0} cache reads)" : ""));
            Console.WriteLine($"  Output tokens: {outputTokens:N0}  (includes thinking)");
            Console.WriteLine($"  Est. cost:     ${cost:F2} USD (tokens only; code execution container time is billed separately)\n");

            var text = string.Join(
                Environment.NewLine,
                allTurns.SelectMany(m => m.Content)
                        .Select(b => b.TryPickText(out var t) ? t.Text : null)
                        .Where(t => !string.IsNullOrWhiteSpace(t)));

            return new AnalysisResult(text, downloads, inputTokens, outputTokens, cacheReadTokens, cost,
                response.StopReason?.Raw());
        }
        catch (AnthropicApiException ex)
        {
            // Carries the HTTP status, the error body, and a request ID to quote to Anthropic support.
            throw new InvalidOperationException(
                $"Claude API returned {(int)ex.StatusCode} ({ex.StatusCode}). Request ID: {ex.RequestID ?? "n/a"}\n{ex.ResponseBody}", ex);
        }
        finally
        {
            // ── Step 5: clean up ──────────────────────────────────────────────────────────
            if (_config.DeleteFilesAfterRun)
            {
                await DeleteFilesAsync(fileIdsToDelete);
            }
        }
    }

    private MessageCreateParams BuildParams(string systemPrompt, List<MessageParam> messages, string? containerId)
    {
        var parameters = new MessageCreateParams
        {
            Model = _config.Model,
            MaxTokens = _config.MaxTokens,
            System = systemPrompt,
            Messages = [.. messages],

            // Opus 5.5 always uses adaptive thinking: no budget_tokens, no "disabled".
            // Effort is the dial, and "summarized" lets us show Claude's reasoning on screen.
            Thinking = new ThinkingConfigAdaptive { Display = _thinkingDisplay },
            OutputConfig = new OutputConfig { Effort = _effort },

            // Latest code execution tool version. GA, so no anthropic-beta header needed.
            Tools = [new CodeExecutionTool20260521()],

            // Note: no temperature/top_p/top_k (rejected by API version 2026-10-21)
            // and no forced tool_choice (Opus 5.5 only accepts auto or none).
        };

        return containerId is null ? parameters : parameters with { Container = containerId };
    }

    /// <summary>
    /// Streams one request, echoing thinking summaries, sandbox activity and the answer to the console
    /// as they arrive, then returns the fully assembled Message.
    /// Streaming is required here anyway: the SDK refuses non-streaming calls that could run past 10 minutes.
    /// </summary>
    private async Task<Message> StreamTurnAsync(MessageCreateParams parameters, CancellationToken cancellationToken)
    {
        var aggregator = new MessageContentAggregator();
        string? currentBlock = null;

        await foreach (var evt in _client.Messages.CreateStreaming(parameters, cancellationToken).CollectAsync(aggregator))
        {
            if (evt.TryPickContentBlockStart(out var start))
            {
                currentBlock = start.ContentBlock.Type.Raw();
                switch (currentBlock)
                {
                    case "thinking" when _thinkingDisplay == Display.Summarized:
                        WriteColor("\n💭 Thinking: ", ConsoleColor.DarkGray, newLine: false);
                        break;
                    case "server_tool_use":
                        WriteColor("\n🔧 Claude is running code in its sandbox...", ConsoleColor.Cyan);
                        break;
                    case "bash_code_execution_tool_result":
                    case "text_editor_code_execution_tool_result":
                        WriteColor("   ↳ step finished", ConsoleColor.DarkCyan);
                        break;
                    case "text":
                        Console.WriteLine();
                        break;
                }
            }
            else if (evt.TryPickContentBlockDelta(out var delta))
            {
                if (delta.Delta.TryPickThinking(out var thinking))
                {
                    WriteColor(thinking.Thinking, ConsoleColor.DarkGray, newLine: false);
                }
                else if (delta.Delta.TryPickText(out var text))
                {
                    Console.Write(text.Text);
                }
            }
        }

        Console.WriteLine();
        return aggregator.Message();
    }

    private async Task<FileMetadata> UploadFileAsync(string filePath, CancellationToken cancellationToken)
    {
        var fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
        var fileName = Path.GetFileName(filePath);
        var mediaType = Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".csv" => "text/csv",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };

        Console.WriteLine($"Uploading {fileName} ({fileBytes.Length / 1024.0:F2} KB) to the Claude Files API...");

        // Building the content from a byte array (rather than a FileStream) keeps the upload retryable.
        BinaryContent content = fileBytes;

        var uploaded = await _client.Files.Upload(new FileUploadParams
        {
            File = content with { FileName = fileName, ContentType = new MediaTypeHeaderValue(mediaType) },
            // D365 security data shouldn't linger: the file expires server-side even if cleanup never runs.
            ExpiresInSeconds = Math.Clamp(_config.FileExpirationHours, 1, 2160) * 3600L
        }, cancellationToken);

        Console.WriteLine($"✓ Uploaded. File ID: {uploaded.ID} (expires {uploaded.ExpiresAt?.ToLocalTime():g})\n");
        return uploaded;
    }

    /// <summary>Files Claude saved to $OUTPUT_DIR come back as file IDs inside bash tool results.</summary>
    private static IEnumerable<string> GetGeneratedFileIds(Message message)
    {
        foreach (var block in message.Content)
        {
            if (block.TryPickBashCodeExecutionToolResult(out var toolResult) &&
                toolResult.Content.TryPickBashCodeExecutionResultBlock(out var result))
            {
                foreach (var output in result.Content)
                {
                    yield return output.FileID;
                }
            }
        }
    }

    private async Task<List<string>> DownloadFilesAsync(
        IReadOnlyList<string> fileIds, string outputDirectory, CancellationToken cancellationToken)
    {
        var saved = new List<string>();
        if (fileIds.Count == 0)
        {
            WriteColor("⚠ Claude didn't save any files to $OUTPUT_DIR, so there's nothing to download.", ConsoleColor.Yellow);
            return saved;
        }

        var runFolder = Path.Combine(outputDirectory, $"Claude_{DateTime.Now:yyyyMMdd_HHmmss}");
        Directory.CreateDirectory(runFolder);

        foreach (var fileId in fileIds)
        {
            var metadata = await _client.Files.RetrieveMetadata(fileId, cancellationToken: cancellationToken);
            var localPath = Path.Combine(runFolder, Path.GetFileName(metadata.Filename));

            using var download = await _client.Files.Download(fileId, cancellationToken: cancellationToken);
            await using var source = await download.ReadAsStream(cancellationToken);
            await using var target = File.Create(localPath);
            await source.CopyToAsync(target, cancellationToken);

            Console.WriteLine($"✓ Downloaded {metadata.Filename} → {localPath}");
            saved.Add(localPath);
        }

        return saved;
    }

    private async Task DeleteFilesAsync(IEnumerable<string> fileIds)
    {
        foreach (var fileId in fileIds.Distinct())
        {
            try
            {
                await _client.Files.Delete(fileId);
            }
            catch (Exception ex)
            {
                // Log but don't fail the run if cleanup fails; the upload also has an expiration as a backstop.
                WriteColor($"⚠ Couldn't delete file {fileId}: {ex.Message}", ConsoleColor.Yellow);
            }
        }
    }

    /// <summary>
    /// Converts response blocks to request blocks by round-tripping their raw JSON, so thinking blocks
    /// (and their signatures) and server tool results go back to the API byte-for-byte as received.
    /// </summary>
    private static List<ContentBlockParam> ToContentParams(IReadOnlyList<ContentBlock> blocks) =>
        blocks.Select(b => JsonSerializer.Deserialize<ContentBlockParam>(b.Json)
                           ?? throw new InvalidOperationException("Couldn't convert a response block for continuation."))
              .ToList();

    private static void ReportStopReason(Message response)
    {
        switch (response.StopReason?.Raw())
        {
            case "max_tokens":
                WriteColor($"\n⚠ Hit Claude:MaxTokens before finishing. Raise MaxTokens or lower Claude:Effort.", ConsoleColor.Yellow);
                break;
            case "refusal":
                WriteColor($"\n⚠ Claude declined this request: {response.StopDetails?.Explanation ?? "no details returned"}", ConsoleColor.Yellow);
                break;
            case "model_context_window_exceeded":
                WriteColor("\n⚠ The conversation outgrew the context window. Try trimming the OData $select columns.", ConsoleColor.Yellow);
                break;
        }
    }

    private static void WriteColor(string text, ConsoleColor color, bool newLine = true)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        if (newLine) Console.WriteLine(text); else Console.Write(text);
        Console.ForegroundColor = previous;
    }

    public void Dispose() => _client.Dispose();
}
