using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;

namespace Shop.Web.API.Repositories
{
    public interface ICustomerRepository
    {
        //Task<List<CustomerRecord>> GetAllAsync(int businessAccountId, int requestingUserId);

        //Task<(List<CustomerRecord> Items, int TotalCount)> GetAllAsync(
        //    int businessAccountId, int requestingUserId,
        //    int page, int pageSize,
        //    string? search, string? status, string? type, bool? hasDue);

        Task<(CustomerSummaryDto Summary, int FilteredCount, List<CustomerRecord> Items)> GetAllAsync(
            int businessAccountId,
            int requestingUserId,
            int page,
            int pageSize,
            string? search,
            string? status,
            string? type,
            bool? hasDue);

        Task<CustomerRecord?> GetByIdAsync(int customerId, int businessAccountId);
        Task<CustomerRecord?> CreateAsync(int businessAccountId, int requestingUserId, CreateCustomerRequest req, string ip);
        Task<CustomerRecord?> UpdateAsync(int customerId, int businessAccountId, int requestingUserId, UpdateCustomerRequest req, string ip);
        Task<CustomerRecord?> ToggleStatusAsync(int customerId, int businessAccountId, int requestingUserId, string ip);
        // ICustomerRepo.cs
        Task<CustomerSummaryDto> GetSummaryAsync(int businessAccountId, int requestingUserId);
    }
}
