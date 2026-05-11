namespace Shop.Web.API.Models.Domain
{
    /// <summary>
    /// Flat record returned by usp_Purchases_GetAll / GetById.
    /// ItemsJson is a raw JSON string — parsed in the service layer.
    /// </summary>
    public class PurchaseRecord
    {
        public int Id { get; init; }
        public int BusinessAccountId { get; init; }
        public int SupplierId { get; init; }
        public string SupplierName { get; init; } = string.Empty;
        public string ItemsJson { get; init; } = "[]";
        public decimal GrandTotal { get; init; }
        public decimal PaidAmount { get; init; }
        public decimal Balance { get; init; }
        public string PaymentStatus { get; init; } = "Paid";
        public string? Notes { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public byte[]? RowVersion { get; init; }   // hex string from SP
    }

    /// <summary>Purchase KPI summary returned by usp_Purchases_GetSummary.</summary>
    public class PurchaseSummaryRecord
    {
        public decimal TotalThisMonth { get; init; }
        public decimal CreditPending { get; init; }
        public int PurchaseCount { get; init; }
    }
}
