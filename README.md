# D365 Claude Analyzer

A .NET console application that extracts data from Microsoft Dynamics 365 Finance & Supply Chain (D365FSC) via OData and leverages Claude AI for intelligent data analysis.

## 🎯 What This Does

Think of this as your D365 data's new best friend who happens to be really good at math (and doesn't require coffee breaks). This tool:

1. **Connects to D365FSC** using OAuth 2.0 authentication (because security matters, even if reading the OAuth docs doesn't spark joy)
2. **Queries OData endpoints** to extract your business data
3. **Exports to Excel** with multiple worksheets (one per query, keeping things organized like Marie Kondo would approve)
4. **Sends to Claude AI** for intelligent analysis and insights
5. **Returns actionable insights** that actually make sense to humans

## 🏗️ Architecture

### Project Structure
```
D365ClaudeAnalyzer/
├── Models/
│   ├── ConfigModels.cs          # Configuration DTOs
│   ├── PromptModels.cs          # Prompt template models
│   └── ODataQuery.cs            # OData query builder
├── Services/
│   ├── D365AuthService.cs       # OAuth authentication with token caching
│   ├── D365ODataService.cs      # OData query execution
│   ├── ExcelExportService.cs    # Multi-sheet Excel generation
│   └── ClaudeApiService.cs      # Claude API integration
├── Helpers/
│   └── ConfigurationHelper.cs   # Configuration management
├── Program.cs                    # Main orchestration
├── App.config                    # Application settings
└── prompts.json                  # Claude prompt templates
```

## 🔧 Prerequisites

- **.NET 8.0 SDK** or later
- **D365 Finance & Supply Chain** environment with:
  - Azure AD app registration
  - Appropriate API permissions
  - Client credentials (tenant ID, client ID, client secret)
- **Claude API key** from Anthropic
- A sense of humor (optional, but recommended when dealing with OData)

## 📦 NuGet Packages

- **ClosedXML** (0.104.1) - Excel file creation and manipulation
- **Microsoft.Identity.Client** (4.61.3) - MSAL for Azure AD authentication
- **Newtonsoft.Json** (13.0.3) - JSON serialization
- **Microsoft.Extensions.Configuration** (8.0.0) - Configuration management
- **System.Configuration.ConfigurationManager** (8.0.0) - App.config support

## ⚙️ Configuration

### 1. App.config Setup

Update `App.config` with your credentials:

```xml
<!-- D365 Finance & Supply Chain Configuration -->
<add key="D365:TenantId" value="your-tenant-id-here" />
<add key="D365:ClientId" value="your-client-id-here" />
<add key="D365:ClientSecret" value="your-client-secret-here" />
<add key="D365:Resource" value="https://your-environment.operations.dynamics.com" />
<add key="D365:BaseUrl" value="https://your-environment.operations.dynamics.com" />

<!-- Claude API Configuration -->
<add key="Claude:ApiKey" value="your-claude-api-key-here" />
<add key="Claude:Model" value="claude-sonnet-4-20250514" />
<add key="Claude:MaxTokens" value="4096" />
```

**Available Claude Models:**
- `claude-sonnet-4-20250514` - Best quality analysis (recommended)
- `claude-haiku-4-20250514` - Faster, cost-effective option

### 2. Define Your OData Queries

Edit `Program.cs` to customize the queries for your specific D365 entities:

```csharp
var queries = new List<ODataQuery>
{
    new ODataQuery
    {
        SheetName = "Customers",
        EntityName = "CustomersV3",
        Select = "CustomerAccount,OrganizationName,PrimaryContactEmail",
        Top = 100
    },
    new ODataQuery
    {
        SheetName = "Sales_Orders",
        EntityName = "SalesOrderHeadersV2",
        Select = "SalesOrderNumber,TotalAmount,CurrencyCode",
        Filter = $"RequestedReceiptDate ge {DateTime.UtcNow.AddDays(-30):yyyy-MM-dd}",
        OrderBy = "RequestedReceiptDate desc",
        Top = 500
    }
};
```

**OData Query Properties:**
- `SheetName` - Name of the Excel worksheet
- `EntityName` - D365 OData entity name (e.g., "CustomersV3", "SalesOrderHeadersV2")
- `Select` - Comma-separated list of fields to retrieve
- `Filter` - OData filter expression (optional)
- `OrderBy` - Sort order (optional)
- `Top` - Maximum number of records (optional)

### 3. Customize Prompts

Edit `prompts.json` to add or modify analysis prompts:

```json
{
  "prompts": [
    {
      "id": "custom-analysis",
      "name": "My Custom Analysis",
      "systemPrompt": "You are a D365 expert specializing in...",
      "userPrompt": "Analyze this data and tell me..."
    }
  ]
}
```

## 🚀 Usage

### Build and Run

```bash
# Restore packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

### Execution Flow

1. **Configuration Validation** - Checks all required settings
2. **D365 Authentication** - Obtains OAuth token
3. **Data Extraction** - Executes all defined OData queries
4. **Excel Export** - Creates multi-sheet workbook
5. **Prompt Selection** - Choose analysis type
6. **Claude Analysis** - Sends data to Claude for insights
7. **Results Display** - Shows analysis in console and saves to file

## 📊 Output Files

The application creates the following outputs:

- **Excel File**: `./output/D365_Data_Export.xlsx`
  - One worksheet per OData query
  - Headers with bold formatting
  - Auto-fitted columns
  
- **Analysis File**: `./output/Analysis_[timestamp].txt`
  - Claude's analysis results
  - Timestamped for version control

## 🎨 Best Practices Implemented

### Token Optimization
- **Selective field retrieval**: Only fetch needed columns via `$select`
- **Result limiting**: Use `$top` to cap record counts
- **Efficient encoding**: Base64 encoding minimizes overhead
- **Token usage reporting**: Monitor input/output tokens for optimization

### Error Handling
- Comprehensive try-catch blocks
- Detailed error messages with context
- Configuration validation upfront
- HTTP response validation

### Security
- Token caching with expiration management
- Credentials stored in config (consider Azure Key Vault for production)
- HTTPS-only communication

### Code Quality
- Strongly-typed configuration models
- Dependency injection pattern
- Single Responsibility Principle
- Async/await throughout for scalability

## 🔍 Common D365 Entity Names

Here are some commonly used D365FSC OData entities to get you started:

| Entity Name | Description |
|------------|-------------|
| `CustomersV3` | Customer master data |
| `VendorsV2` | Vendor master data |
| `SalesOrderHeadersV2` | Sales order headers |
| `SalesOrderLinesV2` | Sales order line details |
| `PurchaseOrderHeadersV2` | Purchase order headers |
| `InventoryOnHandV2` | On-hand inventory quantities |
| `ProductsV2` | Product master data |
| `LedgerJournalHeaderEntity` | General ledger journal headers |
| `WorkOrdersV2` | Work order details |

**Pro Tip**: Use the D365 OData metadata endpoint to discover available entities:
```
https://your-environment.operations.dynamics.com/data/$metadata
```

## 🐛 Troubleshooting

### "Failed to authenticate with D365"
- Verify tenant ID, client ID, and client secret
- Ensure the Azure AD app has proper API permissions
- Check that the resource URL matches your environment

### "OData query failed"
- Verify entity name exists (check $metadata)
- Ensure selected fields are valid for that entity
- Check OData filter syntax

### "Claude API request failed"
- Validate API key
- Check network connectivity
- Ensure file size is within Claude's limits (~32MB for documents)

### Excel File Won't Open
- Check that ClosedXML is properly referenced
- Ensure output directory has write permissions
- Verify no special characters in sheet names

## 🚀 Future Enhancements

Some ideas for extending this solution:

- **Interactive Mode**: Allow runtime query customization
- **Scheduled Execution**: Add timer-based recurring analysis
- **Dashboard Integration**: Push results to Power BI or custom dashboards
- **Email Notifications**: Send analysis results via email
- **Multiple File Support**: Attach reference documents alongside data
- **Conversation Mode**: Multi-turn analysis with Claude for deeper insights
- **Azure Key Vault**: Secure credential management

## 📝 Notes

- **Token Costs**: Monitor Claude API usage; Sonnet 4 is powerful but costs more than Haiku
- **Rate Limits**: D365 OData has throttling limits; implement retry logic for production
- **Data Volume**: Large datasets should be paginated or filtered appropriately
- **Time Zones**: D365 dates are typically UTC; adjust filters accordingly

## 🤝 Contributing

Got ideas? Found a bug? Want to add support for D365 Sales or Customer Service? Contributions are welcome!

## 📄 License

This is a sample project - use it, modify it, break it, fix it. No warranties, but plenty of good intentions.

---

**Remember**: Good code is like a good joke - it doesn't need explaining. But documentation helps anyway. 😉
