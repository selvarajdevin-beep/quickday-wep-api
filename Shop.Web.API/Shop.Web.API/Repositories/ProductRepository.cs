using Dapper;
using Microsoft.Data.SqlClient;
using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using System.Data;

namespace Shop.Web.API.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _conn;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(IConfiguration config, ILogger<ProductRepository> logger)
        {
            _conn = config.GetConnectionString("Default")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
            _logger = logger;
        }

        // ── Get All ───────────────────────────────────────────────

        //public async Task<List<ProductRecord>> GetAllAsync(
        //    int businessAccountId, int requestingUserId,
        //    bool? activeOnly = null, string? category = null)
        //{
        //    using var db = new SqlConnection(_conn);
        //    try
        //    {
        //        var rows = await db.QueryAsync<ProductRecord>(
        //            "dbo.usp_Products_GetAll",
        //            new
        //            {
        //                BusinessAccountId = businessAccountId,
        //                RequestingUserId = requestingUserId,
        //                ActiveOnly = activeOnly.HasValue ? (object)(activeOnly.Value ? 1 : 0) : DBNull.Value,
        //                Category = string.IsNullOrWhiteSpace(category) ? (object)DBNull.Value : category,
        //            },
        //            commandType: CommandType.StoredProcedure);
        //        return rows.AsList();
        //    }
        //    catch (SqlException ex)
        //    {
        //        _logger.LogError(ex, "SQL error in Products.GetAllAsync BusinessAccountId={Id}", businessAccountId);
        //        throw;
        //    }
        //}

        public async Task<(List<ProductRecord> Items, int TotalCount)> GetAllAsync(
            int businessAccountId, int requestingUserId,
            bool? activeOnly, string? category, string? search, bool? lowStockOnly,
            int page, int pageSize)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_Products_GetAll",
                    new
                    {
                        BusinessAccountId = businessAccountId,
                        RequestingUserId = requestingUserId,
                        ActiveOnly = activeOnly.HasValue ? (object)(activeOnly.Value ? 1 : 0) : DBNull.Value,
                        Category = string.IsNullOrWhiteSpace(category) ? (object)DBNull.Value : category,
                        Search = string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : search,
                        LowStockOnly = lowStockOnly.HasValue ? (object)(lowStockOnly.Value ? 1 : 0) : DBNull.Value,
                        Page = page,
                        PageSize = pageSize,
                    },
                    commandType: CommandType.StoredProcedure);

                var totalCount = await multi.ReadSingleAsync<int>();
                var items = (await multi.ReadAsync<ProductRecord>()).AsList();
                return (items, totalCount);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in Products.GetAllAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }

        // ── Get By Id ─────────────────────────────────────────────

        public async Task<ProductRecord?> GetByIdAsync(int productId, int businessAccountId)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                return await db.QuerySingleOrDefaultAsync<ProductRecord>(
                    "dbo.usp_Products_GetById",
                    new { ProductId = productId, BusinessAccountId = businessAccountId },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in Products.GetByIdAsync ProductId={Id}", productId);
                throw;
            }
        }

        // ── Get Summary ───────────────────────────────────────────

        public async Task<ProductSummaryRecord> GetSummaryAsync(int businessAccountId)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                return await db.QuerySingleAsync<ProductSummaryRecord>(
                    "dbo.usp_Products_GetSummary",
                    new { BusinessAccountId = businessAccountId },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in Products.GetSummaryAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }

        // ── Create ────────────────────────────────────────────────

        public async Task<ProductRecord?> CreateAsync(
            int businessAccountId, int requestingUserId,
            CreateProductRequest req, string ip)
        {
            using var db = new SqlConnection(_conn);

            var p = new DynamicParameters();
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@Name", req.Name);
            p.Add("@UnitType", req.UnitType);
            p.Add("@Capacity", string.IsNullOrWhiteSpace(req.Capacity) ? (object)DBNull.Value : req.Capacity);
            p.Add("@Category", string.IsNullOrWhiteSpace(req.Category) ? (object)DBNull.Value : req.Category);
            p.Add("@SellingPrice", req.SellingPrice);
            p.Add("@PurchasePrice", req.PurchasePrice);
            p.Add("@MinStockAlert", req.MinStockAlert);
            p.Add("@Active", req.Active);
            p.Add("@IpAddress", ip);
            p.Add("@NewProductId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<ProductRecord>(
                    "dbo.usp_Products_Create", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "PRODUCT");
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in Products.CreateAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }

        // ── Update ────────────────────────────────────────────────

        public async Task<ProductRecord?> UpdateAsync(
            int productId, int businessAccountId, int requestingUserId,
            UpdateProductRequest req, string ip)
        {
            using var db = new SqlConnection(_conn);

            var rowVersionBytes = DecodeRowVersion(req.RowVersion);

            var p = new DynamicParameters();
            p.Add("@ProductId", productId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@Name", req.Name);
            p.Add("@UnitType", req.UnitType);
            p.Add("@Capacity", string.IsNullOrWhiteSpace(req.Capacity) ? (object)DBNull.Value : req.Capacity);
            p.Add("@Category", string.IsNullOrWhiteSpace(req.Category) ? (object)DBNull.Value : req.Category);
            p.Add("@SellingPrice", req.SellingPrice);
            p.Add("@PurchasePrice", req.PurchasePrice);
            p.Add("@MinStockAlert", req.MinStockAlert);
            p.Add("@Active", req.Active);
            p.Add("@RowVersion", rowVersionBytes, DbType.Binary, size: 8);
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<ProductRecord>(
                    "dbo.usp_Products_Update", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "PRODUCT", code => code == 7009 ? 409 : 400);
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in Products.UpdateAsync ProductId={Id}", productId);
                throw;
            }
        }

        // ── Toggle Status ─────────────────────────────────────────

        public async Task<ProductRecord?> ToggleStatusAsync(
            int productId, int businessAccountId, int requestingUserId, string ip)
        {
            using var db = new SqlConnection(_conn);

            var p = new DynamicParameters();
            p.Add("@ProductId", productId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<ProductRecord>(
                    "dbo.usp_Products_ToggleStatus", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "PRODUCT");
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in Products.ToggleStatusAsync ProductId={Id}", productId);
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
