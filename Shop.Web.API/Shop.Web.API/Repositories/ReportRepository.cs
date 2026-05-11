using Dapper;
using Microsoft.Data.SqlClient;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using System.Data;
using System.Text.Json;

namespace Shop.Web.API.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly string _conn;
        private readonly ILogger<ReportRepository> _logger;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };

        public ReportRepository(IConfiguration config, ILogger<ReportRepository> logger)
        {
            _conn = config.GetConnectionString("Default")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
            _logger = logger;
        }

        public async Task<(int TotalCount, List<CustomerReportRow> Items)> GetCustomerWiseAsync(
            int businessAccountId, CustomerReportParams p)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_Reports_CustomerWise",
                    new
                    {
                        BusinessAccountId = businessAccountId,
                        DateFrom = p.DateFrom.HasValue ? (DateTime?)p.DateFrom.Value.ToDateTime(TimeOnly.MinValue) : null,
                        DateTo = p.DateTo.HasValue ? (DateTime?)p.DateTo.Value.ToDateTime(TimeOnly.MaxValue) : null,
                        Search = string.IsNullOrWhiteSpace(p.Search) ? null : p.Search,
                        SortBy = p.SortBy,
                        SortDir = p.SortDir,
                        Page = p.Page,
                        PageSize = p.PageSize,
                    },
                    commandType: CommandType.StoredProcedure);

                // Result set 1 — total filtered count
                var totalCount = await multi.ReadSingleAsync<int>();

                // Result set 2 — paged rows
                var items = (await multi.ReadAsync<CustomerReportRow>()).AsList();

                return (totalCount, items);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetCustomerWiseAsync");
                throw;
            }
        }

        // ── Purchase-wise report ──────────────────────────────────────────

        public async Task<(
            PurchaseReportGlobalSummary GlobalSummary,
            int TotalCount,
            List<PurchaseReportRow> Items
        )> GetPurchaseWiseAsync(int businessAccountId, PurchaseReportParams p)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_Reports_PurchaseWise",
                    new
                    {
                        BusinessAccountId = businessAccountId,
                        DateFrom = p.DateFrom.HasValue ? (DateTime?)p.DateFrom.Value.ToDateTime(TimeOnly.MinValue) : null,
                        DateTo = p.DateTo.HasValue ? (DateTime?)p.DateTo.Value.ToDateTime(TimeOnly.MaxValue) : null,
                        Search = string.IsNullOrWhiteSpace(p.Search) ? null : p.Search,
                        Page = p.Page,
                        PageSize = p.PageSize,
                    },
                    commandType: CommandType.StoredProcedure);

                // Result set 1 — global summary (full-period totals, all pages)
                var globalSummary = await multi.ReadSingleAsync<PurchaseReportGlobalSummary>();

                // Result set 2 — filtered count (for pagination bar)
                var totalCount = await multi.ReadSingleAsync<int>();

                // Result set 3 — paged rows
                var items = (await multi.ReadAsync<PurchaseReportRow>()).AsList();

                return (globalSummary, totalCount, items);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetPurchaseWiseAsync");
                throw;
            }
        }
    }
}
