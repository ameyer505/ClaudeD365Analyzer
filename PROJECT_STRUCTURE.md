# Project Structure

Complete file listing and description of the D365 Claude Analyzer solution.

## Solution Files

```
D365ClaudeAnalyzer/
│
├── 📄 D365ClaudeAnalyzer.csproj    # Project file with dependencies
├── 📄 Program.cs                    # Main entry point and orchestration
├── 📄 App.config                    # Runtime configuration with credentials
├── 📄 App.config.template           # Template for App.config (safe to commit)
├── 📄 prompts.json                  # Claude AI prompt templates
├── 📄 .gitignore                    # Git ignore rules
│
├── 📚 Documentation
│   ├── README.md                    # Main documentation
│   ├── SETUP.md                     # Quick setup guide
│   ├── EXAMPLES.md                  # Real-world usage scenarios
│   ├── ARCHITECTURE.md              # Technical architecture details
│   └── PROJECT_STRUCTURE.md         # This file
│
├── 📁 Models/                       # Data models and DTOs
│   ├── ClaudeModels.cs             # Claude API request/response models
│   └── ODataQueryConfiguration.cs  # OData query definition models
│
└── 📁 Services/                     # Business logic services
    ├── D365AuthenticationService.cs # Azure AD authentication
    ├── D365DataService.cs           # OData query execution
    ├── ExcelExportService.cs        # Excel file generation
    ├── ClaudeService.cs             # Claude API integration
    └── PromptService.cs             # Prompt template management
```

## File Descriptions

### Core Application Files

#### `Program.cs` (286 lines)
**Purpose**: Main application orchestrator  
**Key Responsibilities**:
- Initialize all services
- Define OData queries
- Orchestrate the ETL pipeline (Extract → Transform → Load)
- Handle user interaction
- Display results

**Key Method**: `GetODataQueries()` - Define your data extraction queries here

#### `D365ClaudeAnalyzer.csproj` (28 lines)
**Purpose**: .NET project definition  
**Contains**:
- Target framework (net8.0)
- Package references
- Build configuration

**Key Dependencies**:
- Microsoft.Identity.Client (Auth)
- ClosedXML (Excel)
- Newtonsoft.Json (JSON)
- System.Configuration.ConfigurationManager (Config)

### Configuration Files

#### `App.config` (30 lines)
**Purpose**: Runtime configuration  
**Contains**:
- D365FSC credentials (Tenant ID, Client ID, Secret)
- D365FSC instance URL
- Claude API key and settings
- Output file configuration

**⚠️ SECURITY**: Contains secrets - DO NOT commit to source control

#### `App.config.template` (30 lines)
**Purpose**: Template for App.config  
**Safe to commit**: Contains placeholder values  
**Usage**: Copy to App.config and fill in real values

#### `prompts.json` (55 lines)
**Purpose**: Claude AI analysis prompt templates  
**Contains**:
- System prompts (define Claude's role)
- User prompts (specific analysis instructions)
- Pre-built templates for common scenarios

**Customization**: Add your own prompts for specific analysis needs

#### `.gitignore` (90 lines)
**Purpose**: Git ignore rules  
**Protects**:
- Build outputs
- Configuration files with secrets
- Generated Excel files
- IDE-specific files

### Model Classes

#### `Models/ClaudeModels.cs` (64 lines)
**Purpose**: Data models for Claude API  
**Classes**:
- `ClaudeRequest` - API request structure
- `ClaudeMessage` - Message format
- `ClaudeContent` - Content blocks (text, documents)
- `ClaudeResponse` - API response structure
- `ClaudeUsage` - Token usage tracking
- `PromptConfiguration` - Prompt template structure
- `PromptTemplate` - Individual prompt definition

**Key Feature**: Supports document attachment via base64 encoding

#### `Models/ODataQueryConfiguration.cs` (26 lines)
**Purpose**: OData query definition models  
**Classes**:
- `ODataQueryConfiguration` - Single query definition
- `ODataQueryCollection` - Multiple query container

**Properties**:
- EntityName - D365FSC entity to query
- WorksheetName - Excel sheet name for results
- SelectFields - Fields to retrieve
- FilterCriteria - OData filter expression
- OrderBy - Sort order
- Top - Max records to retrieve

### Service Classes

#### `Services/D365AuthenticationService.cs` (58 lines)
**Purpose**: Azure AD authentication  
**Responsibilities**:
- Acquire OAuth2 tokens using client credentials flow
- Cache tokens (handled by MSAL)
- Provide resource URL to other services

**Key Method**: `GetAccessTokenAsync()` - Returns valid access token

**Uses**: Microsoft Identity Client (MSAL) library

#### `Services/D365DataService.cs` (108 lines)
**Purpose**: OData query execution  
**Responsibilities**:
- Build OData URLs with query parameters
- Execute HTTP requests to D365FSC
- Parse JSON responses
- Convert to strongly-typed data

**Key Method**: `QueryODataAsync()` - Execute a single query  
**Returns**: List of dictionaries (flexible structure)

**Features**:
- Automatic token inclusion
- OData v4 headers
- Error handling and logging

#### `Services/ExcelExportService.cs` (117 lines)
**Purpose**: Excel file generation  
**Responsibilities**:
- Create multi-worksheet workbooks
- Format headers (bold, gray background)
- Auto-fit columns
- Freeze header rows
- Apply data type formatting
- Handle empty datasets

**Key Method**: `CreateExcelFile()` - Generate workbook  
**Input**: Dictionary mapping worksheet names to data  
**Output**: File path of created Excel file

**Features**:
- Automatic data type detection
- Professional formatting
- Sheet name sanitization

#### `Services/ClaudeService.cs` (141 lines)
**Purpose**: Claude AI API integration  
**Responsibilities**:
- Convert Excel files to base64
- Build API requests
- Send documents to Claude
- Parse responses
- Track token usage

**Key Method**: `AnalyzeDataAsync()` - Send file for analysis  
**Parameters**: 
- Excel file path
- Prompt template
- Optional model override

**Features**:
- Token estimation
- Usage tracking
- Model selection
- Error handling

#### `Services/PromptService.cs` (86 lines)
**Purpose**: Prompt template management  
**Responsibilities**:
- Load prompts from JSON
- Display available templates
- Handle user selection
- Provide default prompts

**Key Methods**:
- `LoadPrompts()` - Read from prompts.json
- `SelectPromptInteractive()` - User selection
- `GetPromptByKey()` - Retrieve specific prompt

**Features**:
- Interactive selection by number or name
- Fallback to default prompt
- Validation and error handling

### Documentation Files

#### `README.md` (350+ lines)
**Purpose**: Main documentation  
**Sections**:
- Feature overview
- Prerequisites
- Getting started guide
- Configuration instructions
- Architecture explanation
- Troubleshooting
- Security best practices
- Usage examples

#### `SETUP.md` (250+ lines)
**Purpose**: Quick setup guide  
**Sections**:
- Azure AD app registration (step-by-step)
- Claude API key acquisition
- Configuration
- Query customization
- Common issues and solutions
- Pro tips

#### `EXAMPLES.md` (400+ lines)
**Purpose**: Real-world scenarios  
**Contains**:
- 6 complete business scenarios
- OData query examples
- Custom prompt templates
- Expected outputs
- Best practices per scenario

**Scenarios**:
1. Monthly Inventory Health Check
2. Sales Performance Dashboard
3. Financial Close Analysis
4. Vendor Payment Analysis
5. Production Efficiency Review
6. Customer Credit Risk Assessment

#### `ARCHITECTURE.md` (400+ lines)
**Purpose**: Technical architecture  
**Sections**:
- Architecture diagram
- Technology stack justification
- Design patterns used
- Token optimization strategies
- Security considerations
- Performance optimizations
- Extensibility points
- Known limitations
- Future enhancements

## Line Count Summary

```
Total Lines of Code: ~1,500 lines
├── Services:     ~570 lines (38%)
├── Program:      ~285 lines (19%)
├── Models:       ~90 lines (6%)
├── Configuration: ~115 lines (8%)
└── Documentation: ~1,400 lines (separate)

Total Documentation: ~1,400 lines
├── README:        ~350 lines
├── SETUP:         ~250 lines
├── EXAMPLES:      ~400 lines
└── ARCHITECTURE:  ~400 lines
```

## Dependencies

### NuGet Packages
```xml
<PackageReference Include="Microsoft.Identity.Client" Version="4.61.3" />
<PackageReference Include="ClosedXML" Version="0.102.2" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="System.Configuration.ConfigurationManager" Version="8.0.0" />
```

### External Services
- Azure Active Directory (Authentication)
- Microsoft Dynamics 365 Finance & Supply Chain (Data Source)
- Anthropic Claude API (AI Analysis)

## Build Artifacts

When built, the solution produces:

```
bin/Debug/net8.0/
├── D365ClaudeAnalyzer.exe       # Executable
├── D365ClaudeAnalyzer.dll       # Main assembly
├── App.config                    # Configuration
├── prompts.json                  # Prompts
├── *.dll                        # Dependencies
└── *.pdb                        # Debug symbols

Output/                          # Generated at runtime
├── D365_Data_Export.xlsx        # Excel data
└── Analysis_*.txt               # Claude analysis results
```

## Customization Points

### Easy to Modify
1. **OData Queries** - `Program.cs` line 150+
2. **Prompt Templates** - `prompts.json`
3. **Configuration** - `App.config`
4. **Output Path** - `App.config` Output section

### Moderate Changes
1. **New Service** - Add class in `Services/` folder
2. **New Model** - Add class in `Models/` folder
3. **Custom Formatting** - Modify `ExcelExportService.cs`

### Advanced Changes
1. **Alternative AI Provider** - Create new interface/implementation
2. **Different Data Source** - Create new data service
3. **Web Interface** - Convert to ASP.NET Core
4. **Parallel Processing** - Modify orchestration in `Program.cs`

## Getting Started Checklist

- [ ] Review README.md for overview
- [ ] Follow SETUP.md to configure
- [ ] Copy App.config.template → App.config
- [ ] Fill in credentials in App.config
- [ ] Customize queries in Program.cs
- [ ] Review/modify prompts.json
- [ ] Run `dotnet restore`
- [ ] Run `dotnet build`
- [ ] Run `dotnet run`
- [ ] Check EXAMPLES.md for scenarios

## Support Files

For additional help, see:
- **Troubleshooting**: README.md → Troubleshooting section
- **Security**: README.md → Security Best Practices
- **Scenarios**: EXAMPLES.md
- **Technical Details**: ARCHITECTURE.md

---

**Project Version**: 1.0  
**Last Updated**: December 2024  
**Target Framework**: .NET 8.0  
**License**: Provided as-is for educational and commercial use
