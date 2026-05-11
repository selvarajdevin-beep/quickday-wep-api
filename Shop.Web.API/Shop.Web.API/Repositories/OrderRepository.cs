using Dapper;
using Microsoft.Data.SqlClient;
using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using System.Data;
using System.Text.Json;

namespace Shop.Web.API.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _conn;
        private readonly ILogger<OrderRepository> _logger;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };

        public OrderRepository(IConfiguration config, ILogger<OrderRepository> logger)
        {
            _conn = config.GetConnectionString("Default")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
            _logger = logger;
        }

        //public async Task<List<OrderRecord>> GetAllAsync(
        //    int businessAccountId, DateTime? from, DateTime? to)
        //{
        //    using var db = new SqlConnection(_conn);
        //    try
        //    {
        //        var rows = await db.QueryAsync<OrderRecord>(
        //            "dbo.usp_Orders_GetAll",
        //            new { BusinessAccountId = businessAccountId, DateFrom = from, DateTo = to },
        //            commandType: CommandType.StoredProcedure);
        //        return rows.AsList();
        //    }
        //    catch (SqlException ex)
        //    {
        //        _logger.LogError(ex, "SQL error in GetAllAsync BusinessAccountId={Id}", businessAccountId);
        //        throw;
        //    }
        //}

        public async Task<(List<OrderRecord> Items, int TotalCount)> GetAllAsync(
    int businessAccountId, DateTime? from, DateTime? to, int page, int pageSize, string? status = null, string? search = null)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_Orders_GetAll",
                    new
                    {
                        BusinessAccountId = businessAccountId,
                        DateFrom = from,
                        DateTo = to,
                        Status = status,
                        Search = search,
                        Page = page,
                        PageSize = pageSize,
                    },
                    commandType: CommandType.StoredProcedure);

                var totalCount = await multi.ReadSingleAsync<int>();
                var items = (await multi.ReadAsync<OrderRecord>()).AsList();
                return (items, totalCount);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetAllAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }

        public async Task<OrderRecord?> GetByIdAsync(int orderId, int businessAccountId)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                return await db.QuerySingleOrDefaultAsync<OrderRecord>(
                    "dbo.usp_Orders_GetById",
                    new { OrderId = orderId, BusinessAccountId = businessAccountId },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetByIdAsync OrderId={Id}", orderId);
                throw;
            }
        }

        //public async Task<List<OrderRecord>> GetByCustomerAsync(int customerId, int businessAccountId)
        //{
        //    using var db = new SqlConnection(_conn);
        //    try
        //    {
        //        var rows = await db.QueryAsync<OrderRecord>(
        //            "dbo.usp_Orders_GetByCustomer",
        //            new { CustomerId = customerId, BusinessAccountId = businessAccountId },
        //            commandType: CommandType.StoredProcedure);
        //        return rows.AsList();
        //    }
        //    catch (SqlException ex)
        //    {
        //        _logger.LogError(ex, "SQL error in GetByCustomerAsync CustomerId={Id}", customerId);
        //        throw;
        //    }
        //}

        public async Task<(List<OrderRecord> Items, int TotalCount)> GetByCustomerAsync(
            int customerId, int businessAccountId, int page, int pageSize)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_Orders_GetByCustomer",
                    new
                    {
                        CustomerId = customerId,
                        BusinessAccountId = businessAccountId,
                        Page = page,
                        PageSize = pageSize,
                    },
                    commandType: CommandType.StoredProcedure);

                var totalCount = await multi.ReadSingleAsync<int>();
                var items = (await multi.ReadAsync<OrderRecord>()).AsList();
                return (items, totalCount);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetByCustomerAsync CustomerId={Id}", customerId);
                throw;
            }
        }

        public async Task<TodaySummaryRecord> GetTodaySummaryAsync(int businessAccountId)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                return await db.QuerySingleAsync<TodaySummaryRecord>(
                    "dbo.usp_Orders_GetTodaySummary",
                    new { BusinessAccountId = businessAccountId },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetTodaySummaryAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }

        public async Task<OrderRecord?> CreateAsync(
            int businessAccountId, int requestingUserId,
            CreateOrderRequest req, string ip)
        {
            using var db = new SqlConnection(_conn);

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
            p.Add("@CustomerId", req.CustomerId);
            p.Add("@CustomerName", req.CustomerName);
            p.Add("@ItemsJson", itemsJson);
            p.Add("@GrandTotal", req.GrandTotal);
            p.Add("@PaidAmount", req.PaidAmount);
            p.Add("@Balance", req.Balance);
            p.Add("@PaymentType", req.PaymentType);
            p.Add("@Status", req.Status);
            p.Add("@DeliveryNote", req.DeliveryNote);

            p.Add("@SubTotal", req.SubTotal);
            p.Add("@TaxableAmount", req.TaxableAmount);
            p.Add("@GstType", req.GstType);
            p.Add("@CgstRate", req.CgstRate);
            p.Add("@SgstRate", req.SgstRate);
            p.Add("@IgstRate", req.IgstRate);
            p.Add("@CgstAmount", req.CgstAmount);
            p.Add("@SgstAmount", req.SgstAmount);
            p.Add("@IgstAmount", req.IgstAmount);
            p.Add("@TotalGst", req.TotalGst);

            p.Add("@IpAddress", ip);
            p.Add("@NewOrderId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<OrderRecord>(
                    "dbo.usp_Orders_Create", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "ORDER");
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in CreateAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }

        public async Task<OrderRecord?> UpdateAsync(
            int orderId, int businessAccountId, int requestingUserId,
            UpdateOrderRequest req, string ip)
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
            p.Add("@OrderId", orderId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@CustomerId", req.CustomerId);
            p.Add("@CustomerName", req.CustomerName);
            p.Add("@ItemsJson", itemsJson);
            p.Add("@GrandTotal", req.GrandTotal);
            p.Add("@PaidAmount", req.PaidAmount);
            p.Add("@Balance", req.Balance);
            p.Add("@PaymentType", req.PaymentType);
            p.Add("@Status", req.Status);
            p.Add("@DeliveryNote", req.DeliveryNote);
            p.Add("@RowVersion", rowVersionBytes, DbType.Binary, size: 8);

            p.Add("@SubTotal", req.SubTotal);
            p.Add("@TaxableAmount", req.TaxableAmount);
            p.Add("@GstType", req.GstType);
            p.Add("@CgstRate", req.CgstRate);
            p.Add("@SgstRate", req.SgstRate);
            p.Add("@IgstRate", req.IgstRate);
            p.Add("@CgstAmount", req.CgstAmount);
            p.Add("@SgstAmount", req.SgstAmount);
            p.Add("@IgstAmount", req.IgstAmount);
            p.Add("@TotalGst", req.TotalGst);

            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<OrderRecord>(
                    "dbo.usp_Orders_Update", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "ORDER", code => code == 11009 ? 409 : 400);
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in UpdateAsync OrderId={Id}", orderId);
                throw;
            }
        }

        //public async Task<List<PaymentRecord>> GetPaymentsByCustomerAsync(
        //    int customerId, int businessAccountId)
        //{
        //    using var db = new SqlConnection(_conn);
        //    try
        //    {
        //        var rows = await db.QueryAsync<PaymentRecord>(
        //            "dbo.usp_Payments_GetByCustomer",
        //            new { CustomerId = customerId, BusinessAccountId = businessAccountId },
        //            commandType: CommandType.StoredProcedure);
        //        return rows.AsList();
        //    }
        //    catch (SqlException ex)
        //    {
        //        _logger.LogError(ex, "SQL error in GetPaymentsByCustomerAsync CustomerId={Id}", customerId);
        //        throw;
        //    }
        //}

        public async Task<(List<PaymentRecord> Items, int TotalCount)> GetPaymentsByCustomerAsync(
            int customerId, int businessAccountId, int page, int pageSize)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_Payments_GetByCustomer",
                    new
                    {
                        CustomerId = customerId,
                        BusinessAccountId = businessAccountId,
                        Page = page,
                        PageSize = pageSize,
                    },
                    commandType: CommandType.StoredProcedure);

                var totalCount = await multi.ReadSingleAsync<int>();
                var items = (await multi.ReadAsync<PaymentRecord>()).AsList();
                return (items, totalCount);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetPaymentsByCustomerAsync CustomerId={Id}", customerId);
                throw;
            }
        }

        public async Task<int> RecordPaymentAsync(
            int businessAccountId, int requestingUserId,
            int customerId, RecordPaymentRequest req, string ip)
        {
            using var db = new SqlConnection(_conn);
            var p = new DynamicParameters();
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@CustomerId", customerId);
            p.Add("@Amount", req.Amount);
            p.Add("@PaymentType", req.PaymentType);
            p.Add("@Note", req.Note);
            p.Add("@OrderId", req.OrderId);
            p.Add("@IpAddress", ip);
            p.Add("@NewPaymentId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                await db.ExecuteAsync(
                    "dbo.usp_Payments_Record", p, commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "PAYMENT");
                return p.Get<int>("@NewPaymentId");
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in RecordPaymentAsync CustomerId={Id}", customerId);
                throw;
            }
        }

        public async Task<int> SoftDeleteAsync(
            int orderId, int businessAccountId, int requestingUserId, string ip)
        {
            using var db = new SqlConnection(_conn);
            return await db.ExecuteAsync(
                "dbo.usp_Orders_SoftDelete",
                new
                {
                    OrderId = orderId,
                    BusinessAccountId = businessAccountId,
                    RequestingUserId = requestingUserId,
                    IpAddress = ip
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<List<PaymentRecord>> GetPaymentsByOrderAsync(
            int orderId, int businessAccountId)
        {
            using var db = new SqlConnection(_conn);
            var rows = await db.QueryAsync<PaymentRecord>(
                "dbo.usp_Payments_GetByOrder",
                new { OrderId = orderId, BusinessAccountId = businessAccountId },
                commandType: CommandType.StoredProcedure);
            return rows.AsList();
        }

        public async Task<OrderDashboardSummaryDto> GetDashboardSummaryAsync(
            int businessAccountId, DateTime? from)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_Orders_GetDashboardSummary",
                    new
                    {
                        BusinessAccountId = businessAccountId,
                        DateFrom = from.HasValue
                                            ? from.Value.ToString("yyyy-MM-dd")  // ← always ISO format
                                            : (string?)null,
                    },
                    commandType: CommandType.StoredProcedure);

                var dailyTotals = (await multi.ReadAsync<OrderDailySummaryDto>()).AsList();
                var recentOrders = (await multi.ReadAsync<OrderRecentItemDto>()).AsList();

                return new OrderDashboardSummaryDto
                {
                    DailyTotals = dailyTotals,
                    RecentOrders = recentOrders,
                };
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetDashboardSummaryAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }

        public async Task<(List<OrderRecord> Items, int TotalCount, decimal TotalSales, decimal TotalPaid, decimal TotalDue)>
    GetByCustomerFilteredAsync(
        int customerId,
        int businessAccountId,
        int page,
        int pageSize,
        string? dateFrom = null,
        string? dateTo = null,
        string? search = null,
        string? status = null)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_Orders_GetByCustomerFiltered",
                    new
                    {
                        CustomerId = customerId,
                        BusinessAccountId = businessAccountId,
                        Page = page,
                        PageSize = pageSize,
                        DateFrom = string.IsNullOrWhiteSpace(dateFrom) ? (DateTime?)null : DateTime.Parse(dateFrom),
                        DateTo = string.IsNullOrWhiteSpace(dateTo) ? (DateTime?)null : DateTime.Parse(dateTo),
                        Search = string.IsNullOrWhiteSpace(search) ? null : search,
                        Status = string.IsNullOrWhiteSpace(status) ? null : status,
                    },
                    commandType: CommandType.StoredProcedure);

                // First result set: summary row
                var summary = await multi.ReadSingleAsync<OrderFilteredSummary>();

                // Second result set: paged rows
                var items = (await multi.ReadAsync<OrderRecord>()).AsList();

                return (items, summary.TotalCount, summary.TotalSales, summary.TotalPaid, summary.TotalDue);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetByCustomerFilteredAsync CustomerId={Id}", customerId);
                throw;
            }
        }

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
