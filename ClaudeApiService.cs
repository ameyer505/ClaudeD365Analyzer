using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using D365ClaudeAnalyzer.Models;

namespace D365ClaudeAnalyzer.Services;

public class ClaudeApiService
{
    private const string FilesApiUrl = "https://api.anthropic.com/v1/files";
    private const string FilesApiBeta = "code-execution-2025-08-25";

    private readonly string _apiKey;
    private readonly string _model;
    private readonly int _maxTokens;
    private readonly string _apiUrl;
    private readonly HttpClient _httpClient;

    public ClaudeApiService(string apiKey, string model, int maxTokens, string apiUrl)
    {
        _apiKey = apiKey;
        _model = model;
        _maxTokens = maxTokens;
        _apiUrl = apiUrl;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(10)
        };
    }

    public async Task<string> AnalyzeWithFileAsync(
        string systemPrompt,
        string userPrompt,
        string filePath)
    {
        Console.WriteLine("Preparing Claude API request...");

        // Read file info
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var fileName = Path.GetFileName(filePath);
        var extension = Path.GetExtension(filePath).ToLower();

        // Determine media type based on file extension
        var mediaType = extension switch
        {
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".csv" => "text/csv",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };

        Console.WriteLine($"File size: {fileBytes.Length / 1024.0:F2} KB");

        // Step 1: Upload the file to the Files API
        Console.WriteLine("Uploading file to Claude Files API...");
        var uploadedFileId = await UploadFileAsync(fileBytes, fileName, mediaType);
        Console.WriteLine($"File uploaded successfully. File ID: {uploadedFileId}");

        try
        {
            // Step 2: Send message with file reference
            Console.WriteLine($"Sending request to Claude API (Model: {_model})...\n");

            var requestBody = new
            {
                model = _model,
                max_tokens = _maxTokens,
                system = systemPrompt,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "container_upload",
                                file_id = uploadedFileId
                            },
                            new
                            {
                                type = "text",
                                text = userPrompt
                            }
                        }
                    }
                },
                tools = new[]
                {
                    new
                    {
                        type = "code_execution_20250825",
                        name = "code_execution"
                    }
                }
            };

            var jsonRequest = JsonConvert.SerializeObject(requestBody);

            var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");
            request.Headers.Add("anthropic-beta", FilesApiBeta);
            request.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Claude API request failed: {response.StatusCode}\n{responseContent}");
            }

            var jsonResponse = JObject.Parse(responseContent);

            // Extract the text content from the response
            var contentArray = jsonResponse["content"] as JArray;
            if (contentArray == null || contentArray.Count == 0)
            {
                throw new Exception("No content received from Claude API");
            }

            var textContent = new StringBuilder();
            foreach (var item in contentArray)
            {
                if (item["type"]?.ToString() == "text")
                {
                    textContent.AppendLine(item["text"]?.ToString());
                }
            }

            // Display token usage information for optimization
            var usage = jsonResponse["usage"];
            if (usage != null)
            {
                var inputTokens = usage["input_tokens"]?.ToObject<int>() ?? 0;
                var outputTokens = usage["output_tokens"]?.ToObject<int>() ?? 0;

                Console.WriteLine($"✓ Analysis complete!");
                Console.WriteLine($"  Input tokens: {inputTokens:N0}");
                Console.WriteLine($"  Output tokens: {outputTokens:N0}");
                Console.WriteLine($"  Total tokens: {(inputTokens + outputTokens):N0}\n");
            }

            return textContent.ToString();
        }
        finally
        {
            // Step 3: Clean up - delete the uploaded file - uncomment if desired
            //await DeleteFileAsync(fileId);
            //Console.WriteLine("Cleaned up uploaded file.");
        }
    }

    private async Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string mediaType)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        content.Add(fileContent, "file", fileName);

        var request = new HttpRequestMessage(HttpMethod.Post, FilesApiUrl);
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Headers.Add("anthropic-beta", "files-api-2025-04-14");
        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to upload file: {response.StatusCode}\n{responseContent}");
        }

        var jsonResponse = JObject.Parse(responseContent);
        var fileId = jsonResponse["id"]?.ToString();

        if (string.IsNullOrEmpty(fileId))
        {
            throw new Exception("No file ID returned from upload");
        }

        return fileId;
    }

    private async Task DeleteFileAsync(string fileId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{FilesApiUrl}/{fileId}");
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");
            request.Headers.Add("anthropic-beta", FilesApiBeta);

            await _httpClient.SendAsync(request);
        }
        catch (Exception ex)
        {
            // Log but don't fail if cleanup fails
            Console.WriteLine($"Warning: Failed to delete uploaded file: {ex.Message}");
        }
    }
}
