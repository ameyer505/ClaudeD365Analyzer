# Sample D365 OData Queries

This document provides ready-to-use OData query examples for common D365 Finance & Supply Chain scenarios. Copy and paste these into your `Program.cs` queries list and customize as needed.

## Financial Data Queries

### General Ledger Transactions
```csharp
new ODataQuery
{
    SheetName = "GL_Transactions",
    EntityName = "GeneralJournalAccountEntries",
    Select = "JournalBatchNumber,AccountingDate,LedgerAccount,TransactionCurrencyAmount,AccountingCurrencyAmount,TransactionCurrencyCode",
    Filter = $"AccountingDate ge {DateTime.UtcNow.AddMonths(-3):yyyy-MM-dd}",
    OrderBy = "AccountingDate desc",
    Top = 1000
}
```

### Budget vs Actuals
```csharp
new ODataQuery
{
    SheetName = "Budget_Analysis",
    EntityName = "BudgetTransactionLines",
    Select = "BudgetModelId,LedgerDimensionDisplayValue,TransactionDate,TransactionCurrencyAmount,AccountingCurrencyAmount",
    Filter = $"TransactionDate ge {DateTime.UtcNow.Year}-01-01",
    OrderBy = "TransactionDate",
    Top = 5000
}
```

### Accounts Receivable Aging
```csharp
new ODataQuery
{
    SheetName = "AR_Aging",
    EntityName = "CustomerInvoiceJournals",
    Select = "InvoiceId,InvoiceDate,DueDate,CustomerAccount,InvoiceAmount,RemainAmountCur,CurrencyCode",
    Filter = $"RemainAmountCur gt 0 and DueDate lt {DateTime.UtcNow:yyyy-MM-dd}",
    OrderBy = "DueDate",
    Top = 1000
}
```

## Sales & Customer Queries

### Recent Sales Orders
```csharp
new ODataQuery
{
    SheetName = "Sales_Orders",
    EntityName = "SalesOrderHeadersV2",
    Select = "SalesOrderNumber,OrderingCustomerAccountNumber,CustomerRequisitionNumber,TotalAmount,CurrencyCode,SalesOrderStatus,RequestedShippingDate",
    Filter = $"RequestedShippingDate ge {DateTime.UtcNow.AddDays(-30):yyyy-MM-dd}",
    OrderBy = "RequestedShippingDate desc",
    Top = 500
}
```

### Sales Order Lines
```csharp
new ODataQuery
{
    SheetName = "Sales_Order_Lines",
    EntityName = "SalesOrderLinesV2",
    Select = "SalesOrderNumber,LineNumber,ItemNumber,ProductName,OrderedSalesQuantity,SalesPrice,LineAmount,CurrencyCode,ConfirmedShippingDate",
    Filter = $"ConfirmedShippingDate ge {DateTime.UtcNow.AddDays(-7):yyyy-MM-dd}",
    Top = 1000
}
```

### Top Customers by Revenue (Last Quarter)
```csharp
new ODataQuery
{
    SheetName = "Top_Customers",
    EntityName = "SalesOrderHeadersV2",
    Select = "OrderingCustomerAccountNumber,CurrencyCode,TotalAmount",
    Filter = $"OrderDate ge {DateTime.UtcNow.AddMonths(-3):yyyy-MM-dd}",
    Top = 100
}
```

### Customer Master Data
```csharp
new ODataQuery
{
    SheetName = "Customers",
    EntityName = "CustomersV3",
    Select = "CustomerAccount,CustomerGroupId,OrganizationName,PrimaryContactEmail,PrimaryContactPhone,AddressCity,AddressCountryRegionId,SalesCurrencyCode",
    Top = 500
}
```

## Inventory & Supply Chain Queries

### Current On-Hand Inventory
```csharp
new ODataQuery
{
    SheetName = "Inventory_OnHand",
    EntityName = "InventoryOnHandV2",
    Select = "ItemNumber,ProductName,WarehouseId,InventorySiteId,AvailableOnHand,AvailableOrdered,AvailablePhysical,UnitOfMeasure",
    Filter = "AvailableOnHand gt 0",
    OrderBy = "ItemNumber",
    Top = 1000
}
```

### Low Stock Items
```csharp
new ODataQuery
{
    SheetName = "Low_Stock_Alert",
    EntityName = "InventoryOnHandV2",
    Select = "ItemNumber,ProductName,WarehouseId,AvailableOnHand,AvailableOrdered",
    Filter = "AvailableOnHand lt 10 and AvailableOnHand gt 0",
    OrderBy = "AvailableOnHand asc",
    Top = 200
}
```

### Inventory Transactions (Recent)
```csharp
new ODataQuery
{
    SheetName = "Recent_Inventory_Moves",
    EntityName = "InventoryTransactions",
    Select = "ItemId,TransactionDate,Quantity,CostAmount,InventorySiteId,InventoryWarehouseId,TransactionType",
    Filter = $"TransactionDate ge {DateTime.UtcNow.AddDays(-7):yyyy-MM-dd}",
    OrderBy = "TransactionDate desc",
    Top = 1000
}
```

### Products with Prices
```csharp
new ODataQuery
{
    SheetName = "Product_Catalog",
    EntityName = "ProductsV2",
    Select = "ProductNumber,ProductName,ProductType,ItemModelGroupId,ItemSalesTaxGroupId,PrimaryVendorAccountNumber",
    Top = 500
}
```

## Procurement Queries

### Open Purchase Orders
```csharp
new ODataQuery
{
    SheetName = "Open_POs",
    EntityName = "PurchaseOrderHeadersV2",
    Select = "PurchaseOrderNumber,OrderVendorAccountNumber,PurchaseOrderStatus,TotalAmount,CurrencyCode,DeliveryDate",
    Filter = "PurchaseOrderStatus ne 'Invoiced' and PurchaseOrderStatus ne 'Canceled'",
    OrderBy = "DeliveryDate",
    Top = 500
}
```

### Purchase Order Lines
```csharp
new ODataQuery
{
    SheetName = "PO_Lines",
    EntityName = "PurchaseOrderLinesV2",
    Select = "PurchaseOrderNumber,LineNumber,ItemNumber,ProductName,OrderedPurchaseQuantity,PurchasePrice,LineAmount,ConfirmedDeliveryDate",
    Filter = $"ConfirmedDeliveryDate ge {DateTime.UtcNow:yyyy-MM-dd}",
    OrderBy = "ConfirmedDeliveryDate",
    Top = 1000
}
```

### Vendor Master Data
```csharp
new ODataQuery
{
    SheetName = "Vendors",
    EntityName = "VendorsV2",
    Select = "VendorAccountNumber,VendorOrganizationName,VendorGroupId,PrimaryContactEmail,PrimaryContactPhone,CurrencyCode",
    Top = 300
}
```

## Manufacturing Queries

### Production Orders
```csharp
new ODataQuery
{
    SheetName = "Production_Orders",
    EntityName = "ProductionOrderHeaders",
    Select = "ProductionOrderNumber,ProductionOrderStatus,ItemNumber,ScheduledStartDate,ScheduledEndDate,ProductionQuantity",
    Filter = "ProductionOrderStatus eq 'Scheduled' or ProductionOrderStatus eq 'Released' or ProductionOrderStatus eq 'Started'",
    OrderBy = "ScheduledStartDate",
    Top = 500
}
```

### Work Orders
```csharp
new ODataQuery
{
    SheetName = "Work_Orders",
    EntityName = "WorkOrdersV2",
    Select = "WorkOrderId,AssetId,MaintenanceJobTypeId,WorkOrderStatus,ActualStartDateTime,ActualEndDateTime,Description",
    Filter = "WorkOrderStatus ne 'Completed'",
    OrderBy = "ActualStartDateTime desc",
    Top = 300
}
```

## Advanced Query Patterns

### Multiple Filters (AND condition)
```csharp
Filter = $"TransactionDate ge {DateTime.UtcNow.AddMonths(-1):yyyy-MM-dd} and TotalAmount gt 1000 and CurrencyCode eq 'USD'"
```

### Multiple Filters (OR condition)
```csharp
Filter = "SalesOrderStatus eq 'Delivered' or SalesOrderStatus eq 'Invoiced'"
```

### String Contains
```csharp
Filter = "contains(CustomerAccount, '001')"
```

### Null Check
```csharp
Filter = "PrimaryContactEmail ne null"
```

### Date Range
```csharp
Filter = $"InvoiceDate ge {startDate:yyyy-MM-dd} and InvoiceDate le {endDate:yyyy-MM-dd}"
```

## Tips for Building Efficient Queries

### 1. Always Use $select
Don't retrieve all fields if you only need a few. This reduces:
- Network bandwidth
- Processing time
- Token usage in Claude analysis

**Bad**:
```csharp
// No Select = Gets ALL fields (could be 50+ fields!)
EntityName = "CustomersV3"
```

**Good**:
```csharp
Select = "CustomerAccount,OrganizationName,PrimaryContactEmail"
```

### 2. Use $top Wisely
Limit results to what you actually need:
- Development/Testing: Use `Top = 100`
- Production: Assess actual need, consider pagination

### 3. Filter at the Source
Push filtering to D365 rather than filtering in code:

**Bad**:
```csharp
// Gets 10,000 records then filters in C#
Top = 10000
// Then filter in LINQ...
```

**Good**:
```csharp
Filter = "AvailableOnHand lt 10",
Top = 200
```

### 4. Order Matters for Top N Queries
When using `$top`, combine with `$orderby`:

```csharp
// Get 50 most recent orders
OrderBy = "OrderDate desc",
Top = 50
```

### 5. Use Specific Date Formats
Always use ISO 8601 format for dates:
```csharp
// Correct
Filter = $"OrderDate ge {DateTime.UtcNow:yyyy-MM-dd}"

// Wrong (will fail)
Filter = $"OrderDate ge {DateTime.UtcNow.ToShortDateString()}"
```

## Common Entity Field Names

| Entity | Key Fields to Select |
|--------|---------------------|
| CustomersV3 | CustomerAccount, OrganizationName, CustomerGroupId, PrimaryContactEmail |
| SalesOrderHeadersV2 | SalesOrderNumber, OrderingCustomerAccountNumber, TotalAmount, CurrencyCode, SalesOrderStatus |
| InventoryOnHandV2 | ItemNumber, ProductName, WarehouseId, AvailableOnHand, AvailableOrdered |
| PurchaseOrderHeadersV2 | PurchaseOrderNumber, OrderVendorAccountNumber, PurchaseOrderStatus, TotalAmount, DeliveryDate |
| VendorsV2 | VendorAccountNumber, VendorOrganizationName, VendorGroupId, CurrencyCode |

## Finding Available Entities

### Method 1: Browse $metadata
```
https://your-environment.operations.dynamics.com/data/$metadata
```

Search for `<EntityType Name="YourEntity">` to see all fields.

### Method 2: Use OData Query Browser
Many D365 environments have built-in OData query tools under **System administration** → **OData query browser**.

### Method 3: Documentation
Check Microsoft Learn documentation:
https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/data-entities/data-entities

## Combining Multiple Queries for Rich Analysis

Create a comprehensive analysis by querying related entities:

```csharp
var queries = new List<ODataQuery>
{
    // Get customer master data
    new ODataQuery
    {
        SheetName = "Customers",
        EntityName = "CustomersV3",
        Select = "CustomerAccount,OrganizationName,CustomerGroupId",
        Top = 100
    },
    
    // Get their orders
    new ODataQuery
    {
        SheetName = "Customer_Orders",
        EntityName = "SalesOrderHeadersV2",
        Select = "SalesOrderNumber,OrderingCustomerAccountNumber,TotalAmount,OrderDate",
        Filter = $"OrderDate ge {DateTime.UtcNow.AddMonths(-6):yyyy-MM-dd}",
        Top = 1000
    },
    
    // Get order lines
    new ODataQuery
    {
        SheetName = "Order_Details",
        EntityName = "SalesOrderLinesV2",
        Select = "SalesOrderNumber,ItemNumber,OrderedSalesQuantity,SalesPrice,LineAmount",
        Top = 5000
    }
};
```

Then prompt Claude to analyze relationships: "Compare customer order patterns and identify which customer segments generate the highest revenue."

---

**Remember**: Start small, test often, and gradually increase data volumes as you optimize your queries! 📊
