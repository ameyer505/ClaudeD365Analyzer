namespace D365ClaudeAnalyzer.Models
{
    public class ClaudeRequest
    {
        public string Model { get; set; } = "claude-sonnet-4-20250514";
        public int MaxTokens { get; set; } = 4096;
        public List<ClaudeMessage> Messages { get; set; } = new();
        public string? System { get; set; }
    }

    public class ClaudeMessage
    {
        public string Role { get; set; } = "user";
        public List<ClaudeContent> Content { get; set; } = new();
    }

    public class ClaudeContent
    {
        public string Type { get; set; } = "text";
        public string? Text { get; set; }
        public ClaudeDocumentSource? Source { get; set; }
    }

    public class ClaudeDocumentSource
    {
        public string Type { get; set; } = "base64";
        public string MediaType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public string Data { get; set; } = string.Empty;
    }

    public class ClaudeResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public List<ClaudeResponseContent> Content { get; set; } = new();
        public ClaudeUsage? Usage { get; set; }
    }

    public class ClaudeResponseContent
    {
        public string Type { get; set; } = string.Empty;
        public string? Text { get; set; }
    }

    public class ClaudeUsage
    {
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
    }
}
