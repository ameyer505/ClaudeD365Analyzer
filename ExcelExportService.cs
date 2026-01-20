using ClosedXML.Excel;
using Newtonsoft.Json.Linq;

namespace D365ClaudeAnalyzer.Services;

public class ExcelExportService
{
    public string CreateExcelFile(List<(string SheetName, JArray Data)> dataSheets, string outputPath)
    {
        Console.WriteLine("\nCreating Excel file...");
        
        using var workbook = new XLWorkbook();
        
        foreach (var (sheetName, data) in dataSheets)
        {
            if (data.Count == 0)
            {
                Console.WriteLine($"⚠ Skipping {sheetName} - no data");
                continue;
            }

            var worksheet = workbook.Worksheets.Add(SanitizeSheetName(sheetName));
            
            // Get all unique property names from the data
            var properties = data
                .SelectMany(item => ((JObject)item).Properties())
                .Select(p => p.Name)
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            // Write headers
            for (int i = 0; i < properties.Count; i++)
            {
                var headerCell = worksheet.Cell(1, i + 1);
                headerCell.Value = properties[i];
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            // Write data
            for (int row = 0; row < data.Count; row++)
            {
                var item = (JObject)data[row];
                
                for (int col = 0; col < properties.Count; col++)
                {
                    var property = properties[col];
                    var value = item[property];
                    
                    if (value != null && value.Type != JTokenType.Null)
                    {
                        worksheet.Cell(row + 2, col + 1).Value = value.ToString();
                    }
                }
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();
            
            Console.WriteLine($"✓ Created sheet '{sheetName}' with {data.Count} rows");
        }

        // Ensure output directory exists
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        workbook.SaveAs(outputPath);
        
        Console.WriteLine($"\n✓ Excel file created: {outputPath}\n");
        
        return outputPath;
    }

    private string SanitizeSheetName(string name)
    {
        // Excel sheet names have restrictions:
        // - Max 31 characters
        // - Cannot contain: \ / ? * [ ]
        
        var invalidChars = new[] { '\\', '/', '?', '*', '[', ']' };
        var sanitized = name;
        
        foreach (var invalidChar in invalidChars)
        {
            sanitized = sanitized.Replace(invalidChar, '_');
        }
        
        if (sanitized.Length > 31)
        {
            sanitized = sanitized.Substring(0, 31);
        }
        
        return sanitized;
    }
}
