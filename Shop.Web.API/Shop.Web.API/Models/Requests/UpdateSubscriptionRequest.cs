namespace Shop.Web.API.Models.Requests
{
    public sealed record UpdateSubscriptionRequest(
        string SubscriptionPlan,
        string SubscriptionStartDate,   // YYYY-MM-DD
        string SubscriptionExpiry       // YYYY-MM-DD
    );
}
