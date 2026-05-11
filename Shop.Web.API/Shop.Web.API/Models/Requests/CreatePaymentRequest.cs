//namespace Shop.Web.API.Models.Requests
//{
//    public sealed record CreatePaymentRequest(
//        int BusinessAccountId,
//        decimal Amount,
//        string Currency,
//        string Plan,
//        string PaymentStatus,
//        string? PaymentMethod,
//        string? TransactionRef,
//        string? Notes,
//        int DurationMonths          // SP computes start/end from this
//    );
//}

namespace Shop.Web.API.Models.Requests
{
    public sealed record CreatePaymentRequest(
        int BusinessAccountId,
        string Plan,               // → @SubscriptionPlan
        int DurationMonths,
        decimal Amount,
        string Currency,
        string PaymentStatus,      // 'Paid' | 'Pending' | 'Failed'
        string? TransactionRef,     // → @TransactionReference
        string? Notes,
        string? PaymentMethod
    );
}
