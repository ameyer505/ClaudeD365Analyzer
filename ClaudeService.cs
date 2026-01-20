using D365ClaudeAnalyzer.Models;
using Newtonsoft.Json;
using System.Configuration;
using System.Text;

namespace D365ClaudeAnalyzer.Services
{
    /// <summary>
    /// Handles communication with the Claude AI API
    /// Making AI do the heavy lifting so you don't have to stare at spreadsheets all day
    /// </summary>
    public class ClaudeService
    {
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly string _model;
        private readonly int _maxTokens;
        private readonly string _anthropicVersion;
        private readonly HttpClient _httpClient;

        public ClaudeService()
        {
            _apiKey = ConfigurationManager.AppSettings["Claude:ApiKey"] 
                ?? throw new ConfigurationErrorsException("Claude:ApiKey not found in configuration");
            _apiUrl = ConfigurationManager.AppSettings["Claude:ApiUrl"] 
                ?? "https://api.anthropic.com/v1/messages";
            _model = ConfigurationManager.AppSettings["Claude:Model"] 
                ?? "claude-sonnet-4-20250514";
            _maxTokens = int.Parse(ConfigurationManager.AppSettings["Claude:MaxTokens"] ?? "4096");
            _anthropicVersion = ConfigurationManager.AppSettings["Claude:AnthropicVersion"] 
                ?? "2023-06-01";

            _httpClient = new HttpClient();
        }

        public async Task<string> AnalyzeDataAsync(
            string excelFilePath, 
            PromptTemplate promptTemplate,
            string? modelOverride = null)
        {
            try
            {
                Console.WriteLine($"\n🤖 Sending data to Claude AI for analysis...");
                Console.WriteLine($"   Model: {modelOverride ?? _model}");
                Console.WriteLine($"   Prompt: {promptTemplate.Name}");

                // Read Excel file and convert to base64
                var excelBytes = await File.ReadAllBytesAsync(excelFilePath);
                var base64Excel = Convert.ToBase64String(excelBytes);
                var fileSize = excelBytes.Length / 1024.0; // Size in KB

                Console.WriteLine($"   File size: {fileSize:F2} KB");

                // Estimate tokens (rough estimate: 1 token ≈ 4 bytes for base64)
                var estimatedTokens = excelBytes.Length / 4;
                Console.WriteLine($"   Estimated input tokens: ~{estimatedTokens:N0}");

                if (estimatedTokens > 150000) // Claude has ~200K context window, leave room for response
                {
                    Console.WriteLine($"   ⚠ Warning: Large file may approach token limits");
                }

                // Build Claude request
                var request = new ClaudeRequest
                {
                    Model = modelOverride ?? _model,
                    MaxTokens = _maxTokens,
                    System = promptTemplate.SystemPrompt,
                    Messages = new List<ClaudeMessage>
                    {
                        new ClaudeMessage
                        {
                            Role = "user",
                            Content = new List<ClaudeContent>
                            {
                                new ClaudeContent
                                {
                                    Type = "document",
                                    Source = new ClaudeDocumentSource
                                    {
                                        Type = "base64",
                                        MediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                        Data = base64Excel
                                    }
                                },
                                new ClaudeContent
                                {
                                    Type = "text",
                                    Text = promptTemplate.UserPrompt
                                }
                            }
                        }
                    }
                };

                // Configure HTTP request
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
                _httpClient.DefaultRequestHeaders.Add("anthropic-version", _anthropicVersion);

                var jsonRequest = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                Console.WriteLine($"   Waiting for Claude's analysis...");
                var startTime = DateTime.Now;

                var response = await _httpClient.PostAsync(_apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var elapsed = (DateTime.Now - startTime).TotalSeconds;
                Console.WriteLine($"   Response received in {elapsed:F1}s");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"   ✗ Claude API error: {response.StatusCode}");
                    Console.WriteLine($"   Error details: {responseContent}");
                    throw new HttpRequestException($"Claude API request failed: {responseContent}");
                }

                var claudeResponse = JsonConvert.DeserializeObject<ClaudeResponse>(responseContent);

                if (claudeResponse?.Content == null || !claudeResponse.Content.Any())
                {
                    throw new InvalidOperationException("Claude API returned empty response");
                }

                // Extract text from response
                var analysisText = string.Join("\n", 
                    claudeResponse.Content
                        .Where(c => c.Type == "text" && !string.IsNullOrWhiteSpace(c.Text))
                        .Select(c => c.Text));

                // Display token usage
                if (claudeResponse.Usage != null)
                {
                    Console.WriteLine($"   Token usage:");
                    Console.WriteLine($"     Input:  {claudeResponse.Usage.InputTokens:N0}");
                    Console.WriteLine($"     Output: {claudeResponse.Usage.OutputTokens:N0}");
                    Console.WriteLine($"     Total:  {(claudeResponse.Usage.InputTokens + claudeResponse.Usage.OutputTokens):N0}");
                }

                Console.WriteLine($"   ✓ Analysis complete!");

                return analysisText;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ Error during Claude analysis: {ex.Message}");
                throw;
            }
        }

        public void DisplayAvailableModels()
        {
            Console.WriteLine("\n📋 Available Claude Models:");
            Console.WriteLine("   1. claude-sonnet-4-20250514    (Recommended - Best balance)");
            Console.WriteLine("   2. claude-opus-4-20250514      (Most capable, slower)");
            Console.WriteLine("   3. claude-haiku-4-20251001     (Fastest, most economical)");
        }
    }
}
