namespace Shop.Web.API.Models.Domain
{
    public class PublicSettingsRecord
    {
        public string BusinessName { get; init; } = string.Empty;
        public string ThemeColor { get; init; } = "#0057FF";
        public string Currency { get; init; } = "INR";
        public string CurrencySymbol { get; init; } = "₹";
        public string ShopType { get; init; } = "Other";
    }
}
