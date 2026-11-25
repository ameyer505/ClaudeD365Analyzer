namespace D365ClaudeAnalyzer.Models;

public class D365Config
{
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}

/// <summary>
/// Settings for the Claude API call.
/// The API key is optional here: if it's blank (or still the template placeholder),
/// the Anthropic SDK reads it from the ANTHROPIC_API_KEY environment variable instead,
/// which is the recommended way to keep secrets out of config files.
/// </summary>
public class ClaudeConfig
{
    public string? ApiKey { get; set; }

    /// <summary>Model ID. Opus 5.5 uses a fixed ID with no date suffix.</summary>
    public string Model { get; set; } = "claude-opus-5-5";

    /// <summary>
    /// Hard ceiling on output tokens for each request: thinking + tool calls + final answer.
    /// Opus 5.5 always thinks, so this needs to be much larger than the old 4096.
    /// </summary>
    public int MaxTokens { get; set; } = 32000;

    /// <summary>
    /// How hard Claude thinks: low | medium | high | xhigh | max.
    /// This is the only knob for thinking depth on Opus 5.5 (the API default is medium).
    /// </summary>
    public string Effort { get; set; } = "high";

    /// <summary>
    /// summarized = stream readable thinking summaries to the console (great for demos).
    /// omitted    = thinking still happens, but the text isn't returned (the API default).
    /// </summary>
    public string ThinkingDisplay { get; set; } = "summarized";

    /// <summary>Uploaded input files auto-expire on Anthropic's side after this many hours (1 to 2160).</summary>
    public int FileExpirationHours { get; set; } = 24;

    /// <summary>Delete the uploaded D365 export, and the generated files, from the Files API once the run finishes.</summary>
    public bool DeleteFilesAfterRun { get; set; } = true;

    /// <summary>Safety valve for long code-execution turns that come back with stop_reason "pause_turn".</summary>
    public int MaxContinuations { get; set; } = 5;

    /// <summary>USD per million tokens, used only for the cost estimate printed at the end of a run.</summary>
    public decimal InputPricePerMTok { get; set; } = 4m;
    public decimal OutputPricePerMTok { get; set; } = 20m;
}

/// <summary>
/// Where the app writes its files locally.
/// (Renamed from OutputConfig so it doesn't collide with the Anthropic SDK's OutputConfig type.)
/// </summary>
public class OutputSettings
{
    public string ExcelFileName { get; set; } = "D365_Data_Export.xlsx";
    public string Directory { get; set; } = "./output";
}
