# Quick Start Guide

## 🚀 Get Running in 5 Minutes

### Step 1: Prerequisites Check
```bash
# Verify .NET is installed
dotnet --version
# Should show 8.0 or higher
```

### Step 2: Configure Your Credentials

1. Copy the template:
   ```bash
   cp App.config.template App.config
   ```

2. Edit `App.config` with your actual values:
   - D365 Tenant ID (from Azure Portal)
   - D365 Client ID (from Azure Portal)
   - D365 Client Secret (from Azure Portal)
   - D365 Environment URL (your-env.operations.dynamics.com)
   - Claude API Key (from console.anthropic.com)

### Step 3: Customize Your Queries

Edit `Program.cs` around line 30 to specify what data you want:

```csharp
var queries = new List<ODataQuery>
{
    new ODataQuery
    {
        SheetName = "Customers",
        EntityName = "CustomersV3",  // Change to your entity
        Select = "CustomerAccount,OrganizationName",  // Your fields
        Top = 100
    }
};
```

**Don't know what entities are available?** Check `SAMPLE_QUERIES.md`!

### Step 4: Build and Run

```bash
# Restore NuGet packages
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

### Step 5: Select Analysis

When prompted, choose a prompt template (1-3) or press Enter to skip.

### Step 6: Get Results

- Excel file: `./output/D365_Data_Export.xlsx`
- Analysis: `./output/Analysis_[timestamp].txt`

## ⚡ Common Issues

**"Failed to authenticate"**
→ Check your Tenant ID, Client ID, and Client Secret

**"OData query failed: 404"**
→ Verify the entity name exists in your D365 environment

**"Configuration validation failed"**
→ Make sure App.config has all required values (no "your-xxx-here" placeholders)

## 📚 Next Steps

1. Read `SETUP.md` for detailed configuration
2. Browse `SAMPLE_QUERIES.md` for query examples
3. Customize `prompts.json` for better analysis

## 🆘 Need Help?

Check these files in order:
1. `README.md` - Overview and architecture
2. `SETUP.md` - Detailed setup instructions
3. `SAMPLE_QUERIES.md` - Query examples

Happy analyzing! 🎉
