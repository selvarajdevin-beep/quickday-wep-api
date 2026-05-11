//namespace Shop.Web.API.Models.Domain
//{
//    public sealed record ShopListRecord(
//        int BusinessAccountId,
//        string BusinessName,
//        string OwnerName,
//        string? BusinessPhone,
//        string? BusinessEmail,
//        bool IsActive,
//        string CreatedAt,
//        string? ShopType,
//        string SubscriptionPlan,
//        string? SubscriptionStartDate,
//        string? SubscriptionExpiry,
//        int DaysLeft,
//        int UserCount
//    );
//}

namespace Shop.Web.API.Models.Domain
{
    public sealed class ShopListRecord
    {
        public int BusinessAccountId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string? BusinessPhone { get; set; }
        public string? BusinessEmail { get; set; }
        public bool IsActive { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string? ShopType { get; set; }
        public string SubscriptionPlan { get; set; } = string.Empty;
        public string? SubscriptionStartDate { get; set; }
        public string? SubscriptionExpiry { get; set; }
        public int DaysLeft { get; set; }
        public int UserCount { get; set; }
    }
}
