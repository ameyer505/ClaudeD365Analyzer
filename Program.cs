using D365ClaudeAnalyzer.Helpers;
using D365ClaudeAnalyzer.Models;
using D365ClaudeAnalyzer.Services;

namespace D365ClaudeAnalyzer;

class Program
{
    static async Task Main(string[] args)
    {
        // So the ✓ / 💭 / 🔧 glyphs render in Windows Terminal
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("   D365 Finance & Supply Chain → Claude AI Analyzer");
        Console.WriteLine("═══════════════════════════════════════════════════════════\n");

        try
        {
            // Validate configuration
            ConfigurationHelper.ValidateConfiguration();

            // Load configurations
            var d365Config = ConfigurationHelper.GetD365Config();
            var claudeConfig = ConfigurationHelper.GetClaudeConfig();
            var outputConfig = ConfigurationHelper.GetOutputSettings();
            var promptConfig = ConfigurationHelper.LoadPrompts();

            // Initialize services
            var authService = new D365AuthService(
                d365Config.TenantId,
                d365Config.ClientId,
                d365Config.ClientSecret,
                d365Config.Resource);

            var odataService = new D365ODataService(authService, d365Config.BaseUrl);
            var excelService = new ExcelExportService();
            using var claudeService = new ClaudeApiService(claudeConfig);

            // Define OData queries - CUSTOMIZE THESE FOR YOUR NEEDS
            var queries = new List<ODataQuery>
            {
                // Get all enabled users
                new ODataQuery
                {
                    SheetName = "Users",
                    EntityName = "SystemUsers",
                    Filter = "Enabled eq true",
                    Select = "UserID,Alias,UserName"
                },
                
                // Get all security roles
                new ODataQuery
                {
                    SheetName = "Roles",
                    EntityName = "SecurityRoles",
                    Select = "SecurityRoleIdentifier,SecurityRoleName"
                },
                
                // Get all user role assignments that are enabled
                new ODataQuery
                {
                    SheetName = "UserRoles",
                    EntityName = "SecurityUserRoles",
                    Select = "UserId,SecurityRoleIdentifier,SecurityRoleName",
                    Filter = "AssignmentStatus eq Microsoft.Dynamics.DataEntities.RoleAssignmentStatus'Enabled'"
                }
            };

            // Execute OData queries and retrieve data
            var dataSheets = await odataService.ExecuteMultipleQueriesAsync(queries);

            // Create Excel file
            var outputPath = Path.Combine(outputConfig.Directory, outputConfig.ExcelFileName);
            var excelFilePath = excelService.CreateExcelFile(dataSheets, outputPath);

            // Display available prompts
            Console.WriteLine("Available Analysis Prompts:");
            for (int i = 0; i < promptConfig.Prompts.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {promptConfig.Prompts[i].Name}");
            }

            Console.Write("\nSelect a prompt (1-{0}) or press Enter to skip analysis: ", promptConfig.Prompts.Count);
            var selection = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(selection) && int.TryParse(selection, out int promptIndex))
            {
                if (promptIndex > 0 && promptIndex <= promptConfig.Prompts.Count)
                {
                    var selectedPrompt = promptConfig.Prompts[promptIndex - 1];
                    
                    Console.WriteLine($"\nUsing prompt: {selectedPrompt.Name}");
                    Console.WriteLine("─────────────────────────────────────────────────────────\n");

                    // Analyze with Claude (streams thinking + progress to the console as it works)
                    var result = await claudeService.AnalyzeWithFileAsync(
                        selectedPrompt.SystemPrompt,
                        selectedPrompt.UserPrompt,
                        excelFilePath,
                        outputConfig.Directory);

                    // Display analysis results
                    Console.WriteLine("═══════════════════════════════════════════════════════════");
                    Console.WriteLine("   CLAUDE AI ANALYSIS");
                    Console.WriteLine("═══════════════════════════════════════════════════════════\n");
                    Console.WriteLine(result.Text);
                    Console.WriteLine("\n═══════════════════════════════════════════════════════════");

                    // Save the written summary alongside the files Claude generated
                    var analysisPath = Path.Combine(
                        outputConfig.Directory,
                        $"Analysis_{DateTime.Now:yyyyMMdd_HHmmss}.md");

                    await File.WriteAllTextAsync(analysisPath, result.Text);
                    Console.WriteLine($"\n✓ Analysis saved to: {analysisPath}");

                    if (result.DownloadedFiles.Count > 0)
                    {
                        Console.WriteLine("✓ Files generated by Claude:");
                        foreach (var file in result.DownloadedFiles)
                        {
                            Console.WriteLine($"   • {file}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Invalid selection. Skipping analysis.");
                }
            }
            else
            {
                Console.WriteLine("Skipping Claude analysis.");
            }

            Console.WriteLine("\n✓ Process completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
            Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
            Environment.ExitCode = 1;
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey(intercept: true);
    }
}
