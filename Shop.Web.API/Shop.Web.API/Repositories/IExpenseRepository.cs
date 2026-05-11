using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;

namespace Shop.Web.API.Repositories
{
    public interface IExpenseRepository
    {
        //Task<List<ExpenseRecord>> GetAllAsync(int businessAccountId, DateOnly? from, DateOnly? to);

        Task<(List<ExpenseRecord> Items, int TotalCount, decimal TotalAmount)> GetAllAsync(
            int businessAccountId,
            string? type,
            string? search,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page,
            int pageSize);

        Task<ExpenseRecord?> GetByIdAsync(int expenseId, int businessAccountId);
        Task<(decimal total, List<ExpenseSummaryTypeRecord> byType)> GetSummaryAsync(int businessAccountId);
        Task<ExpenseRecord?> CreateAsync(int businessAccountId, int requestingUserId, CreateExpenseRequest req, string ip);
        Task<ExpenseRecord?> UpdateAsync(int expenseId, int businessAccountId, int requestingUserId, UpdateExpenseRequest req, string ip);
        Task DeleteAsync(int expenseId, int businessAccountId, int requestingUserId, string ip);
    }
}
