# Setup Guide for D365 Claude Analyzer

This guide walks you through setting up the D365 Claude Analyzer from scratch. Even if you've configured D365 integrations before, there might be a gotcha or two (because D365 likes to keep us on our toes).

## Prerequisites Checklist

- [ ] Visual Studio 2022 or VS Code with C# extensions
- [ ] .NET 8.0 SDK installed
- [ ] Access to a D365 Finance & Supply Chain environment
- [ ] Azure AD admin access (or a friendly admin who owes you a favor)
- [ ] Claude API key from Anthropic
- [ ] Coffee (or your preferred debugging beverage)

## Part 1: Azure AD App Registration

### Step 1: Create App Registration

1. Navigate to [Azure Portal](https://portal.azure.com)
2. Go to **Azure Active Directory** → **App registrations**
3. Click **New registration**
4. Configure:
   - **Name**: `D365-Claude-Analyzer` (or something equally creative)
   - **Supported account types**: "Accounts in this organizational directory only"
   - **Redirect URI**: Leave blank (we're using client credentials)
5. Click **Register**

### Step 2: Note Your IDs

After registration, copy these values (you'll need them for App.config):

- **Application (client) ID**: Found on the Overview page
- **Directory (tenant) ID**: Also on the Overview page

### Step 3: Create Client Secret

1. Go to **Certificates & secrets**
2. Click **New client secret**
3. Add description: "D365 Claude Analyzer Secret"
4. Choose expiration (1 year is typical)
5. Click **Add**
6. **IMMEDIATELY** copy the secret value (you won't see it again!)

### Step 4: Grant API Permissions

1. Go to **API permissions**
2. Click **Add a permission**
3. Select **APIs my organization uses**
4. Search for "Dynamics 365" and select it
5. Choose **Delegated permissions** or **Application permissions** based on your needs
6. Select the permission you need (typically `user_impersonation`)
7. Click **Add permissions**
8. Click **Grant admin consent** (this is the important part!)

## Part 2: D365 Configuration

### Step 1: Register Azure AD Application in D365

1. Log into your D365 environment
2. Navigate to **System administration** → **Setup** → **Azure Active Directory applications**
3. Click **New**
4. Enter:
   - **Client ID**: Your Application (client) ID from Azure
   - **Name**: Descriptive name (e.g., "Claude Analyzer")
   - **User ID**: The D365 user account this app will impersonate
5. Save

### Step 2: Assign Security Roles

The service account needs appropriate permissions:

1. Go to **System administration** → **Security** → **Users**
2. Find the user you specified above
3. Assign appropriate security roles (depends on what data you're querying)
   - For read-only access: Consider custom role with read permissions
   - For broader access: System Administrator (not recommended for production)

### Step 3: Verify OData Endpoint Access

Test your environment's OData endpoint in a browser:

```
https://your-environment.operations.dynamics.com/data/$metadata
```

You should see XML metadata (it's not exciting, but it means it's working).

## Part 3: Claude API Setup

### Step 1: Get API Key

1. Go to [Anthropic Console](https://console.anthropic.com/)
2. Sign up or log in
3. Navigate to **API Keys**
4. Click **Create Key**
5. Give it a name: "D365 Analyzer"
6. Copy the key (starts with `sk-ant-api03-`)

### Step 2: Understand Pricing

Claude API pricing (as of Jan 2025):
- **Sonnet 4**: Higher cost, better analysis
- **Haiku 4**: Lower cost, faster responses

Calculate your costs:
- Input tokens: ~$3 per million tokens
- Output tokens: ~$15 per million tokens
- An Excel file with 1000 rows ≈ 50-100K tokens

**Pro Tip**: Start with small datasets while testing!

## Part 4: Project Configuration

### Step 1: Clone/Download Project

```bash
git clone <repository-url>
cd D365ClaudeAnalyzer
```

### Step 2: Configure App.config

1. Copy the template:
   ```bash
   cp App.config.template App.config
   ```

2. Edit `App.config` with your values:
   ```xml
   <add key="D365:TenantId" value="your-tenant-id-from-azure" />
   <add key="D365:ClientId" value="your-client-id-from-azure" />
   <add key="D365:ClientSecret" value="your-client-secret-from-azure" />
   <add key="D365:Resource" value="https://your-env.operations.dynamics.com" />
   <add key="D365:BaseUrl" value="https://your-env.operations.dynamics.com" />
   <add key="Claude:ApiKey" value="sk-ant-api03-your-key-here" />
   ```

### Step 3: Customize OData Queries

Edit `Program.cs` and find the queries section:

```csharp
var queries = new List<ODataQuery>
{
    new ODataQuery
    {
        SheetName = "Your_Data",
        EntityName = "YourEntityName",  // Change this!
        Select = "Field1,Field2,Field3", // And this!
        Top = 100
    }
};
```

**Finding Entity Names**:
- Check D365 documentation
- Use the $metadata endpoint
- Common entities are listed in README.md

### Step 4: Customize Prompts (Optional)

Edit `prompts.json` to add domain-specific analysis:

```json
{
  "id": "my-custom-analysis",
  "name": "My Analysis",
  "systemPrompt": "You are an expert in [your domain]...",
  "userPrompt": "Analyze this data focusing on [specific aspects]..."
}
```

## Part 5: Build and Test

### Step 1: Restore Packages

```bash
dotnet restore
```

### Step 2: Build

```bash
dotnet build
```

If you see build errors:
- Check .NET SDK version: `dotnet --version` (should be 8.0+)
- Ensure all NuGet packages restored successfully

### Step 3: Run Initial Test

```bash
dotnet run
```

Expected output:
```
═══════════════════════════════════════════════════════════
   D365 Finance & Supply Chain → Claude AI Analyzer
═══════════════════════════════════════════════════════════

Validating configuration...

✓ D365 configuration loaded
✓ Claude configuration loaded
✓ Output configuration loaded
✓ Loaded 3 prompt templates
```

## Part 6: Troubleshooting

### Problem: "Failed to authenticate with D365"

**Possible Causes**:
1. Wrong tenant/client ID
2. Client secret expired or incorrect
3. App not registered in D365
4. Admin consent not granted

**Solution**:
- Verify all IDs in Azure Portal
- Regenerate client secret if needed
- Check Azure AD applications in D365
- Grant admin consent in Azure Portal

### Problem: "OData query failed: 404"

**Possible Causes**:
1. Wrong entity name
2. Entity doesn't exist in your environment
3. Wrong base URL

**Solution**:
- Check $metadata endpoint for available entities
- Verify D365 environment URL
- Ensure entity exists in your version of D365

### Problem: "OData query failed: 403 Forbidden"

**Possible Causes**:
1. Service account lacks permissions
2. Security role not assigned correctly

**Solution**:
- Check user security roles in D365
- Verify user has read access to the entity
- Review data security policies

### Problem: "Claude API request failed: 401"

**Possible Causes**:
1. Invalid API key
2. API key not active

**Solution**:
- Verify API key in Anthropic console
- Regenerate key if needed
- Check for typos in App.config

### Problem: Excel file is huge and Claude times out

**Solution**:
- Reduce `$top` value in queries
- Use `$select` to limit columns
- Add more specific `$filter` clauses
- Consider processing data in batches

## Part 7: Production Considerations

### Security
- [ ] Move secrets to Azure Key Vault
- [ ] Use managed identity instead of client secrets
- [ ] Implement proper logging (not console output)
- [ ] Add audit trail for data access

### Performance
- [ ] Implement retry logic with exponential backoff
- [ ] Add caching for D365 queries
- [ ] Batch large result sets
- [ ] Monitor Claude API rate limits

### Reliability
- [ ] Add comprehensive error handling
- [ ] Implement circuit breaker pattern
- [ ] Log errors to Application Insights or similar
- [ ] Add health checks

### Monitoring
- [ ] Track Claude API token usage
- [ ] Monitor D365 API throttling
- [ ] Alert on authentication failures
- [ ] Dashboard for execution metrics

## Part 8: Next Steps

Now that you have it working:

1. **Test with real data**: Start with small datasets
2. **Refine prompts**: Experiment with different analysis angles
3. **Optimize queries**: Focus on data you actually need
4. **Automate**: Consider scheduled execution
5. **Share insights**: Export to Power BI or other tools

## Need Help?

Common resources:
- [D365 OData Documentation](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/data-entities/odata)
- [Claude API Documentation](https://docs.anthropic.com/claude/reference/getting-started-with-the-api)
- [MSAL.NET Documentation](https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-overview)

Remember: When in doubt, check the logs. And if that doesn't help, check them again with fresh coffee. ☕

---

**Pro Tip**: Save this guide! You'll need it again when you inevitably have to set this up on another machine in 6 months and have forgotten all the steps. (Don't worry, we've all been there.)
