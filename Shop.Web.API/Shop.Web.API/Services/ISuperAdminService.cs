using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;

namespace Shop.Web.API.Services
{
    public interface ISuperAdminService
    {
        Task<SuperAdminDashboardDto> GetDashboardAsync();
        Task<PagedShopsDto> GetShopsAsync(
            int page, int pageSize, string? search, string? planFilter, string? statusFilter);
        Task<ShopDetailDto> GetShopByIdAsync(int businessAccountId);
        Task<ShopDetailDto> UpdateSubscriptionAsync(
            int businessAccountId, UpdateSubscriptionRequest req, int updatedByUserId);
        Task<ShopDetailDto> ToggleShopStatusAsync(int businessAccountId, int updatedByUserId);

        // Services/ISuperAdminService.cs  — add to existing interface
        public Task<PagedPaymentsDto> GetPaymentsAsync(int page, int pageSize,
            int? businessAccountId, string? plan, string? status);

        // ISuperAdminService.cs — replace CreatePayment signature
        Task<PaymentHistoryItemDto> CreatePaymentAsync(
            CreatePaymentRequest req, int createdByUserId);

        Task<RevenueStatsDto> GetRevenueStatsAsync();

        Task<PaymentHistoryItemDto> UpdatePaymentAsync(
            int paymentId, UpdatePaymentRequest req, int updatedByUserId);
    }
}
