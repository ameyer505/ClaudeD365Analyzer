using D365ClaudeAnalyzer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace D365ClaudeAnalyzer.Services
{
    /// <summary>
    /// Handles OData queries to D365 Finance & Supply Chain
    /// OData: Because sometimes you just want your data served with a side of REST
    /// </summary>
    public class D365DataService
    {
        private readonly D365AuthenticationService _authService;
        private readonly HttpClient _httpClient;

        public D365DataService(D365AuthenticationService authService)
        {
            _authService = authService;
            _httpClient = new HttpClient();
        }

        public async Task<List<Dictionary<string, object>>> QueryODataAsync(ODataQueryConfiguration queryConfig)
        {
            try
            {
                var accessToken = await _authService.GetAccessTokenAsync();
                var baseUrl = _authService.GetResourceUrl();

                // Build the full OData URL
                var odataUrl = BuildODataUrl(baseUrl, queryConfig);

                Console.WriteLine($"  Querying: {queryConfig.EntityName}");
                Console.WriteLine($"  URL: {odataUrl}");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                _httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                _httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");

                var response = await _httpClient.GetAsync(odataUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException(
                        $"OData query failed with status {response.StatusCode}: {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(content);
                var values = jsonResponse["value"];

                if (values == null)
                {
                    Console.WriteLine($"  ⚠ No data returned for {queryConfig.EntityName}");
                    return new List<Dictionary<string, object>>();
                }

                // Convert JArray to list of dictionaries for easier Excel processing
                var results = values.ToObject<List<Dictionary<string, object>>>() 
                    ?? new List<Dictionary<string, object>>();

                Console.WriteLine($"  ✓ Retrieved {results.Count} records");
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Error querying {queryConfig.EntityName}: {ex.Message}");
                throw;
            }
        }

        private string BuildODataUrl(string baseUrl, ODataQueryConfiguration config)
        {
            // Start with base URL and entity
            var url = $"{baseUrl.TrimEnd('/')}/data/{config.EntityName}";

            var queryParams = new List<string>();

            // Add $select if specified
            if (!string.IsNullOrWhiteSpace(config.SelectFields))
            {
                queryParams.Add($"$select={config.SelectFields}");
            }

            // Add $filter if specified
            if (!string.IsNullOrWhiteSpace(config.FilterCriteria))
            {
                queryParams.Add($"$filter={config.FilterCriteria}");
            }

            // Add $orderby if specified
            if (!string.IsNullOrWhiteSpace(config.OrderBy))
            {
                queryParams.Add($"$orderby={config.OrderBy}");
            }

            // Add $top if specified
            if (config.Top.HasValue)
            {
                queryParams.Add($"$top={config.Top.Value}");
            }

            // Combine all query parameters
            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams);
            }

            return url;
        }
    }
}
