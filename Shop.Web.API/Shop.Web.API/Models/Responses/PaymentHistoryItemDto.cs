//namespace Shop.Web.API.Models.Responses
//{
//    public sealed class PaymentHistoryItemDto
//    {
//        public int PaymentId { get; init; }
//        public int BusinessAccountId { get; init; }
//        public string BusinessName { get; init; } = "";
//        public string OwnerName { get; init; } = "";
//        public string Plan { get; init; } = "";
//        public int DurationMonths { get; init; }
//        public decimal Amount { get; init; }
//        public string Currency { get; init; } = "INR";
//        public string PaymentStatus { get; init; } = "";
//        public string? PaymentMethod { get; init; }
//        public string? TransactionRef { get; init; }
//        public string? Notes { get; init; }
//        public string PaymentDate { get; init; } = "";
//        public string SubscriptionStartDate { get; init; } = "";
//        public string SubscriptionExpiry { get; init; } = "";
//    }
//}

namespace Shop.Web.API.Models.Responses
{
    public sealed class PaymentHistoryItemDto
    {
        public int PaymentId { get; init; }
        public int BusinessAccountId { get; init; }
        public string BusinessName { get; init; } = "";
        public string OwnerName { get; init; } = "";
        public string Plan { get; init; } = "";
        public int DurationMonths { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = "INR";
        public string PaymentStatus { get; init; } = "";
        public string? PaymentMethod { get; init; }
        public string? TransactionRef { get; init; }
        public string? Notes { get; init; }
        public string PaymentDate { get; init; } = "";
        public string SubscriptionStartDate { get; init; } = "";
        public string SubscriptionExpiry { get; init; } = "";
    }
}