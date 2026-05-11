using Dapper;
using Microsoft.Data.SqlClient;
using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using System.Data;
using System.Text.Json;

namespace Shop.Web.API.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly string _conn;
        private readonly ILogger<PurchaseRepository> _logger;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };

        public PurchaseRepository(IConfiguration config, ILogger<PurchaseRepository> logger)
        {
            _conn = config.GetConnectionString("Default")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
            _logger = logger;
        }

        // ── Get All ───────────────────────────────────────────────

        //public async Task<List<PurchaseRecord>> GetAllAsync(
        //    int businessAccountId, int requestingUserId,
        //    DateOnly? from = null, DateOnly? to = null)
        //{
        //    using var db = new SqlConnection(_conn);
        //    try
        //    {
        //        var rows = await db.QueryAsync<PurchaseRecord>(
        //            "dbo.usp_Purchases_GetAll",
        //            new
        //            {
        //                BusinessAccountId = businessAccountId,
        //                RequestingUserId = requestingUserId,
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

        public async Task<(List<PurchaseRecord> Items, int TotalCount)> GetAllAsync(
            int businessAccountId, int requestingUserId,
            string? status, int? supplierId, string? search,
            DateTime? dateFrom, DateTime? dateTo,
            int page, int pageSize)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_Purchases_GetAll",
                    new
                    {
                        BusinessAccountId = businessAccountId,
                        RequestingUserId = requestingUserId,
                        Status = string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status,
                        SupplierId = supplierId.HasValue ? (object)supplierId.Value : DBNull.Value,
                        Search = string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : search,
                        DateFrom = dateFrom.HasValue ? dateFrom.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value,
                        DateTo = dateTo.HasValue ? dateTo.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value,
                        Page = page,
                        PageSize = pageSize,
                    },
                    commandType: CommandType.StoredProcedure);

                var totalCount = await multi.ReadSingleAsync<int>();
                var items = (await multi.ReadAsync<PurchaseRecord>()).AsList();
                return (items, totalCount);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in Purchases.GetAllAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }

        // ── Get By Id ─────────────────────────────────────────────

        public async Task<PurchaseRecord?> GetByIdAsync(int purchaseId, int businessAccountId)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                return await db.QuerySingleOrDefaultAsync<PurchaseRecord>(
                    "dbo.usp_Purchases_GetById",
                    new { PurchaseId = purchaseId, BusinessAccountId = businessAccountId },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetByIdAsync PurchaseId={Id}", purchaseId);
                throw;
            }
        }

        // ── Get Summary ───────────────────────────────────────────

        public async Task<PurchaseSummaryRecord> GetSummaryAsync(int businessAccountId)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                var result = await db.QuerySingleAsync<PurchaseSummaryRecord>(
                    "dbo.usp_Purchases_GetSummary",
                    new { BusinessAccountId = businessAccountId },
                    commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetSummaryAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }

        // ── Create ────────────────────────────────────────────────

        public async Task<PurchaseRecord?> CreateAsync(
            int businessAccountId, int requestingUserId,
            CreatePurchaseRequest req, string ip)
        {
            using var db = new SqlConnection(_conn);

            // Serialize items to JSON for the SP parameter
            var itemsJson = JsonSerializer.Serialize(req.Items.Select(i => new
            {
                productId = i.ProductId,
                productName = i.ProductName,
                quantity = i.Quantity,
                pricePerUnit = i.PricePerUnit,
                total = i.Total,
            }), _jsonOpts);

            var p = new DynamicParameters();
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@SupplierId", req.SupplierId);
            p.Add("@SupplierName", req.SupplierName);
            p.Add("@ItemsJson", itemsJson);
            p.Add("@GrandTotal", req.GrandTotal);
            p.Add("@PaidAmount", req.PaidAmount);
            p.Add("@Balance", req.Balance);
            p.Add("@PaymentStatus", req.PaymentStatus);
            p.Add("@Notes", req.Notes);
            p.Add("@IpAddress", ip);
            p.Add("@NewPurchaseId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<PurchaseRecord>(
                    "dbo.usp_Purchases_Create", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "PURCHASE");
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

        public async Task<PurchaseRecord?> UpdateAsync(
            int purchaseId, int businessAccountId, int requestingUserId,
            UpdatePurchaseRequest req, string ip)
        {
            using var db = new SqlConnection(_conn);

            var rowVersionBytes = DecodeRowVersion(req.RowVersion);
            var itemsJson = JsonSerializer.Serialize(req.Items.Select(i => new
            {
                productId = i.ProductId,
                productName = i.ProductName,
                quantity = i.Quantity,
                pricePerUnit = i.PricePerUnit,
                total = i.Total,
            }), _jsonOpts);

            var p = new DynamicParameters();
            p.Add("@PurchaseId", purchaseId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@SupplierId", req.SupplierId);
            p.Add("@SupplierName", req.SupplierName);
            p.Add("@ItemsJson", itemsJson);
            p.Add("@GrandTotal", req.GrandTotal);
            p.Add("@PaidAmount", req.PaidAmount);
            p.Add("@Balance", req.Balance);
            p.Add("@PaymentStatus", req.PaymentStatus);
            p.Add("@Notes", req.Notes);
            p.Add("@RowVersion", rowVersionBytes, DbType.Binary, size: 8);
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<PurchaseRecord>(
                    "dbo.usp_Purchases_Update", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "PURCHASE", code => code == 6009 ? 409 : 400);
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in UpdateAsync PurchaseId={Id}", purchaseId);
                throw;
            }
        }

        // ── Mark Paid ─────────────────────────────────────────────

        public async Task<PurchaseRecord?> MarkPaidAsync(
            int purchaseId, int businessAccountId, int requestingUserId, string ip)
        {
            using var db = new SqlConnection(_conn);

            var p = new DynamicParameters();
            p.Add("@PurchaseId", purchaseId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<PurchaseRecord>(
                    "dbo.usp_Purchases_MarkPaid", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "PURCHASE");
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in MarkPaidAsync PurchaseId={Id}", purchaseId);
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
            DynamicParameters p,
            string prefix,
            Func<int, int>? httpStatusResolver = null)
        {
            int code = p.Get<int>("@ErrorCode");
            string message = p.Get<string>("@ErrorMessage") ?? string.Empty;
            if (code == 0) return;
            int httpStatus = httpStatusResolver?.Invoke(code) ?? 400;
            throw new AppException(message, $"{prefix}_{code}", httpStatus);
        }
    }

}
