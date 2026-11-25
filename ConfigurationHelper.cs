using System.Configuration;
using System.Globalization;
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
        var defaults = new ClaudeConfig();

        var config = new ClaudeConfig
        {
            ApiKey = GetOptionalAppSetting("Claude:ApiKey"),
            Model = GetOptionalAppSetting("Claude:Model") ?? defaults.Model,
            MaxTokens = GetInt("Claude:MaxTokens", defaults.MaxTokens),
            Effort = GetOptionalAppSetting("Claude:Effort") ?? defaults.Effort,
            ThinkingDisplay = GetOptionalAppSetting("Claude:ThinkingDisplay") ?? defaults.ThinkingDisplay,
            FileExpirationHours = GetInt("Claude:FileExpirationHours", defaults.FileExpirationHours),
            DeleteFilesAfterRun = GetBool("Claude:DeleteFilesAfterRun", defaults.DeleteFilesAfterRun),
            MaxContinuations = GetInt("Claude:MaxContinuations", defaults.MaxContinuations),
            InputPricePerMTok = GetDecimal("Claude:InputPricePerMTok", defaults.InputPricePerMTok),
            OutputPricePerMTok = GetDecimal("Claude:OutputPricePerMTok", defaults.OutputPricePerMTok)
        };

        // Treat the template's placeholder as "not set" so the SDK falls back to ANTHROPIC_API_KEY
        if (config.ApiKey is not null && config.ApiKey.Contains("your-key-here", StringComparison.OrdinalIgnoreCase))
        {
            config.ApiKey = null;
        }

        if (config.ApiKey is null && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")))
        {
            throw new ConfigurationErrorsException(
                "No Claude API key found. Set the ANTHROPIC_API_KEY environment variable (recommended) " +
                "or the Claude:ApiKey setting in App.config.");
        }

        return config;
    }

    public static OutputSettings GetOutputSettings()
    {
        return new OutputSettings
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

    private static string? GetOptionalAppSetting(string key)
    {
        var value = ConfigurationManager.AppSettings[key];
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static int GetInt(string key, int fallback) =>
        int.TryParse(GetOptionalAppSetting(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : fallback;

    private static bool GetBool(string key, bool fallback) =>
        bool.TryParse(GetOptionalAppSetting(key), out var v) ? v : fallback;

    private static decimal GetDecimal(string key, decimal fallback) =>
        decimal.TryParse(GetOptionalAppSetting(key), NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : fallback;

    public static void ValidateConfiguration()
    {
        Console.WriteLine("Validating configuration...\n");

        try
        {
            GetD365Config();
            Console.WriteLine("✓ D365 configuration loaded");

            var claudeConfig = GetClaudeConfig();
            var keySource = claudeConfig.ApiKey is null ? "ANTHROPIC_API_KEY environment variable" : "App.config";
            Console.WriteLine($"✓ Claude configuration loaded (model: {claudeConfig.Model}, effort: {claudeConfig.Effort}, key from {keySource})");

            GetOutputSettings();
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
