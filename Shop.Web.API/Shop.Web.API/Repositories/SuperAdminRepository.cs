using Dapper;
using Microsoft.Data.SqlClient;
using Shop.Web.API.Models.Domain;
using System.Data;

namespace Shop.Web.API.Repositories
{
    public sealed class SuperAdminRepository : ISuperAdminRepository
    {
        private readonly string _conn;
        private readonly ILogger<SuperAdminRepository> _logger;

        public SuperAdminRepository(IConfiguration config, ILogger<SuperAdminRepository> logger)
        {
            _conn = config.GetConnectionString("Default")
                      ?? throw new InvalidOperationException("Connection string missing.");
            _logger = logger;
        }

        public async Task<SuperAdminDashboardRecord?> GetDashboardAsync()
        {
            using var db = new SqlConnection(_conn);
            try
            {
                return await db.QuerySingleOrDefaultAsync<SuperAdminDashboardRecord>(
                    "dbo.usp_SuperAdmin_GetDashboard",
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetDashboardAsync");
                throw;
            }
        }

        public async Task<(int TotalCount, IEnumerable<ShopListRecord> Items)> GetShopsAsync(
            int page, int pageSize, string? search, string? planFilter, string? statusFilter)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                var p = new DynamicParameters();
                p.Add("@Page", page);
                p.Add("@PageSize", pageSize);
                p.Add("@Search", search);
                p.Add("@PlanFilter", planFilter);
                p.Add("@StatusFilter", statusFilter);

                // SP returns 2 result sets: COUNT then rows
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_SuperAdmin_GetShops", p,
                    commandType: CommandType.StoredProcedure);

                var totalCount = await multi.ReadSingleAsync<int>();
                var items = await multi.ReadAsync<ShopListRecord>();
                return (totalCount, items);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetShopsAsync");
                throw;
            }
        }

        public async Task<ShopDetailRecord?> GetShopByIdAsync(int businessAccountId)
        {
            using var db = new SqlConnection(_conn);
            return await db.QuerySingleOrDefaultAsync<ShopDetailRecord>(
                "dbo.usp_SuperAdmin_GetShopById",
                new { BusinessAccountId = businessAccountId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<ShopDetailRecord?> UpdateSubscriptionAsync(
            int businessAccountId, string plan,
            DateOnly startDate, DateOnly expiry,
            int updatedByUserId)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_SuperAdmin_UpdateSubscription",
                    new
                    {
                        BusinessAccountId = businessAccountId,
                        SubscriptionPlan = plan,
                        SubscriptionStartDate = startDate.ToDateTime(TimeOnly.MinValue),
                        SubscriptionExpiry = expiry.ToDateTime(TimeOnly.MinValue),
                        UpdatedByUserId = updatedByUserId,
                    },
                    commandType: CommandType.StoredProcedure);

                return await multi.ReadSingleOrDefaultAsync<ShopDetailRecord>();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in UpdateSubscriptionAsync for BA={Id}",
                    businessAccountId);
                throw;
            }
        }

        public async Task<ShopDetailRecord?> ToggleShopStatusAsync(
            int businessAccountId, int updatedByUserId)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_SuperAdmin_ToggleShopStatus",
                    new { BusinessAccountId = businessAccountId, UpdatedByUserId = updatedByUserId },
                    commandType: CommandType.StoredProcedure);

                return await multi.ReadSingleOrDefaultAsync<ShopDetailRecord>();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in ToggleShopStatusAsync for BA={Id}",
                    businessAccountId);
                throw;
            }
        }

        // Repositories/SuperAdminRepository.cs  — add to existing class

        public async Task<(int TotalCount, IEnumerable<SubscriptionPaymentRecord> Items)> GetPaymentsAsync(
            int page, int pageSize,
            int? businessAccountId, string? plan, string? status)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                var p = new DynamicParameters();
                p.Add("@Page", page);
                p.Add("@PageSize", pageSize);
                p.Add("@BusinessAccountId", businessAccountId);
                p.Add("@Plan", plan);
                p.Add("@Status", status);

                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_SuperAdmin_GetPayments", p,
                    commandType: CommandType.StoredProcedure);

                var totalCount = await multi.ReadSingleAsync<int>();
                var items = await multi.ReadAsync<SubscriptionPaymentRecord>();
                return (totalCount, items);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetPaymentsAsync");
                throw;
            }
        }

        // SuperAdminRepository.cs — replace CreatePaymentAsync
        public async Task<SubscriptionPaymentRecord> CreatePaymentAsync(
            int businessAccountId, decimal amount, string currency,
            string plan, string paymentStatus,
            string? paymentMethod, string? transactionReference, string? notes,
            int durationMonths, int createdByUserId)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                return await db.QuerySingleAsync<SubscriptionPaymentRecord>(
                    "dbo.usp_SuperAdmin_CreatePayment",
                    new
                    {
                        BusinessAccountId = businessAccountId,
                        Amount = amount,
                        Currency = currency,
                        SubscriptionPlan = plan,
                        PaymentStatus = paymentStatus,
                        PaymentMethod = paymentMethod,
                        TransactionReference = transactionReference,
                        Notes = notes,
                        DurationMonths = durationMonths,
                        CreatedByUserId = createdByUserId,
                    },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in CreatePaymentAsync for BA={Id}", businessAccountId);
                throw;
            }
        }

        public async Task<SubscriptionPaymentRecord> UpdatePaymentAsync(
            int paymentId, string plan, string paymentStatus, int updatedByUserId)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                return await db.QuerySingleAsync<SubscriptionPaymentRecord>(
                    "dbo.usp_SuperAdmin_UpdatePayment",
                    new
                    {
                        PaymentId = paymentId,
                        SubscriptionPlan = plan,
                        PaymentStatus = paymentStatus,
                        UpdatedByUserId = updatedByUserId,
                    },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in UpdatePaymentAsync for PaymentId={Id}", paymentId);
                throw;
            }
        }

        public async Task<(
            RevenueStatsRecord Kpis,
            IEnumerable<MonthlyRevenueRecord> Monthly,
            IEnumerable<PlanRevenueRecord> ByPlan,
            IEnumerable<RecentPaymentRecord> Recent
        )> GetRevenueStatsAsync()
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_SuperAdmin_GetRevenueStats",
                    commandType: CommandType.StoredProcedure);

                var kpis = await multi.ReadSingleAsync<RevenueStatsRecord>();
                var monthly = await multi.ReadAsync<MonthlyRevenueRecord>();
                var byPlan = await multi.ReadAsync<PlanRevenueRecord>();
                var recent = await multi.ReadAsync<RecentPaymentRecord>();

                return (kpis, monthly, byPlan, recent);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetRevenueStatsAsync");
                throw;
            }
        }

    }
}
