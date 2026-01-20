namespace D365ClaudeAnalyzer.Models;

public class D365Config
{
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}

public class ClaudeConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4-20250514";
    public int MaxTokens { get; set; } = 4096;
    public string ApiUrl { get; set; } = "https://api.anthropic.com/v1/messages";
}

public class OutputConfig
{
    public string ExcelFileName { get; set; } = "D365_Data_Export.xlsx";
    public string Directory { get; set; } = "./output";
}
