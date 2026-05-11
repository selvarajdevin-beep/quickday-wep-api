namespace Shop.Web.API.Models.Domain
{
    public class CustomerRecord
    {
        public int Id { get; init; }
        public int BusinessAccountId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public string CustomerType { get; init; } = "Home";
        public decimal DefaultPricePerCan { get; init; }
        public int DefaultPriceProductId { get; init; }
        public bool UsePriceFromProduct { get; init; }
        public int TotalOrders { get; init; }
        public decimal TotalDue { get; init; }
        public DateTime? LastOrderDate { get; init; }
        public bool Active { get; init; }
        public DateTime CreatedAt { get; init; }
        //public string? RowVersion { get; init; }
        public byte[]? RowVersion { get; init; }
    }
}