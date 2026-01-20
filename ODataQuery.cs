namespace D365ClaudeAnalyzer.Models;

public class ODataQuery
{
    public string SheetName { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? Filter { get; set; }
    public string? Select { get; set; }
    public string? OrderBy { get; set; }
    public int? Top { get; set; }
    
    public string BuildQueryString()
    {
        var queryParts = new List<string>();
        
        if (!string.IsNullOrEmpty(Select))
            queryParts.Add($"$select={Select}");
        
        if (!string.IsNullOrEmpty(Filter))
            queryParts.Add($"$filter={Filter}");
        
        if (!string.IsNullOrEmpty(OrderBy))
            queryParts.Add($"$orderby={OrderBy}");
        
        if (Top.HasValue)
            queryParts.Add($"$top={Top.Value}");
        
        return queryParts.Count > 0 ? "?" + string.Join("&", queryParts) : string.Empty;
    }
}
