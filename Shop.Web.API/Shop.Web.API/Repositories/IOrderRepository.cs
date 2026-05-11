using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;

namespace Shop.Web.API.Repositories
{
    public interface IOrderRepository
    {
        //Task<List<OrderRecord>> GetAllAsync(int businessAccountId, DateTime? from, DateTime? to);
        Task<OrderRecord?> GetByIdAsync(int orderId, int businessAccountId);
        //Task<List<OrderRecord>> GetByCustomerAsync(int customerId, int businessAccountId);
        Task<TodaySummaryRecord> GetTodaySummaryAsync(int businessAccountId);
        Task<OrderRecord?> CreateAsync(int businessAccountId, int requestingUserId, CreateOrderRequest req, string ip);
        Task<OrderRecord?> UpdateAsync(int orderId, int businessAccountId, int requestingUserId, UpdateOrderRequest req, string ip);
        //Task<List<PaymentRecord>> GetPaymentsByCustomerAsync(int customerId, int businessAccountId);
        Task<int> RecordPaymentAsync(int businessAccountId, int requestingUserId, int customerId, RecordPaymentRequest req, string ip);
        Task<int> SoftDeleteAsync(int orderId, int businessAccountId, int requestingUserId, string ip);
        Task<List<PaymentRecord>> GetPaymentsByOrderAsync(int orderId, int businessAccountId);

        Task<(List<OrderRecord> Items, int TotalCount)> GetAllAsync(
            int businessAccountId, DateTime? from, DateTime? to, int page, int pageSize, string? status = null, string? search = null);

        Task<(List<OrderRecord> Items, int TotalCount)> GetByCustomerAsync(
            int customerId, int businessAccountId, int page, int pageSize);

        Task<(List<PaymentRecord> Items, int TotalCount)> GetPaymentsByCustomerAsync(
            int customerId, int businessAccountId, int page, int pageSize);

        Task<OrderDashboardSummaryDto> GetDashboardSummaryAsync(int businessAccountId, DateTime? from);

        Task<(List<OrderRecord> Items, int TotalCount, decimal TotalSales, decimal TotalPaid, decimal TotalDue)>
        GetByCustomerFilteredAsync(
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
