namespace Shop.Web.API.Models.Responses
{
    public class ShopListItemDto
    {
        public int BusinessAccountId { get; init; }
        public string BusinessName { get; init; } = "";
        public string OwnerName { get; init; } = "";
        public string? BusinessPhone { get; init; }
        public string? BusinessEmail { get; init; }
        public bool IsActive { get; init; }
        public string CreatedAt { get; init; } = "";
        public string? ShopType { get; init; }
        public string SubscriptionPlan { get; init; } = "Free";
        public string? SubscriptionStartDate { get; init; }
        public string? SubscriptionExpiry { get; init; }
        public int DaysLeft { get; init; }
        public int UserCount { get; init; }
        // Derived
        public string SubscriptionStatus => DaysLeft < 0 ? "Expired"
                                           : DaysLeft <= 30 ? "Expiring"
                                           : "Active";
    }
}
