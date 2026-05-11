namespace Shop.Web.API.Models.Responses
{
    public record CustomerReportRow
    {
        public int CustomerId { get; init; }
        public string CustomerName { get; init; } = "";
        public string Phone { get; init; } = "";
        public string CustomerType { get; init; } = "";
        public int TotalOrders { get; init; }
        public decimal TotalSales { get; init; }
        public decimal TotalPaid { get; init; }
        public decimal TotalDue { get; init; }
        public DateTime? LastOrderDate { get; init; }
    }

    public record PurchaseReportRow
    {
        public int SupplierId { get; init; }
        public string SupplierName { get; init; } = "";
        public string Phone { get; init; } = "";
        public int TotalPurchases { get; init; }
        public decimal TotalAmount { get; init; }
        public decimal TotalPaid { get; init; }
        public decimal TotalDue { get; init; }
        public DateTime? LastPurchaseDate { get; init; }
    }

    // ── Global summary returned alongside purchase report ────────

    public record PurchaseReportGlobalSummary
    {
        public int TotalSuppliers { get; init; }
        public int TotalPurchases { get; init; }
        public decimal TotalAmount { get; init; }
        public decimal TotalDue { get; init; }
    }

    // ── Paged response with embedded global summary ───────────────

    /// <summary>
    /// Wraps the paged purchase report rows and includes the full-period
    /// global totals (across all pages) so the summary card is always accurate.
    /// </summary>
    public class PagedPurchaseReportResponse : PagedResponse<PurchaseReportRow>
    {
        public PurchaseReportGlobalSummary GlobalSummary { get; init; } = new();
    }
}
