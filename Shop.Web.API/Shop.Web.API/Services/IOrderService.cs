using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;

namespace Shop.Web.API.Services
{
    public interface IOrderService
    {
        //Task<List<OrderDto>> GetAllAsync(int businessAccountId, DateTime? from, DateTime? to);
        Task<OrderDto> GetByIdAsync(int orderId, int businessAccountId);
        //Task<List<OrderDto>> GetByCustomerAsync(int customerId, int businessAccountId);
        Task<TodaySummaryDto> GetTodaySummaryAsync(int businessAccountId);
        Task<OrderDto> CreateAsync(int businessAccountId, int requestingUserId, CreateOrderRequest req, string ip);
        Task<OrderDto> UpdateAsync(int orderId, int businessAccountId, int requestingUserId, UpdateOrderRequest req, string ip);
        //Task<List<PaymentDto>> GetPaymentsByCustomerAsync(int customerId, int businessAccountId);
        Task<int> RecordPaymentAsync(int businessAccountId, int requestingUserId, int customerId, RecordPaymentRequest req, string ip);
        Task SoftDeleteAsync(int orderId, int businessAccountId, int requestingUserId, string ip);
        Task<List<PaymentDto>> GetPaymentsByOrderAsync(int orderId, int businessAccountId);

        Task<PagedResponse<OrderDto>> GetAllAsync(
            int businessAccountId, DateTime? from, DateTime? to, int page, int pageSize, string? status = null, string? search = null);

        Task<PagedResponse<OrderDto>> GetByCustomerAsync(
            int customerId, int businessAccountId, int page, int pageSize);

        Task<PagedResponse<PaymentDto>> GetPaymentsByCustomerAsync(
            int customerId, int businessAccountId, int page, int pageSize);

        Task<OrderDashboardSummaryDto> GetDashboardSummaryAsync(int businessAccountId, DateTime? from);

        Task<PagedOrderHistoryResponse> GetByCustomerFilteredAsync(
            int customerId,
            int businessAccountId,
            int page,
            int pageSize,
            string? dateFrom = null,
            string? dateTo = null,
            string? search = null,
            string? status = null);
    }
}
