namespace Shop.Web.API.Models.Domain
{
    public sealed record SuperAdminDashboardRecord(
        int TotalShops,
        int ActiveShops,
        int ExpiringIn30Days,
        int ExpiredShops,
        int FreePlanCount,
        int BasicPlanCount,
        int ProPlanCount
    );
}
