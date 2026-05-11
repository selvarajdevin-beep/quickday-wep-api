namespace Shop.Web.API.Models.Responses
{
    public sealed class RevenueStatsDto
    {
        // KPIs
        public decimal TotalRevenue { get; init; }
        public decimal CurrentMonthRevenue { get; init; }
        public decimal PreviousMonthRevenue { get; init; }
        public decimal PendingRevenue { get; init; }
        public int TotalTransactions { get; init; }
        public int ActivePaidSubscriptions { get; init; }
        public decimal AvgRevenuePerBusiness { get; init; }

        // Derived in service — positive = growth, negative = decline
        public decimal MoMChangePercent { get; init; }

        // Charts
        public List<MonthlyRevenueDto> MonthlyRevenue { get; init; } = [];
        public List<PlanRevenueDto> PlanRevenue { get; init; } = [];

        // Recent transactions feed
        public List<RecentPaymentDto> RecentPayments { get; init; } = [];
    }

    public sealed class MonthlyRevenueDto
    {
        public string MonthLabel { get; init; } = "";
        public string MonthDisplay { get; init; } = "";
        public decimal Revenue { get; init; }
        public int Transactions { get; init; }
    }

    public sealed class PlanRevenueDto
    {
        public string Plan { get; init; } = "";
        public decimal Revenue { get; init; }
        public int Transactions { get; init; }
        public decimal RevenuePercent { get; init; }
    }

    public sealed class RecentPaymentDto
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
