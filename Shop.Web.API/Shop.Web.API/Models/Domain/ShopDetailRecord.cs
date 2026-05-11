namespace Shop.Web.API.Models.Domain
{
    public sealed record ShopDetailRecord(
        int BusinessAccountId,
        string BusinessName,
        string OwnerName,
        string? BusinessPhone,
        string? BusinessEmail,
        string? Address,
        string? Gstin,
        bool IsActive,
        string CreatedAt,
        string? ShopType,
        string SubscriptionPlan,
        string? SubscriptionStartDate,
        string? SubscriptionExpiry,
        string? ThemeColor,
        string? Currency,
        int DaysLeft,
        int UserCount,
        int TotalOrders
    );
}
