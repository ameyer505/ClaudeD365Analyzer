using Microsoft.Identity.Client;

namespace D365ClaudeAnalyzer.Services;

public class D365AuthService
{
    private readonly string _tenantId;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _resource;
    private string? _cachedToken;
    private DateTime _tokenExpiration;

    public D365AuthService(string tenantId, string clientId, string clientSecret, string resource)
    {
        _tenantId = tenantId;
        _clientId = clientId;
        _clientSecret = clientSecret;
        _resource = resource;
        _tokenExpiration = DateTime.MinValue;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        // Return cached token if still valid (with 5-minute buffer)
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiration.AddMinutes(-5))
        {
            return _cachedToken;
        }

        try
        {
            var authority = $"https://login.microsoftonline.com/{_tenantId}";
            var app = ConfidentialClientApplicationBuilder
                .Create(_clientId)
                .WithClientSecret(_clientSecret)
                .WithAuthority(new Uri(authority))
                .Build();

            var scopes = new[] { $"{_resource}/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            _cachedToken = result.AccessToken;
            _tokenExpiration = result.ExpiresOn.UtcDateTime;

            Console.WriteLine($"✓ Successfully authenticated with D365 (Token expires: {_tokenExpiration:yyyy-MM-dd HH:mm:ss} UTC)");
            return _cachedToken;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to authenticate with D365: {ex.Message}", ex);
        }
    }
}
