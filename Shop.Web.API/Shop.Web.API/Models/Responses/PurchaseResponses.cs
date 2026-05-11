namespace Shop.Web.API.Models.Responses
{
    /// <summary>Mirrors the Purchase interface in shared-state.interfaces.ts.</summary>
    public class PurchaseDto
    {
        public int Id { get; init; }
        public int BusinessAccountId { get; init; }
        public int SupplierId { get; init; }
        public string SupplierName { get; init; } = string.Empty;
        public List<PurchaseItemDto> Items { get; init; } = [];
        public decimal GrandTotal { get; init; }
        public decimal PaidAmount { get; init; }
        public decimal Balance { get; init; }
        public string PaymentStatus { get; init; } = "Paid";
        public string? Notes { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public string RowVersion { get; init; } = string.Empty;
    }

    public class PurchaseItemDto
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public decimal PricePerUnit { get; init; }
        public decimal Total { get; init; }
    }

    public class PurchaseSummaryDto
    {
        public decimal TotalThisMonth { get; init; }
        public decimal CreditPending { get; init; }
        public int PurchaseCount { get; init; }
    }

}
