namespace Shop.Web.API.Models.Domain
{
    public class InventoryLogRecord
    {
        public long Id { get; init; }
        public int BusinessAccountId { get; init; }
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public string Reason { get; init; } = string.Empty;
        public string Reference { get; init; } = string.Empty;
        public DateTime Date { get; init; }
    }
}
