namespace Shop.Web.API.Models.Responses
{
    /// <summary>
    /// Mirrors the Product interface in shared-state.interfaces.ts.
    /// Returned by all Product API endpoints.
    /// </summary>
    public class ProductDto
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
        /// <summary>Base64-encoded RowVersion — must be sent back on UPDATE.</summary>
        public string RowVersion { get; init; } = string.Empty;
    }

    public class ProductSummaryDto
    {
        public int TotalProducts { get; init; }
        public int ActiveCount { get; init; }
        public int InactiveCount { get; init; }
        public int LowStockCount { get; init; }
        public int CategoryCount { get; init; }
    }
}
