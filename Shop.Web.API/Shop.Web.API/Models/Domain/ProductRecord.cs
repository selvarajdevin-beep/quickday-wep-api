namespace Shop.Web.API.Models.Domain
{
    /// <summary>
    /// Flat record returned by usp_Products_GetAll / GetById.
    /// Maps 1-to-1 to the SELECT column list in those SPs.
    /// </summary>
    public class ProductRecord
    {
        public int Id { get; init; }
        public int BusinessAccountId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string UnitType { get; init; } = string.Empty;
        public string Capacity { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public decimal SellingPrice { get; init; }
        public decimal PurchasePrice { get; init; }
        public int CurrentStock { get; init; }
        public int MinStockAlert { get; init; }
        public bool Active { get; init; }
        public int TotalOrders { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public byte[]? RowVersion { get; init; }   // hex string "0x..." from SP
    }

    /// <summary>KPI summary returned by usp_Products_GetSummary.</summary>
    public class ProductSummaryRecord
    {
        public int TotalProducts { get; init; }
        public int ActiveCount { get; init; }
        public int InactiveCount { get; init; }
        public int LowStockCount { get; init; }
        public int CategoryCount { get; init; }
    }

    public class ProductInventorySummary
    {
        public int TotalStockUnits { get; set; }
        public int LowStockCount { get; set; }
    }
}
