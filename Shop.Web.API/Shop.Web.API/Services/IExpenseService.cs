using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;

namespace Shop.Web.API.Services
{
    public interface IExpenseService
    {
        //Task<List<ExpenseDto>> GetAllAsync(int businessAccountId, DateOnly? from, DateOnly? to);

        Task<PagedResponse<ExpenseDto>> GetAllAsync(
            int businessAccountId,
            int userId,
            string? type,
            string? search,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page,
            int pageSize);

        Task<ExpenseDto> GetByIdAsync(int expenseId, int businessAccountId);
        Task<ExpenseSummaryDto> GetSummaryAsync(int businessAccountId);
        Task<ExpenseDto> CreateAsync(int businessAccountId, int requestingUserId, CreateExpenseRequest req, string ip);
        Task<ExpenseDto> UpdateAsync(int expenseId, int businessAccountId, int requestingUserId, UpdateExpenseRequest req, string ip);
        Task DeleteAsync(int expenseId, int businessAccountId, int requestingUserId, string ip);
    }
}
