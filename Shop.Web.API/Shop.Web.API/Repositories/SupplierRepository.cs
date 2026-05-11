using Dapper;
using Microsoft.Data.SqlClient;
using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using System.Data;

namespace Shop.Web.API.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly string _conn;
        private readonly ILogger<SupplierRepository> _logger;

        public SupplierRepository(IConfiguration config, ILogger<SupplierRepository> logger)
        {
            _conn = config.GetConnectionString("Default")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
            _logger = logger;
        }

        // ── Get All ───────────────────────────────────────────────

        //public async Task<List<SupplierRecord>> GetAllAsync(int businessAccountId, int requestingUserId)
        //{
        //    using var db = new SqlConnection(_conn);
        //    try
        //    {
        //        var rows = await db.QueryAsync<SupplierRecord>(
        //            "dbo.usp_Suppliers_GetAll",
        //            new { BusinessAccountId = businessAccountId, RequestingUserId = requestingUserId },
        //            commandType: CommandType.StoredProcedure);
        //        return rows.AsList();
        //    }
        //    catch (SqlException ex)
        //    {
        //        _logger.LogError(ex, "SQL error in GetAllAsync BusinessAccountId={Id}", businessAccountId);
        //        throw;
        //    }
        //}

        public async Task<(List<SupplierRecord> Items, int TotalCount)> GetAllAsync(
            int businessAccountId, int requestingUserId,
            string? search, string? status,
            int page, int pageSize)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_Suppliers_GetAll",
                    new
                    {
                        BusinessAccountId = businessAccountId,
                        RequestingUserId = requestingUserId,
                        Search = string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : search,
                        Status = string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status,
                        Page = page,
                        PageSize = pageSize,
                    },
                    commandType: CommandType.StoredProcedure);

                var totalCount = await multi.ReadSingleAsync<int>();
                var items = (await multi.ReadAsync<SupplierRecord>()).AsList();
                return (items, totalCount);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in Suppliers.GetAllAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }


        // ── Get By Id ─────────────────────────────────────────────

        public async Task<SupplierRecord?> GetByIdAsync(int supplierId, int businessAccountId)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                return await db.QuerySingleOrDefaultAsync<SupplierRecord>(
                    "dbo.usp_Suppliers_GetById",
                    new { SupplierId = supplierId, BusinessAccountId = businessAccountId },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetByIdAsync SupplierId={Id}", supplierId);
                throw;
            }
        }

        // ── Create ────────────────────────────────────────────────

        public async Task<SupplierRecord?> CreateAsync(
            int businessAccountId, int requestingUserId,
            CreateSupplierRequest req, string ip)
        {
            using var db = new SqlConnection(_conn);

            var p = new DynamicParameters();
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@Name", req.Name.Trim());
            p.Add("@Phone", req.Phone.Trim());
            p.Add("@Email", req.Email?.Trim());
            p.Add("@Address", req.Address?.Trim());
            p.Add("@GSTIN", req.GSTIN?.Trim());
            p.Add("@ContactPerson", req.ContactPerson?.Trim());
            p.Add("@Notes", req.Notes?.Trim());
            p.Add("@IpAddress", ip);
            p.Add("@NewSupplierId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<SupplierRecord>(
                    "dbo.usp_Suppliers_Create", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "SUPPLIER");
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

        public async Task<SupplierRecord?> UpdateAsync(
            int supplierId, int businessAccountId, int requestingUserId,
            UpdateSupplierRequest req, string ip)
        {
            using var db = new SqlConnection(_conn);
            var rowVersionBytes = DecodeRowVersion(req.RowVersion);

            var p = new DynamicParameters();
            p.Add("@SupplierId", supplierId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@Name", req.Name.Trim());
            p.Add("@Phone", req.Phone.Trim());
            p.Add("@Email", req.Email?.Trim());
            p.Add("@Address", req.Address?.Trim());
            p.Add("@GSTIN", req.GSTIN?.Trim());
            p.Add("@ContactPerson", req.ContactPerson?.Trim());
            p.Add("@Notes", req.Notes?.Trim());
            p.Add("@Active", req.Active);
            p.Add("@RowVersion", rowVersionBytes, DbType.Binary, size: 8);
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<SupplierRecord>(
                    "dbo.usp_Suppliers_Update", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "SUPPLIER", code => code == 5009 ? 409 : 400);
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in UpdateAsync SupplierId={Id}", supplierId);
                throw;
            }
        }

        // ── Toggle Status ─────────────────────────────────────────

        public async Task<SupplierRecord?> ToggleStatusAsync(
            int supplierId, int businessAccountId, int requestingUserId, string ip)
        {
            using var db = new SqlConnection(_conn);

            var p = new DynamicParameters();
            p.Add("@SupplierId", supplierId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<SupplierRecord>(
                    "dbo.usp_Suppliers_ToggleStatus", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "SUPPLIER");
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in ToggleStatusAsync SupplierId={Id}", supplierId);
                throw;
            }
        }

        // ── Get Purchases ─────────────────────────────────────────

        public async Task<List<PurchaseRecord>> GetPurchasesAsync(
            int supplierId, int businessAccountId, int maxRows = 10)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                var rows = await db.QueryAsync<PurchaseRecord>(
                    "dbo.usp_Suppliers_GetPurchases",
                    new { SupplierId = supplierId, BusinessAccountId = businessAccountId, MaxRows = maxRows },
                    commandType: CommandType.StoredProcedure);
                return rows.AsList();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetPurchasesAsync SupplierId={Id}", supplierId);
                throw;
            }
        }

        // ── Record Payment ────────────────────────────────────────

        public async Task<SupplierRecord?> RecordPaymentAsync(
            int supplierId, int businessAccountId, int requestingUserId,
            decimal amount, string ip)
        {
            using var db = new SqlConnection(_conn);

            var p = new DynamicParameters();
            p.Add("@SupplierId", supplierId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@Amount", amount);
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<SupplierRecord>(
                    "dbo.usp_Suppliers_RecordPayment", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "SUPPLIER");
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in RecordPaymentAsync SupplierId={Id}", supplierId);
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
