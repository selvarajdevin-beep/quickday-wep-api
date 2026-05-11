namespace Shop.Web.API.Models.Responses
{
    public sealed class ShopDetailDto : ShopListItemDto
    {
        public string? Address { get; init; }
        public string? Gstin { get; init; }
        public string? ThemeColor { get; init; }
        public string? Currency { get; init; }
        public int TotalOrders { get; init; }
    }
}
