using D365ClaudeAnalyzer.Models;
using Newtonsoft.Json;

namespace D365ClaudeAnalyzer.Services
{
    /// <summary>
    /// Manages loading and selection of prompt templates
    /// Because good prompts are the difference between "meh" and "wow"
    /// </summary>
    public class PromptService
    {
        private const string PromptsFileName = "prompts.json";
        private PromptConfiguration? _promptConfig;

        public PromptService()
        {
            LoadPrompts();
        }

        private void LoadPrompts()
        {
            try
            {
                if (!File.Exists(PromptsFileName))
                {
                    throw new FileNotFoundException($"Prompts file not found: {PromptsFileName}");
                }

                var json = File.ReadAllText(PromptsFileName);
                _promptConfig = JsonConvert.DeserializeObject<PromptConfiguration>(json);

                if (_promptConfig?.Prompts == null || !_promptConfig.Prompts.Any())
                {
                    throw new InvalidOperationException("No prompts found in configuration file");
                }

                Console.WriteLine($"✓ Loaded {_promptConfig.Prompts.Count} prompt templates");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error loading prompts: {ex.Message}");
                throw;
            }
        }

        public void DisplayAvailablePrompts()
        {
            if (_promptConfig?.Prompts == null) return;

            Console.WriteLine("\n📝 Available Prompt Templates:");
            var index = 1;
            foreach (var prompt in _promptConfig.Prompts)
            {
                Console.WriteLine($"   {index}. {prompt.Name}");
                index++;
            }
        }

        public PromptTemplate? GetPromptByKey(string key)
        {
            if (_promptConfig?.Prompts == null) return null;

            return _promptConfig.Prompts.FirstOrDefault(p => p.Name.Equals(key, StringComparison.OrdinalIgnoreCase)) ?? null;
        }
    }
}
