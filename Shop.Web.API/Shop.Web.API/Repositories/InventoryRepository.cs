using Dapper;
using Microsoft.Data.SqlClient;
using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using System.Data;

namespace Shop.Web.API.Repositories
{
    public class InventoryRepository: IInventoryRepository
    {
        private readonly string _conn;
        private readonly ILogger<InventoryRepository> _logger;

        public InventoryRepository(IConfiguration config, ILogger<InventoryRepository> logger)
        {
            _conn = config.GetConnectionString("Default")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
            _logger = logger;
        }

        //public async Task<List<InventoryLogRecord>> GetLogsAsync(
        // int businessAccountId, DateOnly? from, DateOnly? to)
        //{
        //    using var db = new SqlConnection(_conn);
        //    try
        //    {
        //        var rows = await db.QueryAsync<InventoryLogRecord>(
        //            "dbo.usp_Inventory_GetLogs",
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
        //        _logger.LogError(ex, "SQL error in GetLogsAsync BusinessAccountId={Id}", businessAccountId);
        //        throw;
        //    }
        //}

        public async Task<(List<InventoryLogRecord> Items, int TotalCount)> GetLogsAsync(
            int businessAccountId,
            DateOnly? from, DateOnly? to, string? search,
            int page, int pageSize)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_Inventory_GetLogs",
                    new
                    {
                        BusinessAccountId = businessAccountId,
                        DateFrom = from.HasValue ? (DateTime?)from.Value.ToDateTime(TimeOnly.MinValue) : null,
                        DateTo = to.HasValue ? (DateTime?)to.Value.ToDateTime(TimeOnly.MaxValue) : null,
                        Search = string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : search,
                        Page = page,
                        PageSize = pageSize,
                    },
                    commandType: CommandType.StoredProcedure);

                var totalCount = await multi.ReadSingleAsync<int>();
                var items = (await multi.ReadAsync<InventoryLogRecord>()).AsList();
                return (items, totalCount);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetLogsAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }

        public async Task<ProductRecord?> AdjustStockAsync(
            int productId, int businessAccountId, int requestingUserId,
            AdjustStockRequest req, string ip)
        {
            using var db = new SqlConnection(_conn);
            var p = new DynamicParameters();
            p.Add("@ProductId", productId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@Quantity", req.Quantity);
            p.Add("@Type", req.Type);
            p.Add("@Reason", req.Reason.Trim());
            p.Add("@Reference", req.Reference?.Trim());
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<ProductRecord>(
                    "dbo.usp_Inventory_AdjustStock", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "INVENTORY");
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in AdjustStockAsync ProductId={Id}", productId);
                throw;
            }
        }

        public async Task<ProductRecord?> UpdateMinStockAlertAsync(
            int productId, int businessAccountId, int requestingUserId, int minStock, string ip)
        {
            using var db = new SqlConnection(_conn);
            var p = new DynamicParameters();
            p.Add("@ProductId", productId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@MinStockAlert", minStock);
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<ProductRecord>(
                    "dbo.usp_Inventory_UpdateMinStockAlert", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "INVENTORY");
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in UpdateMinStockAlertAsync ProductId={Id}", productId);
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
