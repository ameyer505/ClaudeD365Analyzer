# 👋 START HERE: D365 Claude Analyzer

Welcome! This is your complete guide to getting started with the D365 Claude Analyzer.

## 🎯 What Is This?

A .NET application that:
1. Connects to your D365 Finance & Supply Chain instance
2. Pulls data using OData queries
3. Exports to Excel with multiple worksheets
4. Sends to Claude AI for intelligent analysis
5. Gives you actionable business insights

## ⚡ Quick Start (3 Steps)

### Step 1: Get Your Credentials (5 minutes)

You need two sets of credentials:

**A. Azure AD Credentials** (for D365 access)
- Tenant ID
- Client ID (Application ID)
- Client Secret

**B. Claude API Key** (for AI analysis)
- Get from: https://console.anthropic.com/

📖 **Detailed instructions**: See `SETUP.md`

### Step 2: Configure the App (2 minutes)

```bash
# Copy the template
cp App.config.template App.config

# Edit App.config and fill in:
# - Your Azure AD credentials
# - Your D365 instance URL
# - Your Claude API key
```

### Step 3: Run It (1 minute)

```bash
dotnet restore
dotnet build
dotnet run
```

That's it! 🎉

## 📚 Documentation Roadmap

Read these in order:

1. **START_HERE.md** ← You are here!
   - First-time setup
   - Quick overview
   - Where to go next

2. **README.md** (15 minutes)
   - Comprehensive overview
   - All features explained
   - Basic troubleshooting

3. **SETUP.md** (10 minutes)
   - Step-by-step configuration
   - Azure AD setup walkthrough
   - Common issues solved

4. **EXAMPLES.md** (20 minutes)
   - 6 real-world business scenarios
   - Complete query examples
   - Custom prompts for each

5. **ARCHITECTURE.md** (30 minutes)
   - Technical deep-dive
   - Design decisions explained
   - Extension points identified

6. **PROJECT_STRUCTURE.md** (5 minutes)
   - File-by-file reference
   - Quick lookup guide

## 🗂️ Project Structure

```
D365ClaudeAnalyzer/
│
├── 📄 START_HERE.md              ← You are here
├── 📄 README.md                  ← Main documentation
├── 📄 SETUP.md                   ← Setup guide
├── 📄 EXAMPLES.md                ← Usage examples
├── 📄 ARCHITECTURE.md            ← Technical details
├── 📄 PROJECT_STRUCTURE.md       ← File reference
├── 📄 PROJECT_SUMMARY.md         ← Project overview
│
├── 📄 Program.cs                 ← Main application
├── 📄 D365ClaudeAnalyzer.csproj ← Project file
├── 📄 App.config                 ← Your credentials (DO NOT COMMIT!)
├── 📄 App.config.template        ← Template (safe to commit)
├── 📄 prompts.json               ← Claude AI prompts
├── 📄 .gitignore                 ← Git ignore rules
│
├── 📁 Models/
│   ├── ClaudeModels.cs          ← Claude API models
│   └── ODataQueryConfiguration.cs ← Query definitions
│
└── 📁 Services/
    ├── D365AuthenticationService.cs
    ├── D365DataService.cs
    ├── ExcelExportService.cs
    ├── ClaudeService.cs
    └── PromptService.cs
```

## 🎓 Learning Path

### Beginner (Just Want It Working)
1. Read: START_HERE.md (this file)
2. Read: SETUP.md
3. Configure App.config
4. Run the application
5. Check EXAMPLES.md for ideas

**Time**: 30 minutes to first analysis

### Intermediate (Customization)
1. Everything from Beginner
2. Read: README.md (Features section)
3. Modify queries in Program.cs
4. Create custom prompts in prompts.json
5. Read: EXAMPLES.md for inspiration

**Time**: 2-3 hours to fully customize

### Advanced (Extension & Integration)
1. Everything from Intermediate
2. Read: ARCHITECTURE.md
3. Add new service classes
4. Integrate with other systems
5. Optimize for your use case

**Time**: Days to weeks for major customizations

## 🚦 Prerequisites Checklist

Before you start, make sure you have:

- [ ] .NET 8 SDK installed (`dotnet --version`)
- [ ] Access to D365 Finance & Supply Chain
- [ ] Azure AD app registration permissions
- [ ] Claude API account with credits
- [ ] Text editor or IDE (VS Code, Visual Studio, Rider)
- [ ] Basic understanding of:
  - [ ] C# (helpful but not required)
  - [ ] OData queries (can learn as you go)
  - [ ] JSON configuration files

## ⚙️ System Requirements

**Minimum**:
- OS: Windows 10/11, macOS 10.15+, Linux (Ubuntu 20.04+)
- .NET: 8.0 SDK or later
- RAM: 4 GB
- Disk: 500 MB free space

**Recommended**:
- RAM: 8 GB+
- Fast internet connection (for API calls)
- SSD for better performance

## 🎯 What Can You Do With This?

### Common Use Cases

**Finance Team**:
- Daily GL transaction reviews
- Month-end close validation
- Budget vs actual analysis
- Variance explanations

**Supply Chain**:
- Inventory stockout alerts
- Slow-moving item identification
- Vendor payment optimization
- Purchase order tracking

**Sales Operations**:
- Sales performance dashboards
- Customer credit monitoring
- Revenue trend analysis
- Territory performance

**Production**:
- Production efficiency tracking
- Quality issue identification
- Bottleneck analysis
- Resource utilization

See `EXAMPLES.md` for complete scenarios with code!

## 🛠️ Common First-Time Issues

### "Authentication failed"
**Fix**: Double-check your Azure AD credentials in App.config
**Details**: SETUP.md → Azure AD App Registration

### "OData query failed"
**Fix**: Verify entity name exists in your D365 instance
**Check**: `https://your-instance.dynamics.com/data/$metadata`

### "Claude API error"
**Fix**: Verify API key and account has credits
**Check**: https://console.anthropic.com/

### "No data available"
**Fix**: Your filter might be too restrictive
**Try**: Remove FilterCriteria or increase Top value

See README.md → Troubleshooting for more solutions.

## 💡 Pro Tips for First Run

1. **Start Small**: Use `Top = 100` for first test
2. **Test Credentials**: Run a simple query first
3. **Check Entity Names**: Use $metadata endpoint
4. **Use Default Prompt**: Get familiar before customizing
5. **Review Output**: Check both Excel and analysis files

## 🎉 Your First Analysis in 5 Steps

1. **Install .NET 8**:
   ```bash
   # Check if installed
   dotnet --version
   
   # If not, download from:
   # https://dotnet.microsoft.com/download/dotnet/8.0
   ```

2. **Configure Credentials**:
   ```bash
   # Copy template
   cp App.config.template App.config
   
   # Edit with your editor
   code App.config  # VS Code
   # or
   notepad App.config  # Windows
   # or
   nano App.config  # Linux/Mac
   ```

3. **Build Project**:
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Run Application**:
   ```bash
   dotnet run
   ```

5. **Review Results**:
   - Excel file: `./Output/D365_Data_Export.xlsx`
   - Analysis: Console + `./Output/Analysis_*.txt`

## 📞 Getting Help

### In This Project
- Quick answers: This file (START_HERE.md)
- Setup help: SETUP.md
- Usage examples: EXAMPLES.md
- Technical details: ARCHITECTURE.md
- Troubleshooting: README.md

### External Resources
- **D365 OData**: [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/)
- **Claude API**: [Anthropic Docs](https://docs.anthropic.com/)
- **.NET**: [Microsoft .NET Docs](https://docs.microsoft.com/dotnet/)

## 🔐 Security Reminder

**IMPORTANT**: Your `App.config` contains sensitive credentials!

✅ **DO**:
- Keep App.config secure
- Use App.config.template for sharing
- Add App.config to .gitignore
- Rotate secrets regularly
- Use Azure Key Vault in production

❌ **DON'T**:
- Commit App.config to source control
- Share credentials in chat/email
- Use production credentials in development
- Leave default passwords

## 🚀 Next Steps After First Run

### Immediate
- [ ] Review the generated Excel file
- [ ] Read Claude's analysis
- [ ] Check token usage in console
- [ ] Try a different prompt template

### This Week
- [ ] Customize OData queries for your data
- [ ] Create a custom prompt in prompts.json
- [ ] Run analysis on real business question
- [ ] Share results with stakeholders

### This Month
- [ ] Add more entities to track
- [ ] Create department-specific prompts
- [ ] Schedule regular automated runs
- [ ] Measure time/cost savings

## 📊 What Success Looks Like

After setup, you should have:
- ✅ Excel file with your D365 data
- ✅ Claude's analysis in readable format
- ✅ Token usage stats displayed
- ✅ No errors in console output
- ✅ Actionable insights from Claude

**Typical first run**: 30-60 seconds for analysis

## 🎯 Ready to Begin?

You now have everything you need:
1. ✅ Clear starting point (this file)
2. ✅ Complete documentation
3. ✅ Working code
4. ✅ Real examples
5. ✅ Troubleshooting guides

**Your Action Items**:
1. Open `SETUP.md`
2. Follow the 5-minute setup
3. Run your first analysis
4. Explore `EXAMPLES.md` for ideas

---

## 📝 Quick Reference Card

**To Configure**: Edit `App.config`  
**To Customize Queries**: Edit `Program.cs` → `GetODataQueries()`  
**To Change Prompts**: Edit `prompts.json`  
**To Run**: `dotnet run`  
**Output Location**: `./Output/` directory  

**Most Important Files**:
- `SETUP.md` - Getting started
- `EXAMPLES.md` - How to use it
- `README.md` - Everything else

---

**Welcome aboard, and happy analyzing!** 🎉

Got questions? Check the documentation. Ready to start? Open SETUP.md!

**Let's turn your D365 data into insights!** 🚀
