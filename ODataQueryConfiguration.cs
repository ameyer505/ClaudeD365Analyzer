namespace D365ClaudeAnalyzer.Models
{
    /// <summary>
    /// Represents a single OData query configuration for D365FSC
    /// </summary>
    public class ODataQueryConfiguration
    {
        public string EntityName { get; set; } = string.Empty;
        public string WorksheetName { get; set; } = string.Empty;
        public string ODataQuery { get; set; } = string.Empty;
        public string? SelectFields { get; set; }
        public string? FilterCriteria { get; set; }
        public string? OrderBy { get; set; }
        public int? Top { get; set; }
    }

    /// <summary>
    /// Container for multiple OData queries
    /// Yeah, I know it's a simple list wrapper, but it keeps things organized. 
    /// Plus, you never know when you'll need to add query metadata later!
    /// </summary>
    public class ODataQueryCollection
    {
        public List<ODataQueryConfiguration> Queries { get; set; } = new();
    }
}
