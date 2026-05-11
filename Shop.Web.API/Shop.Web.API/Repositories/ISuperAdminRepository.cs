using Shop.Web.API.Models.Domain;

namespace Shop.Web.API.Repositories
{
    public interface ISuperAdminRepository
    {
        Task<SuperAdminDashboardRecord?> GetDashboardAsync();
        Task<(int TotalCount, IEnumerable<ShopListRecord> Items)> GetShopsAsync(
            int page, int pageSize, string? search, string? planFilter, string? statusFilter);
        Task<ShopDetailRecord?> GetShopByIdAsync(int businessAccountId);
        Task<ShopDetailRecord?> UpdateSubscriptionAsync(
            int businessAccountId, string plan,
            DateOnly startDate, DateOnly expiry,
            int updatedByUserId);
        Task<ShopDetailRecord?> ToggleShopStatusAsync(int businessAccountId, int updatedByUserId);

        // ISuperAdminRepository.cs — replace the two payment signatures
        Task<(int TotalCount, IEnumerable<SubscriptionPaymentRecord> Items)> GetPaymentsAsync(
            int page, int pageSize,
            int? businessAccountId, string? plan, string? status);

        Task<SubscriptionPaymentRecord> CreatePaymentAsync(
            int businessAccountId, decimal amount, string currency,
            string plan, string paymentStatus,
            string? paymentMethod, string? transactionReference, string? notes,
            int durationMonths, int createdByUserId);

        Task<(
            RevenueStatsRecord Kpis,
            IEnumerable<MonthlyRevenueRecord> Monthly,
            IEnumerable<PlanRevenueRecord> ByPlan,
            IEnumerable<RecentPaymentRecord> Recent
        )> GetRevenueStatsAsync();

        Task<SubscriptionPaymentRecord> UpdatePaymentAsync(
            int paymentId, string plan, string paymentStatus, int updatedByUserId);

    }
}
