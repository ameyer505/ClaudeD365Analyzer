# System Flow Diagram

## 📊 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                       D365 Claude Analyzer                       │
│                      (.NET 8.0 Console App)                      │
└─────────────────────────────────────────────────────────────────┘
                                │
                ┌───────────────┴───────────────┐
                │                               │
        ┌───────▼──────┐                ┌──────▼───────┐
        │  App.config  │                │ prompts.json │
        │              │                │              │
        │ • D365       │                │ • System     │
        │ • Claude     │                │ • User       │
        │ • Output     │                │ • Prompts    │
        └───────┬──────┘                └──────┬───────┘
                │                               │
                └───────────────┬───────────────┘
                                │
                        ┌───────▼──────┐
                        │  Program.cs  │
                        │ (Orchestrator)│
                        └───────┬──────┘
                                │
        ┌───────────────────────┼───────────────────────┐
        │                       │                       │
┌───────▼──────┐        ┌──────▼───────┐       ┌──────▼───────┐
│ Configuration│        │   Services    │       │    Models    │
│   Helpers    │        │               │       │              │
│              │        │ • D365Auth    │       │ • Config     │
│ • Validate   │        │ • OData       │       │ • Prompts    │
│ • Load       │        │ • Excel       │       │ • Queries    │
│ • Parse      │        │ • Claude      │       │              │
└──────────────┘        └───────┬──────┘       └──────────────┘
                                │
                ┌───────────────┼───────────────┐
                │               │               │
        ┌───────▼──────┐ ┌─────▼──────┐ ┌─────▼──────┐
        │     D365     │ │   Excel    │ │   Claude   │
        │ (External)   │ │  (Local)   │ │ (External) │
        └──────────────┘ └────────────┘ └────────────┘
```

## 🔄 Execution Flow

```
START
  │
  ├─► 1. Load & Validate Configuration
  │   ├─ App.config (D365, Claude, Output)
  │   └─ prompts.json
  │
  ├─► 2. Initialize Services
  │   ├─ D365AuthService
  │   ├─ D365ODataService
  │   ├─ ExcelExportService
  │   └─ ClaudeApiService
  │
  ├─► 3. Authenticate with D365
  │   ├─ Azure AD OAuth 2.0
  │   ├─ Get Access Token
  │   └─ Cache Token
  │
  ├─► 4. Execute OData Queries
  │   ├─ Query 1: Customers
  │   ├─ Query 2: Sales Orders
  │   ├─ Query 3: Inventory
  │   └─ ... (multiple queries)
  │
  ├─► 5. Create Excel Workbook
  │   ├─ Sheet 1: Customers
  │   ├─ Sheet 2: Sales Orders
  │   ├─ Sheet 3: Inventory
  │   └─ Save to ./output/
  │
  ├─► 6. User Selects Prompt
  │   ├─ Financial Analysis
  │   ├─ Inventory Analysis
  │   ├─ Custom Analysis
  │   └─ Or Skip
  │
  ├─► 7. Send to Claude API
  │   ├─ Encode Excel as Base64
  │   ├─ Build Request (System + User Prompts)
  │   ├─ POST to Claude API
  │   └─ Receive Analysis
  │
  ├─► 8. Display & Save Results
  │   ├─ Console Output
  │   ├─ Save Analysis to File
  │   └─ Show Token Usage
  │
END
```

## 🔐 Authentication Flow

```
Application
    │
    ├─► Azure AD Token Request
    │   ├─ Tenant ID
    │   ├─ Client ID
    │   ├─ Client Secret
    │   └─ Resource (D365 URL)
    │
    ▼
Azure AD
    │
    ├─► Validate Credentials
    │
    ▼
    │
    ├─► Issue Access Token
    │   ├─ Valid for ~60 minutes
    │   └─ Includes permissions
    │
    ▼
Application
    │
    ├─► Cache Token
    │   └─ Reuse until expiration
    │
    ▼
D365 OData API
    │
    ├─► Validate Token
    │
    ├─► Return Data
    │
    ▼
Application
```

## 📡 OData Query Flow

```
ODataQuery Object
    │
    ├─ EntityName: "CustomersV3"
    ├─ Select: "CustomerAccount,Name"
    ├─ Filter: "Status eq 'Active'"
    ├─ OrderBy: "Name desc"
    └─ Top: 100
    │
    ▼
Build Query String
    │
    └─► /data/CustomersV3?
        $select=CustomerAccount,Name&
        $filter=Status eq 'Active'&
        $orderby=Name desc&
        $top=100
    │
    ▼
HTTP GET Request
    │
    ├─ Headers:
    │  ├─ Authorization: Bearer <token>
    │  └─ Accept: application/json
    │
    ▼
D365 Server
    │
    ├─► Process Query
    ├─► Apply Filters
    ├─► Fetch Data
    │
    ▼
JSON Response
    │
    └─► {
          "value": [
            { "CustomerAccount": "001", "Name": "Acme" },
            { "CustomerAccount": "002", "Name": "TechCo" }
          ]
        }
    │
    ▼
Parse to JArray
    │
    ▼
Add to Results List
```

## 📊 Excel Generation Flow

```
Data Collection
    │
    ├─ Sheet 1: [100 Customer Records]
    ├─ Sheet 2: [500 Sales Orders]
    └─ Sheet 3: [200 Inventory Items]
    │
    ▼
Create Workbook
    │
For Each Data Sheet:
    │
    ├─► Create Worksheet
    │   └─ Sanitize Name (max 31 chars)
    │
    ├─► Extract Property Names
    │   └─ From first record
    │
    ├─► Write Headers (Row 1)
    │   ├─ Bold Font
    │   └─ Gray Background
    │
    ├─► Write Data (Row 2+)
    │   └─ Handle null values
    │
    └─► Auto-fit Columns
    │
    ▼
Save to File
    │
    └─► ./output/D365_Data_Export.xlsx
```

## 🤖 Claude API Integration Flow

```
Excel File
    │
    ├─► Read File Bytes
    │
    ├─► Convert to Base64
    │
    ▼
Build Request
    │
    ├─ model: "claude-sonnet-4-20250514"
    ├─ max_tokens: 4096
    │
    ├─ system: "You are a D365 expert..."
    │
    ├─ messages: [
    │   {
    │     role: "user",
    │     content: [
    │       {
    │         type: "document",
    │         source: {
    │           type: "base64",
    │           media_type: "application/vnd...",
    │           data: "<base64 data>"
    │         }
    │       },
    │       {
    │         type: "text",
    │         text: "Analyze this data..."
    │       }
    │     ]
    │   }
    │ ]
    │
    ▼
POST to Claude API
    │
    └─► https://api.anthropic.com/v1/messages
        Headers:
        ├─ x-api-key: <key>
        ├─ anthropic-version: 2023-06-01
        └─ Content-Type: application/json
    │
    ▼
Claude Processing
    │
    ├─► Parse Excel Structure
    ├─► Analyze Data
    ├─► Generate Insights
    │
    ▼
JSON Response
    │
    └─► {
          "content": [
            {
              "type": "text",
              "text": "Based on the analysis..."
            }
          ],
          "usage": {
            "input_tokens": 125000,
            "output_tokens": 2500
          }
        }
    │
    ▼
Extract & Display
    │
    ├─► Console Output
    └─► Save to ./output/Analysis_[timestamp].txt
```

## 🔁 Service Dependencies

```
Program.cs
    │
    ├─► ConfigurationHelper
    │   └─► System.Configuration
    │
    ├─► D365AuthService
    │   └─► Microsoft.Identity.Client (MSAL)
    │
    ├─► D365ODataService
    │   ├─► D365AuthService (dependency)
    │   └─► HttpClient
    │
    ├─► ExcelExportService
    │   └─► ClosedXML
    │
    └─► ClaudeApiService
        └─► HttpClient
```

## 💾 Data Flow Through System

```
D365 Database
    │
    └─► OData Endpoint
         │
         ├─► JSON Data
         │
         ▼
    .NET Application
         │
         ├─► JArray Objects
         │
         ▼
    Excel File (.xlsx)
         │
         ├─► Base64 Encoded
         │
         ▼
    Claude API
         │
         ├─► AI Analysis
         │
         ▼
    Text Analysis
         │
         ├─► Console Display
         └─► Text File (.txt)
```

## 🎯 Error Handling Flow

```
Try
│
├─► Validate Configuration
│   └─ Fail → Log Error → Exit(1)
│
├─► Authenticate
│   └─ Fail → Log Error → Exit(1)
│
├─► Execute Queries
│   ├─ Query 1 Fails → Log → Continue
│   ├─ Query 2 Succeeds → Continue
│   └─ Query 3 Fails → Log → Continue
│
├─► Create Excel
│   └─ Fail → Log Error → Exit(1)
│
├─► Call Claude API
│   └─ Fail → Log Error → Skip Analysis
│
└─► Complete
    └─ Success → Exit(0)
```

## 📈 Token Flow Optimization

```
Large Dataset
│
├─► Use $select
│   └─ 50 fields → 5 fields
│       └─ Token Savings: 40%
│
├─► Use $top
│   └─ 10000 rows → 1000 rows
│       └─ Token Savings: 90%
│
├─► Use $filter
│   └─ All Records → Relevant Records
│       └─ Token Savings: Variable
│
└─► Result
    └─ Manageable Token Count
        └─ Lower API Costs
```

---

## 🔍 Legend

- `┌─┐` - Component Boundary
- `│` - Data/Control Flow
- `├─►` - Flow Direction
- `▼` - Downward Flow
- `└─►` - End Point Flow

---

*This diagram provides a visual reference for understanding the system architecture and data flow. Refer to code comments for implementation details.*
