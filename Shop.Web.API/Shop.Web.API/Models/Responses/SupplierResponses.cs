namespace Shop.Web.API.Models.Responses
{
    /// <summary>
    /// Mirrors the Supplier interface in shared-state.interfaces.ts.
    /// </summary>
    public class SupplierDto
    {
        public int Id { get; init; }
        public int BusinessAccountId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public string GSTIN { get; init; } = string.Empty;
        public string ContactPerson { get; init; } = string.Empty;
        public string Notes { get; init; } = string.Empty;
        public bool Active { get; init; }
        public DateTime CreatedAt { get; init; }
        public int TotalPurchases { get; init; }
        public decimal TotalAmount { get; init; }
        public decimal AmountDue { get; init; }
        public DateTime? LastPurchaseDate { get; init; }
        public string RowVersion { get; init; } = string.Empty;
    }

    //public class PurchaseItemDto
    //{
    //    public int ProductId { get; init; }
    //    public string ProductName { get; init; } = string.Empty;
    //    public int Quantity { get; init; }
    //    public decimal PricePerUnit { get; init; }
    //    public decimal Total { get; init; }
    //}

    //public class PurchaseDto
    //{
    //    public int Id { get; init; }
    //    public int SupplierId { get; init; }
    //    public string SupplierName { get; init; } = string.Empty;
    //    public List<PurchaseItemDto> Items { get; init; } = [];
    //    public decimal GrandTotal { get; init; }
    //    public decimal PaidAmount { get; init; }
    //    public decimal Balance { get; init; }
    //    public string PaymentStatus { get; init; } = "Paid";
    //    public string? Notes { get; init; }
    //    public DateTime CreatedAt { get; init; }
    //    public string RowVersion { get; init; } = string.Empty;
    //}

}
