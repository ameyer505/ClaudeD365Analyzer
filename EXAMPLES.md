# Example Usage Scenarios

Real-world examples of how to use D365 Claude Analyzer for common business analysis tasks.

## Scenario 1: Monthly Inventory Health Check

**Business Need**: Identify slow-moving inventory and potential stockouts

### OData Queries

```csharp
// In Program.cs - GetODataQueries()
new ODataQueryConfiguration
{
    EntityName = "InventSum",
    WorksheetName = "Current_Stock_Levels",
    SelectFields = "ItemId,AvailPhysical,ReservPhysical,OnOrder",
    OrderBy = "AvailPhysical",
    Top = 2000
},
new ODataQueryConfiguration
{
    EntityName = "InventTrans",
    WorksheetName = "Recent_Movements",
    SelectFields = "ItemId,TransDate,Qty,CostAmountPosted",
    FilterCriteria = "TransDate gt " + DateTime.Now.AddDays(-90).ToString("yyyy-MM-ddTHH:mm:ssZ"),
    OrderBy = "TransDate desc",
    Top = 5000
}
```

### Custom Prompt

Add to `prompts.json`:

```json
{
  "monthly_inventory_check": {
    "name": "Monthly Inventory Health Check",
    "systemPrompt": "You are an inventory management specialist. Focus on identifying risks and opportunities in the inventory data.",
    "userPrompt": "Analyze this inventory data and provide:\n\n1. Items at risk of stockout (low available + high reservations)\n2. Slow-moving items (low recent activity)\n3. Items with excessive stock levels\n4. Recommendations for reorder points\n5. Cost implications of current inventory levels\n\nPrioritize actionable insights for the warehouse manager."
  }
}
```

### Expected Output

Claude will identify:
- Top 10 items at risk of stockout
- Slow movers consuming warehouse space
- Optimal reorder recommendations
- Estimated cost savings opportunities

---

## Scenario 2: Sales Performance Dashboard

**Business Need**: Understand sales trends and identify growth opportunities

### OData Queries

```csharp
new ODataQueryConfiguration
{
    EntityName = "SalesLine",
    WorksheetName = "Sales_Details",
    SelectFields = "SalesId,ItemId,CustAccount,LineAmount,SalesQty,SalesPrice,InventLocationId,CreatedDateTime",
    FilterCriteria = "CreatedDateTime gt " + DateTime.Now.AddMonths(-6).ToString("yyyy-MM-ddTHH:mm:ssZ"),
    OrderBy = "CreatedDateTime desc",
    Top = 10000
},
new ODataQueryConfiguration
{
    EntityName = "CustTable",
    WorksheetName = "Customer_Info",
    SelectFields = "AccountNum,Name,CustGroup,Currency",
    Top = 5000
}
```

### Using the Built-in Prompt

Select the `sales_performance` prompt when running the application.

### Sample Analysis Output

```
SALES PERFORMANCE ANALYSIS
=========================

Key Findings:
1. Revenue Growth: 15% increase QoQ driven primarily by Product Category A
2. Top Customers: Account 1234 represents 23% of revenue (concentration risk)
3. Seasonal Pattern: Clear Q4 spike, Q1 dip (plan inventory accordingly)
4. Price Trends: Average selling price decreased 3% (margin pressure)

Opportunities:
- Cross-sell Product B to customers buying only Product A (32 accounts)
- Re-engage 47 dormant customers (no orders in 90+ days)
- Regional expansion in Location X showing 40% growth

Concerns:
- Customer concentration risk in top 3 accounts
- Declining margins on high-volume products
- Warehouse X showing fulfillment delays
```

---

## Scenario 3: Financial Close Analysis

**Business Need**: Review GL transactions for month-end close

### OData Queries

```csharp
new ODataQueryConfiguration
{
    EntityName = "GeneralJournalAccountEntry",
    WorksheetName = "GL_Transactions",
    SelectFields = "AccountingDate,JournalNumber,MainAccount,PostingType,AccountingCurrencyAmount,TransactionCurrencyAmount",
    FilterCriteria = "AccountingDate ge " + new DateTime(2024, 11, 1).ToString("yyyy-MM-ddTHH:mm:ssZ") + 
                     " and AccountingDate lt " + new DateTime(2024, 12, 1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
    OrderBy = "AccountingDate",
    Top = 20000
}
```

### Custom Prompt

```json
{
  "financial_close_review": {
    "name": "Financial Close Review",
    "systemPrompt": "You are a financial controller performing month-end close procedures.",
    "userPrompt": "Review the GL transactions for:\n\n1. Unusual or large transactions requiring investigation\n2. Journal entries missing descriptions\n3. Posting pattern anomalies (timing, frequency)\n4. Accounts with unexpected activity\n5. Balance validation opportunities\n\nHighlight any red flags that need immediate attention before close."
  }
}
```

### What to Look For

- Manual journal entries that need audit support
- Accounts with unusual activity
- Missing accruals or prepayments
- Transactions that might need reclassification

---

## Scenario 4: Vendor Payment Analysis

**Business Need**: Optimize cash management and vendor relationships

### OData Queries

```csharp
new ODataQueryConfiguration
{
    EntityName = "VendInvoiceJour",
    WorksheetName = "Vendor_Invoices",
    SelectFields = "InvoiceId,InvoiceAccount,InvoiceDate,DueDate,InvoiceAmountMST,PurchId",
    FilterCriteria = "DueDate lt " + DateTime.Now.AddDays(30).ToString("yyyy-MM-ddTHH:mm:ssZ"),
    OrderBy = "DueDate",
    Top = 5000
},
new ODataQueryConfiguration
{
    EntityName = "VendTable",
    WorksheetName = "Vendor_Terms",
    SelectFields = "AccountNum,Name,PaymTermId,Currency",
    Top = 2000
}
```

### Analysis Focus

```json
{
  "vendor_payment_optimization": {
    "name": "Vendor Payment Optimization",
    "systemPrompt": "You are a cash management specialist focused on optimizing payment timing and vendor relationships.",
    "userPrompt": "Analyze vendor invoices and provide:\n\n1. Payment schedule for next 30 days\n2. Early payment discount opportunities\n3. Vendors with payment terms that could be renegotiated\n4. Cash flow impact of payment timing\n5. Vendors approaching credit limits or overdue\n\nPrioritize by dollar amount and strategic importance."
  }
}
```

---

## Scenario 5: Production Efficiency Review

**Business Need**: Identify bottlenecks and optimize production

### OData Queries

```csharp
new ODataQueryConfiguration
{
    EntityName = "ProdTable",
    WorksheetName = "Production_Orders",
    SelectFields = "ProdId,ItemId,ProdStatus,SchedStartDate,SchedEndDate,QtySched",
    FilterCriteria = "ProdStatus ne 'Ended'",
    OrderBy = "SchedStartDate",
    Top = 2000
},
new ODataQueryConfiguration
{
    EntityName = "RouteCardTable",
    WorksheetName = "Route_Cards",
    SelectFields = "ProdId,OprId,OprNum,Hours,QtyGood,QtyError",
    FilterCriteria = "QtyError gt 0",
    Top = 3000
}
```

### Custom Analysis

```json
{
  "production_efficiency": {
    "name": "Production Efficiency Analysis",
    "systemPrompt": "You are a production manager focused on maximizing efficiency and minimizing waste.",
    "userPrompt": "Review production data for:\n\n1. Operations with high error rates\n2. Production orders behind schedule\n3. Bottleneck operations (high hours, long lead times)\n4. Quality issues by product or operation\n5. Resource utilization opportunities\n\nProvide specific recommendations for improvement."
  }
}
```

---

## Scenario 6: Customer Credit Risk Assessment

**Business Need**: Proactively manage customer credit exposure

### OData Queries

```csharp
new ODataQueryConfiguration
{
    EntityName = "CustTable",
    WorksheetName = "Customer_Credit",
    SelectFields = "AccountNum,Name,CreditMax,Balance,PaymTermId",
    OrderBy = "Balance desc",
    Top = 1000
},
new ODataQueryConfiguration
{
    EntityName = "CustTrans",
    WorksheetName = "Customer_Transactions",
    SelectFields = "AccountNum,TransDate,AmountCur,DueDate,SettleAmountCur",
    FilterCriteria = "DueDate lt " + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ") + " and SettleAmountCur eq 0",
    Top = 5000
}
```

### Risk Assessment Prompt

```json
{
  "credit_risk_assessment": {
    "name": "Customer Credit Risk Assessment",
    "systemPrompt": "You are a credit analyst focused on minimizing bad debt while supporting sales growth.",
    "userPrompt": "Evaluate customer credit exposure:\n\n1. Customers exceeding credit limits\n2. Past due accounts by aging bucket\n3. Customers with deteriorating payment patterns\n4. High-risk accounts requiring attention\n5. Recommended credit holds or limit adjustments\n\nPrioritize by dollar exposure and risk severity."
  }
}
```

---

## Best Practices for Each Scenario

### Data Freshness
- Run inventory checks weekly
- Sales analysis monthly or quarterly
- Financial close analysis daily during close period
- Credit reviews weekly

### Token Optimization
- Use `Top` to limit results: Start with 1000-2000 records
- Be selective with `SelectFields`: Only include necessary fields
- Apply smart filters: Date ranges, status codes, thresholds

### Prompt Engineering Tips

**Be Specific**: Instead of "analyze this data," specify what you're looking for  
**Set Context**: Tell Claude your role and business constraints  
**Request Structure**: Ask for numbered lists, tables, or specific formats  
**Define Priorities**: What's most important to you?

### Follow-Up Actions

After getting Claude's analysis:
1. Export the analysis text file for documentation
2. Share insights with stakeholders
3. Create action items in your project management tool
4. Update the Excel file with notes based on decisions made
5. Schedule follow-up analysis to track improvements

---

## Combining Multiple Scenarios

You can run multiple analysis types in sequence:

```csharp
// Run inventory + sales together for demand planning
var queries = new List<ODataQueryConfiguration>();
queries.AddRange(GetInventoryQueries());
queries.AddRange(GetSalesQueries());
```

Then use a custom prompt that looks at both:
```
"Analyze inventory levels in context of sales trends to recommend:
1. Reorder quantities based on actual sales velocity
2. Items to discontinue due to low sales
3. High-demand items needing increased stock"
```

---

**Remember**: Claude is a powerful analyst, but you're the domain expert. Use these examples as starting points and customize them for your specific business needs!
