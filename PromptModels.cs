namespace D365ClaudeAnalyzer.Models;

public class PromptConfiguration
{
    public List<PromptTemplate> Prompts { get; set; } = new();
}

public class PromptTemplate
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
}
