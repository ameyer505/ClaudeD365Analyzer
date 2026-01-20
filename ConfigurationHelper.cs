using System.Configuration;
using Newtonsoft.Json;
using D365ClaudeAnalyzer.Models;

namespace D365ClaudeAnalyzer.Helpers;

public static class ConfigurationHelper
{
    public static D365Config GetD365Config()
    {
        return new D365Config
        {
            TenantId = GetAppSetting("D365:TenantId"),
            ClientId = GetAppSetting("D365:ClientId"),
            ClientSecret = GetAppSetting("D365:ClientSecret"),
            Resource = GetAppSetting("D365:Resource"),
            BaseUrl = GetAppSetting("D365:BaseUrl")
        };
    }

    public static ClaudeConfig GetClaudeConfig()
    {
        return new ClaudeConfig
        {
            ApiKey = GetAppSetting("Claude:ApiKey"),
            Model = GetAppSetting("Claude:Model"),
            MaxTokens = int.Parse(GetAppSetting("Claude:MaxTokens")),
            ApiUrl = GetAppSetting("Claude:ApiUrl")
        };
    }

    public static OutputConfig GetOutputConfig()
    {
        return new OutputConfig
        {
            ExcelFileName = GetAppSetting("Output:ExcelFileName"),
            Directory = GetAppSetting("Output:Directory")
        };
    }

    public static PromptConfiguration LoadPrompts(string promptsFilePath = "prompts.json")
    {
        if (!File.Exists(promptsFilePath))
        {
            throw new FileNotFoundException($"Prompts configuration file not found: {promptsFilePath}");
        }

        var json = File.ReadAllText(promptsFilePath);
        return JsonConvert.DeserializeObject<PromptConfiguration>(json) 
            ?? throw new Exception("Failed to deserialize prompts configuration");
    }

    private static string GetAppSetting(string key)
    {
        var value = ConfigurationManager.AppSettings[key];
        if (string.IsNullOrEmpty(value))
        {
            throw new ConfigurationErrorsException($"Missing required configuration: {key}");
        }
        return value;
    }

    public static void ValidateConfiguration()
    {
        Console.WriteLine("Validating configuration...\n");

        try
        {
            var d365Config = GetD365Config();
            Console.WriteLine("✓ D365 configuration loaded");

            var claudeConfig = GetClaudeConfig();
            Console.WriteLine("✓ Claude configuration loaded");

            var outputConfig = GetOutputConfig();
            Console.WriteLine("✓ Output configuration loaded");

            var prompts = LoadPrompts();
            Console.WriteLine($"✓ Loaded {prompts.Prompts.Count} prompt templates\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Configuration validation failed: {ex.Message}\n");
            throw;
        }
    }
}
