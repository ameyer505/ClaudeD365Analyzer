# Technical Architecture & Design Decisions

## Overview

This document explains the technical architecture, design decisions, and best practices implemented in the D365 Claude Analyzer solution.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         Console Application                      │
│                            (Program.cs)                          │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           │ Orchestrates
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
        ▼                  ▼                  ▼
┌───────────────┐  ┌───────────────┐  ┌───────────────┐
│ D365 Services │  │ Excel Service │  │ Claude Service│
│               │  │               │  │               │
│ • Auth        │  │ • Export      │  │ • Analysis    │
│ • OData Query │  │ • Format      │  │ • Prompts     │
└───────┬───────┘  └───────┬───────┘  └───────┬───────┘
        │                  │                  │
        ▼                  ▼                  ▼
┌───────────────┐  ┌───────────────┐  ┌───────────────┐
│   Azure AD    │  │  ClosedXML    │  │ Anthropic API │
│   (MSAL)      │  │  Library      │  │               │
└───────────────┘  └───────────────┘  └───────────────┘
```

## Technology Stack

### Framework & Language
- **.NET 8.0**: Latest LTS version with performance improvements
- **C# 12**: Leveraging modern language features (nullable reference types, pattern matching)

### Key Libraries

| Library | Version | Purpose | Why Chosen |
|---------|---------|---------|------------|
| **Microsoft.Identity.Client** | 4.61.3 | Azure AD Authentication | Official Microsoft MSAL library, handles token acquisition/refresh |
| **ClosedXML** | 0.102.2 | Excel Generation | Easy-to-use, no Excel installation required, supports advanced formatting |
| **Newtonsoft.Json** | 13.0.3 | JSON Serialization | Mature, reliable, better control over serialization than System.Text.Json for complex scenarios |
| **System.Configuration.ConfigurationManager** | 8.0.0 | App.config Management | Familiar configuration pattern, separation of secrets |

### Why These Choices?

**MSAL over custom auth**: Handles token caching, refresh, and various authentication flows automatically

**ClosedXML over EPPlus**: More permissive license, better API for complex formatting, no licensing concerns

**Newtonsoft.Json over System.Text.Json**: Better handling of dynamic objects from OData, more mature for our use case

**App.config over appsettings.json**: Better for console apps, familiar to .NET Framework developers migrating code

## Design Patterns & Principles

### Separation of Concerns

Each service class has a single, well-defined responsibility:

```
D365AuthenticationService  → Authentication only
D365DataService           → OData queries only
ExcelExportService        → Excel generation only
ClaudeService             → AI API calls only
PromptService             → Prompt management only
```

**Benefits**:
- Easy to test individual components
- Simple to replace implementations (e.g., swap Excel library)
- Clear where to make changes for specific functionality

### Configuration-Driven Design

Critical settings are externalized:
- **App.config**: Credentials, URLs, model selection
- **prompts.json**: Analysis templates
- **Program.cs**: OData query definitions

**Benefits**:
- No recompilation for configuration changes
- Secrets can be managed separately (Azure Key Vault, etc.)
- Easy to create multiple configurations for different environments

### Fail Fast Philosophy

The application validates configuration at startup:

```csharp
_tenantId = ConfigurationManager.AppSettings["D365:TenantId"] 
    ?? throw new ConfigurationErrorsException("D365:TenantId not found");
```

**Benefits**:
- Immediate feedback on misconfiguration
- Prevents partial execution with missing credentials
- Clear error messages for troubleshooting

## Token Optimization Strategies

### 1. Selective Field Retrieval

```csharp
SelectFields = "ItemId,ItemName,Price"  // Only what we need
```

**Impact**: Can reduce payload by 70-90% compared to retrieving all fields

### 2. Smart Filtering

```csharp
FilterCriteria = "CreatedDateTime gt 2024-01-01"  // Recent data only
```

**Impact**: Focuses analysis on relevant data, reduces noise in Claude's analysis

### 3. Result Limiting

```csharp
Top = 1000  // Cap result set
```

**Impact**: Prevents token overflow, keeps analysis focused

### 4. Base64 Encoding Efficiency

Excel files are sent as base64, which is ~33% larger than binary but necessary for API transport.

**Token estimate**: `base64_bytes / 4 ≈ input_tokens`

Example:
- 100 KB Excel → ~133 KB base64 → ~33,000 tokens
- 1 MB Excel → ~1.3 MB base64 → ~330,000 tokens

### 5. Structured Prompts

Specific prompts yield focused responses:
```
Bad:  "Analyze this data"
Good: "Identify top 5 items at risk of stockout and recommend reorder quantities"
```

**Impact**: Shorter, more actionable responses = fewer output tokens

## Security Considerations

### Credential Management

**Current Design** (suitable for development/POC):
```
App.config (local file) → Services
```

**Production Recommendation**:
```
Azure Key Vault → Configuration → Services
```

### Sensitive Data Handling

- API keys and secrets are never logged
- Excel files contain business data - consider encryption at rest
- Claude API calls are over HTTPS (TLS 1.2+)
- No sensitive data in exception messages

### Principle of Least Privilege

Azure AD app registration should have:
- **Read-only** access to D365FSC
- **Specific entity** permissions, not wildcard
- **No write** permissions unless explicitly needed

## Error Handling Strategy

### Three-Tier Approach

**1. Service Level**: Catch and log specific errors
```csharp
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
    throw; // Re-throw for higher level handling
}
```

**2. Operation Level**: Handle recoverable errors
```csharp
try {
    var data = await QueryODataAsync(query);
} catch {
    // Add empty dataset, continue processing other queries
    dataByWorksheet[query.WorksheetName] = new List();
}
```

**3. Application Level**: Fatal errors stop execution
```csharp
catch (Exception ex)
{
    Console.WriteLine($"Fatal Error: {ex.Message}");
    Environment.Exit(1);
}
```

### User-Friendly Error Messages

Technical errors are translated for business users:
```
❌ "HttpRequestException: 401 Unauthorized"
✅ "Authentication failed. Please verify your credentials in App.config"
```

## Performance Considerations

### Async/Await Throughout

All I/O operations use async:
```csharp
await authService.GetAccessTokenAsync();
await dataService.QueryODataAsync();
await File.ReadAllBytesAsync();
```

**Benefits**:
- Non-blocking operations
- Better resource utilization
- Scalable to parallel queries (future enhancement)

### HttpClient Reuse

Single `HttpClient` instance per service (not per request):
```csharp
private readonly HttpClient _httpClient = new HttpClient();
```

**Benefits**:
- Avoids socket exhaustion
- Connection pooling
- Better performance

### Memory Efficiency

Large Excel files are read once and encoded:
```csharp
var excelBytes = await File.ReadAllBytesAsync(path);
var base64Excel = Convert.ToBase64String(excelBytes);
```

Then sent to Claude. No intermediate copies or string manipulations.

## Extensibility Points

### Adding New Services

Easy to add new functionality:

```csharp
// New service for Power BI integration
public class PowerBIService
{
    public async Task PublishDatasetAsync(string excelPath) { }
}
```

### Custom Data Sources

The architecture supports additional data sources:

```csharp
// Add SQL Server queries
public class SqlServerDataService
{
    public async Task<DataTable> QueryAsync(string query) { }
}
```

### Alternative AI Providers

The Claude service interface can be abstracted:

```csharp
public interface IAIAnalysisService
{
    Task<string> AnalyzeDataAsync(string filePath, PromptTemplate prompt);
}

// Implement for different providers
public class OpenAIService : IAIAnalysisService { }
public class ClaudeService : IAIAnalysisService { }
```

## Testing Strategy

### Manual Testing

Current implementation focuses on:
- Configuration validation
- API connectivity
- Data transformation
- Error scenarios

### Future Enhancements

Recommended test structure:

```
Tests/
├── Unit/
│   ├── AuthenticationTests.cs
│   ├── ODataQueryBuilderTests.cs
│   └── ExcelExportTests.cs
├── Integration/
│   ├── D365ConnectionTests.cs
│   └── ClaudeAPITests.cs
└── E2E/
    └── FullWorkflowTests.cs
```

## Known Limitations & Trade-offs

### Synchronous Query Execution

**Current**: Queries execute sequentially  
**Trade-off**: Simple, predictable, but slower for many queries  
**Future**: Parallel execution with `Task.WhenAll()`

### In-Memory Data Processing

**Current**: All data loaded into memory  
**Trade-off**: Simple, fast for moderate datasets  
**Limitation**: Large datasets (millions of rows) may cause issues  
**Future**: Streaming or batched processing

### Single Excel File Output

**Current**: All worksheets in one workbook  
**Trade-off**: Convenient, but large files increase Claude token usage  
**Future**: Option to split into multiple files

### Configuration in Code

**Current**: OData queries defined in `Program.cs`  
**Trade-off**: Type-safe, compile-time checking  
**Limitation**: Requires recompilation to change queries  
**Future**: External query definition file (JSON/YAML)

## Deployment Considerations

### Console Application

**Strengths**:
- Simple deployment (xcopy)
- Easy scheduling (Task Scheduler, cron)
- No web server required

**Considerations**:
- Requires .NET 8 runtime on target machine
- Console output may not be suitable for all environments

### Alternative Deployments

**Azure Function** (serverless):
```
Triggers: Timer, HTTP, Queue Message
Benefits: Automatic scaling, no infrastructure
```

**Windows Service**:
```
Benefits: Runs continuously, automatic recovery
Use case: Real-time monitoring scenarios
```

**Docker Container**:
```
Benefits: Consistent environment, easy deployment
Use case: Cloud-native deployments
```

## Maintenance & Operations

### Logging Best Practices

Current: Console output  
Production recommendation:
```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/d365-analyzer-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

### Monitoring

Key metrics to track:
- Query execution time
- Token usage per analysis
- API error rates
- Data quality issues (empty results, null values)

### Secrets Rotation

Regular maintenance tasks:
- Azure AD client secret (expires)
- Claude API key (rotate quarterly)
- D365FSC credentials (per security policy)

## Cost Optimization

### Claude API Usage

**Sonnet 4**: $3 per million input tokens, $15 per million output tokens

**Example calculation**:
- 100 KB Excel file = ~33K tokens = $0.10 input
- 2K token response = $0.03 output
- **Total per analysis**: ~$0.13

**Monthly usage** (daily runs):
- 30 analyses = ~$4/month
- 100 analyses = ~$13/month

### D365FSC API Calls

- Generally included in D365 license
- Monitor for throttling limits
- Consider caching for repeated queries

## Future Enhancements

### Short Term
1. Parallel query execution
2. Query result caching
3. Better error recovery (retry logic)
4. Output format options (PDF, CSV)

### Medium Term
1. Web dashboard for viewing results
2. Scheduled analysis (cron/Task Scheduler)
3. Historical trend comparison
4. Alert system for anomalies

### Long Term
1. Multi-tenant support
2. Query builder UI
3. Custom AI model fine-tuning
4. Real-time streaming analysis

---

## Questions or Improvements?

This architecture is designed to be maintainable and extensible. If you identify areas for improvement or have questions about design decisions, document them here for future reference.
