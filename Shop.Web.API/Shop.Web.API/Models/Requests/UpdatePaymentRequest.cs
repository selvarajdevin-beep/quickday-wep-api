namespace Shop.Web.API.Models.Requests
{
    public sealed record UpdatePaymentRequest(
        string Plan,          // → @SubscriptionPlan
        string PaymentStatus  // 'Paid' | 'Pending' | 'Failed'
    );
}