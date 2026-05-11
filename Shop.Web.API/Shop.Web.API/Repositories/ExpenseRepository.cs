using Dapper;
using Microsoft.Data.SqlClient;
using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using System.Data;

namespace Shop.Web.API.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly string _conn;
        private readonly ILogger<ExpenseRepository> _logger;

        public ExpenseRepository(IConfiguration config, ILogger<ExpenseRepository> logger)
        {
            _conn = config.GetConnectionString("Default")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
            _logger = logger;
        }

        // ── Get All ───────────────────────────────────────────────

        //public async Task<List<ExpenseRecord>> GetAllAsync(
        //    int businessAccountId, DateOnly? from, DateOnly? to)
        //{
        //    using var db = new SqlConnection(_conn);
        //    try
        //    {
        //        var rows = await db.QueryAsync<ExpenseRecord>(
        //            "dbo.usp_Expenses_GetAll",
        //            new
        //            {
        //                BusinessAccountId = businessAccountId,
        //                DateFrom = from.HasValue ? (DateTime?)from.Value.ToDateTime(TimeOnly.MinValue) : null,
        //                DateTo = to.HasValue ? (DateTime?)to.Value.ToDateTime(TimeOnly.MaxValue) : null,
        //            },
        //            commandType: CommandType.StoredProcedure);
        //        return rows.AsList();
        //    }
        //    catch (SqlException ex)
        //    {
        //        _logger.LogError(ex, "SQL error in GetAllAsync BusinessAccountId={Id}", businessAccountId);
        //        throw;
        //    }
        //}

        public async Task<(List<ExpenseRecord> Items, int TotalCount, decimal TotalAmount)> GetAllAsync(
            int businessAccountId,
            string? type,
            string? search,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page,
            int pageSize)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_Expenses_GetAll",
                    new
                    {
                        BusinessAccountId = businessAccountId,
                        Type = string.IsNullOrWhiteSpace(type) ? (object)DBNull.Value : type,
                        Search = string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : search,
                        DateFrom = dateFrom.HasValue ? dateFrom.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value,
                        DateTo = dateTo.HasValue ? dateTo.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value,
                        Page = page,
                        PageSize = pageSize,
                    },
                    commandType: CommandType.StoredProcedure);

                var totalCount = await multi.ReadSingleAsync<int>();
                var totalAmount = await multi.ReadSingleAsync<decimal>();
                var items = (await multi.ReadAsync<ExpenseRecord>()).AsList();
                return (items, totalCount, totalAmount);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex,
                    "SQL error in Expenses.GetAllAsync BusinessAccountId={Id}",
                    businessAccountId);
                throw;
            }
        }


        // ── Get By Id ─────────────────────────────────────────────

        public async Task<ExpenseRecord?> GetByIdAsync(int expenseId, int businessAccountId)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                return await db.QuerySingleOrDefaultAsync<ExpenseRecord>(
                    "dbo.usp_Expenses_GetById",
                    new { ExpenseId = expenseId, BusinessAccountId = businessAccountId },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetByIdAsync ExpenseId={Id}", expenseId);
                throw;
            }
        }

        // ── Get Summary ───────────────────────────────────────────
        // SP returns two result sets:
        //   1. Scalar row: TotalThisMonth
        //   2. Row set:    Type, Amount (per category)

        public async Task<(decimal total, List<ExpenseSummaryTypeRecord> byType)> GetSummaryAsync(
            int businessAccountId)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_Expenses_GetSummary",
                    new { BusinessAccountId = businessAccountId },
                    commandType: CommandType.StoredProcedure);

                var totalRow = await multi.ReadSingleOrDefaultAsync<ExpenseSummaryTotalRecord>();
                var byType = (await multi.ReadAsync<ExpenseSummaryTypeRecord>()).AsList();

                return (totalRow?.TotalThisMonth ?? 0m, byType);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetSummaryAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }

        // ── Create ────────────────────────────────────────────────

        public async Task<ExpenseRecord?> CreateAsync(
            int businessAccountId, int requestingUserId,
            CreateExpenseRequest req, string ip)
        {
            using var db = new SqlConnection(_conn);
            var p = new DynamicParameters();
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@Type", req.Type);
            p.Add("@Amount", req.Amount);
            //p.Add("@Date", req.Date.ToDateTime(TimeOnly.MinValue));
            p.Add("@Date", req.Date);

            p.Add("@Notes", req.Notes?.Trim());
            p.Add("@IpAddress", ip);
            p.Add("@NewExpenseId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<ExpenseRecord>(
                    "dbo.usp_Expenses_Create", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "EXPENSE");
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in CreateAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }

        // ── Update ────────────────────────────────────────────────

        public async Task<ExpenseRecord?> UpdateAsync(
            int expenseId, int businessAccountId, int requestingUserId,
            UpdateExpenseRequest req, string ip)
        {
            using var db = new SqlConnection(_conn);
            var rowVersionBytes = DecodeRowVersion(req.RowVersion);

            var p = new DynamicParameters();
            p.Add("@ExpenseId", expenseId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@Type", req.Type);
            p.Add("@Amount", req.Amount);
            //p.Add("@Date", req.Date.ToDateTime(TimeOnly.MinValue));
            p.Add("@Date", req.Date);
            p.Add("@Notes", req.Notes?.Trim());
            p.Add("@RowVersion", rowVersionBytes, DbType.Binary, size: 8);
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<ExpenseRecord>(
                    "dbo.usp_Expenses_Update", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "EXPENSE", code => code == 9009 ? 409 : 400);
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in UpdateAsync ExpenseId={Id}", expenseId);
                throw;
            }
        }

        // ── Delete ────────────────────────────────────────────────

        public async Task DeleteAsync(
            int expenseId, int businessAccountId, int requestingUserId, string ip)
        {
            using var db = new SqlConnection(_conn);
            var p = new DynamicParameters();
            p.Add("@ExpenseId", expenseId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                await db.ExecuteAsync(
                    "dbo.usp_Expenses_Delete", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "EXPENSE");
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in DeleteAsync ExpenseId={Id}", expenseId);
                throw;
            }
        }

        // ── Helpers ───────────────────────────────────────────────

        private static byte[] DecodeRowVersion(string base64)
        {
            try { return Convert.FromBase64String(base64); }
            catch { throw new ArgumentException("Invalid row version format.", nameof(base64)); }
        }

        private static void ThrowIfSpError(
            DynamicParameters p, string prefix, Func<int, int>? httpResolver = null)
        {
            int code = p.Get<int>("@ErrorCode");
            string message = p.Get<string>("@ErrorMessage") ?? string.Empty;
            if (code == 0) return;
            int httpStatus = httpResolver?.Invoke(code) ?? 400;
            throw new AppException(message, $"{prefix}_{code}", httpStatus);
        }
    }

}
