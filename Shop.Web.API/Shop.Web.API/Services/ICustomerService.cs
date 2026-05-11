using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;

namespace Shop.Web.API.Services
{
    public interface ICustomerService
    {
        //Task<List<CustomerDto>> GetAllAsync(int businessAccountId, int requestingUserId);
        // ICustomerService
        //Task<PagedResponse<CustomerDto>> GetAllAsync(
        //    int businessAccountId, int requestingUserId,
        //    int page, int pageSize,
        //    string? search, string? status, string? type, bool? hasDue);

        Task<PagedCustomerResponse> GetAllAsync(
            int businessAccountId,
            int requestingUserId,
            int page,
            int pageSize,
            string? search,
            string? status,
            string? type,
            bool? hasDue);

        Task<CustomerDto> GetByIdAsync(int customerId, int businessAccountId);
        Task<CustomerDto> CreateAsync(int businessAccountId, int requestingUserId, CreateCustomerRequest req, string ip);
        Task<CustomerDto> UpdateAsync(int customerId, int businessAccountId, int requestingUserId, UpdateCustomerRequest req, string ip);
        Task<CustomerDto> ToggleStatusAsync(int customerId, int businessAccountId, int requestingUserId, string ip);
        Task<CustomerSummaryDto> GetSummaryAsync(int businessAccountId, int requestingUserId);
    }
}