using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using D365ClaudeAnalyzer.Models;

namespace D365ClaudeAnalyzer.Services;

public class D365ODataService
{
    private readonly D365AuthService _authService;
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;

    public D365ODataService(D365AuthService authService, string baseUrl)
    {
        _authService = authService;
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient();
    }

    public async Task<JArray> ExecuteQueryAsync(ODataQuery query)
    {
        var token = await _authService.GetAccessTokenAsync();
        
        var queryUrl = $"{_baseUrl}/data/{query.EntityName}{query.BuildQueryString()}";
        
        Console.WriteLine($"Querying: {query.EntityName}...");
        
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"OData query failed for {query.EntityName}: {response.StatusCode} - {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(content);
            
            var values = jsonResponse["value"] as JArray ?? new JArray();
            
            Console.WriteLine($"✓ Retrieved {values.Count} records from {query.EntityName}");
            
            return values;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error querying {query.EntityName}: {ex.Message}");
            throw;
        }
    }

    public async Task<List<(string SheetName, JArray Data)>> ExecuteMultipleQueriesAsync(List<ODataQuery> queries)
    {
        var results = new List<(string, JArray)>();
        
        Console.WriteLine($"\nExecuting {queries.Count} OData queries...\n");
        
        foreach (var query in queries)
        {
            var data = await ExecuteQueryAsync(query);
            results.Add((query.SheetName, data));
        }
        
        Console.WriteLine($"\n✓ All queries completed successfully\n");
        
        return results;
    }
}
