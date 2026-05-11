namespace Shop.Web.API.Models.Domain
{
    //public sealed record SubscriptionPaymentRecord(
    //    int PaymentId,
    //    int BusinessAccountId,
    //    string BusinessName,
    //    string OwnerName,
    //    string Plan,                   // SP aliases SubscriptionPlan → Plan
    //    int DurationMonths,
    //    decimal Amount,
    //    string Currency,
    //    string PaymentStatus,
    //    string? PaymentMethod,
    //    string? TransactionRef,         // SP aliases TransactionReference → TransactionRef
    //    string? Notes,
    //    string PaymentDate,            // SP: ISNULL(PaidAt, CreatedAt)
    //    string SubscriptionStartDate,
    //    string SubscriptionExpiry
    //);

    public sealed record SubscriptionPaymentRecord(
        int PaymentId,
        int BusinessAccountId,
        string BusinessName,
        string OwnerName,
        string Plan,                   // SP: SubscriptionPlan AS [Plan]
        int DurationMonths,
        decimal Amount,
        string Currency,
        string PaymentStatus,
        string? PaymentMethod,
        string? TransactionRef,         // SP: TransactionReference AS TransactionRef
        string? Notes,
        string PaymentDate,            // SP: ISNULL(PaidAt, CreatedAt/UpdatedAt)
        string SubscriptionStartDate,  // SP: PeriodStart
        string SubscriptionExpiry      // SP: PeriodEnd
    );
}
