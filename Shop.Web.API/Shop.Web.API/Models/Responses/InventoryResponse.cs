namespace Shop.Web.API.Models.Responses
{
    /// <summary>Mirrors InventoryLog interface in shared-state.interfaces.ts.</summary>
    public class InventoryLogDto
    {
        public long Id { get; init; }
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public string Reason { get; init; } = string.Empty;
        public string? Reference { get; init; }
        public DateTime Date { get; init; }
    }
}
