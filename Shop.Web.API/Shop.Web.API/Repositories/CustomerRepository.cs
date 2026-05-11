using Dapper;
using Microsoft.Data.SqlClient;
using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using System.Data;

namespace Shop.Web.API.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _conn;
        private readonly ILogger<CustomerRepository> _logger;

        public CustomerRepository(IConfiguration config, ILogger<CustomerRepository> logger)
        {
            _conn = config.GetConnectionString("Default")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
            _logger = logger;
        }

        //public async Task<List<CustomerRecord>> GetAllAsync(int businessAccountId, int requestingUserId)
        //{
        //    using var db = new SqlConnection(_conn);
        //    try
        //    {
        //        var rows = await db.QueryAsync<CustomerRecord>(
        //            "dbo.usp_Customers_GetAll",
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

        //public async Task<(List<CustomerRecord> Items, int TotalCount)> GetAllAsync(
        //    int businessAccountId, int requestingUserId,
        //    int page, int pageSize,
        //    string? search, string? status, string? type, bool? hasDue)
        //{
        //    using var db = new SqlConnection(_conn);
        //    try
        //    {
        //        using var multi = await db.QueryMultipleAsync(
        //            "dbo.usp_Customers_GetAll",
        //            new
        //            {
        //                BusinessAccountId = businessAccountId,
        //                RequestingUserId = requestingUserId,
        //                Page = page,
        //                PageSize = pageSize,
        //                Search = search,
        //                Status = status,
        //                Type = type,
        //                HasDue = hasDue,
        //            },
        //            commandType: CommandType.StoredProcedure);

        //        var totalCount = await multi.ReadSingleAsync<int>();
        //        var items = (await multi.ReadAsync<CustomerRecord>()).AsList();
        //        return (items, totalCount);
        //    }
        //    catch (SqlException ex)
        //    {
        //        _logger.LogError(ex, "SQL error in GetAllAsync BusinessAccountId={Id}", businessAccountId);
        //        throw;
        //    }
        //}

        public async Task<(CustomerSummaryDto Summary, int FilteredCount, List<CustomerRecord> Items)> GetAllAsync(
    int businessAccountId, int requestingUserId,
    int page, int pageSize,
    string? search, string? status, string? type, bool? hasDue)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_Customers_GetAll",
                    new
                    {
                        BusinessAccountId = businessAccountId,
                        RequestingUserId = requestingUserId,
                        Page = page,
                        PageSize = pageSize,
                        Search = search,
                        Status = status,
                        Type = type,
                        HasDue = hasDue,
                    },
                    commandType: CommandType.StoredProcedure);

                // Result set 1: KPI summary (always unfiltered)
                var summary = await multi.ReadSingleAsync<CustomerSummaryDto>();

                // Result set 2: filtered total count (for pagination)
                var filteredCount = await multi.ReadSingleAsync<int>();

                // Result set 3: paged rows
                var items = (await multi.ReadAsync<CustomerRecord>()).AsList();

                return (summary, filteredCount, items);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetAllAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }

        public async Task<CustomerRecord?> GetByIdAsync(int customerId, int businessAccountId)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                return await db.QuerySingleOrDefaultAsync<CustomerRecord>(
                    "dbo.usp_Customers_GetById",
                    new { CustomerId = customerId, BusinessAccountId = businessAccountId },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetByIdAsync CustomerId={Id}", customerId);
                throw;
            }
        }

        public async Task<CustomerRecord?> CreateAsync(
            int businessAccountId, int requestingUserId,
            CreateCustomerRequest req, string ip)
        {
            using var db = new SqlConnection(_conn);
            var p = new DynamicParameters();
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@Name", req.Name.Trim());
            p.Add("@Phone", req.Phone.Trim());
            p.Add("@Address", req.Address?.Trim());
            p.Add("@CustomerType", req.CustomerType);
            p.Add("@DefaultPricePerCan", req.DefaultPricePerCan);
            p.Add("@DefaultPriceProductId", req.DefaultPriceProductId);
            p.Add("@UsePriceFromProduct", req.UsePriceFromProduct);
            p.Add("@IpAddress", ip);
            p.Add("@NewCustomerId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<CustomerRecord>(
                    "dbo.usp_Customers_Create", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "CUSTOMER");
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in CreateAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }

        public async Task<CustomerRecord?> UpdateAsync(
            int customerId, int businessAccountId, int requestingUserId,
            UpdateCustomerRequest req, string ip)
        {
            using var db = new SqlConnection(_conn);
            var rowVersionBytes = DecodeRowVersion(req.RowVersion);

            var p = new DynamicParameters();
            p.Add("@CustomerId", customerId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@Name", req.Name.Trim());
            p.Add("@Phone", req.Phone.Trim());
            p.Add("@Address", req.Address?.Trim());
            p.Add("@CustomerType", req.CustomerType);
            p.Add("@DefaultPricePerCan", req.DefaultPricePerCan);
            p.Add("@DefaultPriceProductId", req.DefaultPriceProductId);
            p.Add("@UsePriceFromProduct", req.UsePriceFromProduct);
            p.Add("@RowVersion", rowVersionBytes, DbType.Binary, size: 8);
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<CustomerRecord>(
                    "dbo.usp_Customers_Update", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "CUSTOMER", code => code == 10009 ? 409 : 400);
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in UpdateAsync CustomerId={Id}", customerId);
                throw;
            }
        }

        public async Task<CustomerRecord?> ToggleStatusAsync(
            int customerId, int businessAccountId, int requestingUserId, string ip)
        {
            using var db = new SqlConnection(_conn);
            var p = new DynamicParameters();
            p.Add("@CustomerId", customerId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<CustomerRecord>(
                    "dbo.usp_Customers_ToggleStatus", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "CUSTOMER");
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in ToggleStatusAsync CustomerId={Id}", customerId);
                throw;
            }
        }

        // CustomerRepo.cs
        public async Task<CustomerSummaryDto> GetSummaryAsync(int businessAccountId, int requestingUserId)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_Customers_GetAll",
                    new
                    {
                        BusinessAccountId = businessAccountId,
                        RequestingUserId = requestingUserId,
                        SummaryOnly = true,
                    },
                    commandType: CommandType.StoredProcedure);

                var summary = await multi.ReadSingleAsync<CustomerSummaryDto>();
                var topDueCustomers = (await multi.ReadAsync<CustomerDueItemDto>()).AsList();

                return new CustomerSummaryDto
                {
                    TotalCount = summary.TotalCount,
                    ActiveCount = summary.ActiveCount,
                    InactiveCount = summary.InactiveCount,
                    HotelCount = summary.HotelCount,
                    HomeCount = summary.HomeCount,
                    CustomersWithDue = summary.CustomersWithDue,
                    TotalDueAmount = summary.TotalDueAmount,
                    TopDueCustomers = topDueCustomers,
                };
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetSummaryAsync BusinessAccountId={Id}", businessAccountId);
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
