using Microsoft.Identity.Client;
using System.Configuration;

namespace D365ClaudeAnalyzer.Services
{
    /// <summary>
    /// Handles authentication to D365 Finance & Supply Chain using MSAL
    /// Because nobody likes authentication code, but everybody needs it!
    /// </summary>
    public class D365AuthenticationService
    {
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _resourceUrl;
        private readonly string _authority;

        public D365AuthenticationService()
        {
            _tenantId = ConfigurationManager.AppSettings["D365:TenantId"] 
                ?? throw new ConfigurationErrorsException("D365:TenantId not found in configuration");
            _clientId = ConfigurationManager.AppSettings["D365:ClientId"] 
                ?? throw new ConfigurationErrorsException("D365:ClientId not found in configuration");
            _clientSecret = ConfigurationManager.AppSettings["D365:ClientSecret"] 
                ?? throw new ConfigurationErrorsException("D365:ClientSecret not found in configuration");
            _resourceUrl = ConfigurationManager.AppSettings["D365:ResourceUrl"] 
                ?? throw new ConfigurationErrorsException("D365:ResourceUrl not found in configuration");
            _authority = ConfigurationManager.AppSettings["D365:Authority"] 
                ?? throw new ConfigurationErrorsException("D365:Authority not found in configuration");
        }

        public async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var app = ConfidentialClientApplicationBuilder
                    .Create(_clientId)
                    .WithClientSecret(_clientSecret)
                    .WithAuthority(new Uri($"{_authority}{_tenantId}"))
                    .Build();

                var scopes = new[] { $"{_resourceUrl}/.default" };
                var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

                Console.WriteLine($"✓ Successfully authenticated to D365FSC");
                return result.AccessToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Authentication failed: {ex.Message}");
                throw;
            }
        }

        public string GetResourceUrl() => _resourceUrl;
    }
}
