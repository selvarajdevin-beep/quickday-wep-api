using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Repositories;

namespace Shop.Web.API.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _repo;
        private readonly ILogger<ExpenseService> _logger;

        public ExpenseService(IExpenseRepository repo, ILogger<ExpenseService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        //public async Task<List<ExpenseDto>> GetAllAsync(
        //    int businessAccountId, DateOnly? from, DateOnly? to)
        //{
        //    var records = await _repo.GetAllAsync(businessAccountId, from, to);
        //    return records.Select(MapToDto).ToList();
        //}

        public async Task<PagedResponse<ExpenseDto>> GetAllAsync(
            int businessAccountId,
            int userId,
            string? type,
            string? search,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page,
            int pageSize)
        {
            var (items, totalCount, totalAmount) = await _repo.GetAllAsync(
                businessAccountId, type, search, dateFrom, dateTo, page, pageSize);

            var totalPages = pageSize > 0
                ? (int)Math.Ceiling((double)totalCount / pageSize)
                : 1;

            return new PagedResponse<ExpenseDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                TotalAmount = totalAmount,
                Page = page,
                PageSize = pageSize
            };
        }


        public async Task<ExpenseDto> GetByIdAsync(int expenseId, int businessAccountId)
        {
            var record = await _repo.GetByIdAsync(expenseId, businessAccountId);
            if (record is null)
                throw new NotFoundException($"Expense with ID {expenseId} not found.");
            return MapToDto(record);
        }

        public async Task<ExpenseSummaryDto> GetSummaryAsync(int businessAccountId)
        {
            var (total, byType) = await _repo.GetSummaryAsync(businessAccountId);
            return new ExpenseSummaryDto
            {
                TotalThisMonth = total,
                ByType = byType.Select(r => new ExpenseSummaryByTypeDto
                {
                    Type = r.Type,
                    Amount = r.Amount,
                }).ToList(),
            };
        }

        public async Task<ExpenseDto> CreateAsync(
            int businessAccountId, int requestingUserId,
            CreateExpenseRequest req, string ip)
        {
            var record = await _repo.CreateAsync(businessAccountId, requestingUserId, req, ip);
            if (record is null)
                throw new AppException("Failed to save expense. Please try again.", "EXPENSE_UNEXPECTED");

            _logger.LogInformation(
                "Expense created: #{Id} Type={Type} Amount=₹{Amount} Date={Date} BusinessAccountId={BizId} by UserId={UserId}",
                record.Id, req.Type, req.Amount, req.Date, businessAccountId, requestingUserId);

            return MapToDto(record);
        }

        public async Task<ExpenseDto> UpdateAsync(
            int expenseId, int businessAccountId, int requestingUserId,
            UpdateExpenseRequest req, string ip)
        {
            var record = await _repo.UpdateAsync(expenseId, businessAccountId, requestingUserId, req, ip);
            if (record is null)
                throw new NotFoundException($"Expense with ID {expenseId} not found.");

            _logger.LogInformation(
                "Expense updated: #{Id} by UserId={UserId}", expenseId, requestingUserId);

            return MapToDto(record);
        }

        public async Task DeleteAsync(
            int expenseId, int businessAccountId, int requestingUserId, string ip)
        {
            await _repo.DeleteAsync(expenseId, businessAccountId, requestingUserId, ip);

            _logger.LogInformation(
                "Expense deleted: #{Id} by UserId={UserId}", expenseId, requestingUserId);
        }

        // ── Mapper ────────────────────────────────────────────────

        private static ExpenseDto MapToDto(ExpenseRecord r) => new()
        {
            Id = r.Id,
            BusinessAccountId = r.BusinessAccountId,
            Type = r.Type,
            Amount = r.Amount,
            Date = r.Date,
            Notes = r.Notes,
            CreatedAt = r.CreatedAt,
            //RowVersion = HexToBase64(r.RowVersion),
            RowVersion = r.RowVersion is not null
                                ? Convert.ToBase64String(r.RowVersion)
                                : string.Empty
        };

        private static string HexToBase64(string? hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return string.Empty;
            try
            {
                var h = hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? hex[2..] : hex;
                return Convert.ToBase64String(Convert.FromHexString(h));
            }
            catch { return string.Empty; }
        }
    }

}
