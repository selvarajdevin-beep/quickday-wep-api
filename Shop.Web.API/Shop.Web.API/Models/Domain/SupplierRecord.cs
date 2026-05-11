namespace Shop.Web.API.Models.Domain
{
    /// <summary>
    /// Flat record returned by usp_Suppliers_GetAll / GetById.
    /// Aggregates (TotalPurchases, TotalAmount, AmountDue, LastPurchaseDate)
    /// are computed in the SP via OUTER APPLY — not stored columns.
    /// </summary>
    public class SupplierRecord
    {
        public int Id { get; init; }
        public int BusinessAccountId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string? Address { get; init; }
        public string? GSTIN { get; init; }
        public string? ContactPerson { get; init; }
        public string? Notes { get; init; }
        public bool Active { get; init; }
        public DateTime CreatedAt { get; init; }
        //public string? RowVersion { get; init; }   // hex string from SP
        public byte[]? RowVersion { get; init; }


        // Aggregated from Purchases
        public int TotalPurchases { get; init; }
        public decimal TotalAmount { get; init; }
        public decimal AmountDue { get; init; }
        public DateTime? LastPurchaseDate { get; init; }
    }

    /// <summary>Purchase record returned by usp_Suppliers_GetPurchases.</summary>
    //public class PurchaseRecord
    //{
    //    public int Id { get; init; }
    //    public int BusinessAccountId { get; init; }
    //    public int SupplierId { get; init; }
    //    public string SupplierName { get; init; } = string.Empty;
    //    public string ItemsJson { get; init; } = "[]";
    //    public decimal GrandTotal { get; init; }
    //    public decimal PaidAmount { get; init; }
    //    public decimal Balance { get; init; }
    //    public string PaymentStatus { get; init; } = "Paid";
    //    public string? Notes { get; init; }
    //    public DateTime CreatedAt { get; init; }
    //    public string? RowVersion { get; init; }
    //}
}
