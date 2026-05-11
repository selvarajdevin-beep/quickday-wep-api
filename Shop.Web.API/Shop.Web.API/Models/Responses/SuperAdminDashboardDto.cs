namespace Shop.Web.API.Models.Responses
{
    public sealed class SuperAdminDashboardDto
    {
        public int TotalShops { get; init; }
        public int ActiveShops { get; init; }
        public int ExpiringIn30Days { get; init; }
        public int ExpiredShops { get; init; }
        public int FreePlanCount { get; init; }
        public int BasicPlanCount { get; init; }
        public int ProPlanCount { get; init; }
    }
}
