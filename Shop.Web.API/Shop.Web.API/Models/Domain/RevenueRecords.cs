namespace Shop.Web.API.Models.Domain
{
    /// <summary>Result set 1 — KPI summary row.</summary>
    public sealed class RevenueStatsRecord
    {
        public decimal TotalRevenue { get; init; }
        public decimal CurrentMonthRevenue { get; init; }
        public decimal PreviousMonthRevenue { get; init; }
        public decimal PendingRevenue { get; init; }
        public int TotalTransactions { get; init; }
        public int ActivePaidSubscriptions { get; init; }
        public decimal AvgRevenuePerBusiness { get; init; }
    }

    /// <summary>Result set 2 — one row per calendar month (last 12).</summary>
    public sealed class MonthlyRevenueRecord
    {
        public string MonthLabel { get; init; } = "";  // "YYYY-MM"
        public string MonthDisplay { get; init; } = "";  // "Jan 25"
        public decimal Revenue { get; init; }
        public int Transactions { get; init; }
    }

    /// <summary>Result set 3 — revenue split by plan.</summary>
    public sealed class PlanRevenueRecord
    {
        public string Plan { get; init; } = "";
        public decimal Revenue { get; init; }
        public int Transactions { get; init; }
        public decimal RevenuePercent { get; init; }
    }

    /// <summary>Result set 4 — recent paid transactions.</summary>
    public sealed class RecentPaymentRecord
    {
        public int PaymentId { get; init; }
        public string BusinessName { get; init; } = "";
        public string OwnerName { get; init; } = "";
        public string Plan { get; init; } = "";
        public decimal Amount { get; init; }
        public string Currency { get; init; } = "INR";
        public int DurationMonths { get; init; }
        public string? PaymentMethod { get; init; }
        public string PaymentDate { get; init; } = "";
    }

}
