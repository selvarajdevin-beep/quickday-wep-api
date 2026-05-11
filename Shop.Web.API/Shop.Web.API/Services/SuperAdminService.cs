using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Repositories;
using System.ComponentModel.DataAnnotations;

namespace Shop.Web.API.Services
{
    public sealed class SuperAdminService : ISuperAdminService
    {
        private readonly ISuperAdminRepository _repo;
        private readonly ILogger<SuperAdminService> _logger;

        public SuperAdminService(ISuperAdminRepository repo, ILogger<SuperAdminService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<SuperAdminDashboardDto> GetDashboardAsync()
        {
            var r = await _repo.GetDashboardAsync()
                    ?? throw new InvalidOperationException("Dashboard query returned no data.");
            return new SuperAdminDashboardDto
            {
                TotalShops = r.TotalShops,
                ActiveShops = r.ActiveShops,
                ExpiringIn30Days = r.ExpiringIn30Days,
                ExpiredShops = r.ExpiredShops,
                FreePlanCount = r.FreePlanCount,
                BasicPlanCount = r.BasicPlanCount,
                ProPlanCount = r.ProPlanCount,
            };
        }

        public async Task<PagedShopsDto> GetShopsAsync(
            int page, int pageSize, string? search, string? planFilter, string? statusFilter)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (totalCount, items) = await _repo.GetShopsAsync(
                page, pageSize, search, planFilter, statusFilter);

            return new PagedShopsDto
            {
                Items = items.Select(MapListItem).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        public async Task<ShopDetailDto> GetShopByIdAsync(int businessAccountId)
        {
            var r = await _repo.GetShopByIdAsync(businessAccountId)
                    ?? throw new NotFoundException($"Business account {businessAccountId} not found.");
            return MapDetail(r);
        }

        public async Task<ShopDetailDto> UpdateSubscriptionAsync(
            int businessAccountId, UpdateSubscriptionRequest req, int updatedByUserId)
        {
            if (!DateOnly.TryParse(req.SubscriptionStartDate, out var start))
                throw new ValidationException("Invalid start date format. Use YYYY-MM-DD.");
            if (!DateOnly.TryParse(req.SubscriptionExpiry, out var expiry))
                throw new ValidationException("Invalid expiry date format. Use YYYY-MM-DD.");

            var r = await _repo.UpdateSubscriptionAsync(
                businessAccountId, req.SubscriptionPlan, start, expiry, updatedByUserId)
                    ?? throw new NotFoundException($"Business account {businessAccountId} not found.");
            return MapDetail(r);
        }

        public async Task<ShopDetailDto> ToggleShopStatusAsync(
            int businessAccountId, int updatedByUserId)
        {
            var r = await _repo.ToggleShopStatusAsync(businessAccountId, updatedByUserId)
                    ?? throw new NotFoundException($"Business account {businessAccountId} not found.");
            return MapDetail(r);
        }

        // Services/SuperAdminService.cs  — add to existing class

        public async Task<PagedPaymentsDto> GetPaymentsAsync(
            int page, int pageSize,
            int? businessAccountId, string? plan, string? status)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (totalCount, items) = await _repo.GetPaymentsAsync(
                page, pageSize, businessAccountId, plan, status);

            return new PagedPaymentsDto
            {
                Items = items.Select(MapPayment).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        // SuperAdminService.cs — replace CreatePaymentAsync + mapper

        public async Task<PaymentHistoryItemDto> CreatePaymentAsync(
            CreatePaymentRequest req, int createdByUserId)
        {
            if (req.DurationMonths < 1)
                throw new ValidationException("DurationMonths must be at least 1.");

            //var allowed = new[] { "Paid", "Pending", "Failed" };
            //if (!allowed.Contains(req.PaymentStatus))
            //    throw new ValidationException($"PaymentStatus must be one of: {string.Join(", ", allowed)}.");

            var r = await _repo.CreatePaymentAsync(
                req.BusinessAccountId, req.Amount, req.Currency,
                req.Plan, req.PaymentStatus,
                req.PaymentMethod, req.TransactionRef, req.Notes,
                req.DurationMonths, createdByUserId);

            return MapPayment(r);
        }

        public async Task<PaymentHistoryItemDto> UpdatePaymentAsync(
            int paymentId, UpdatePaymentRequest req, int updatedByUserId)
        {
            var allowed = new[] { "Paid", "Pending", "Failed" };
            if (!allowed.Contains(req.PaymentStatus))
                throw new ValidationException(
                    $"PaymentStatus must be one of: {string.Join(", ", allowed)}.");

            var r = await _repo.UpdatePaymentAsync(
                paymentId, req.Plan, req.PaymentStatus, updatedByUserId);

            return MapPayment(r);
        }

        public async Task<RevenueStatsDto> GetRevenueStatsAsync()
        {
            var (kpis, monthly, byPlan, recent) = await _repo.GetRevenueStatsAsync();

            // Month-over-month change %
            decimal momPct = kpis.PreviousMonthRevenue == 0
                ? (kpis.CurrentMonthRevenue > 0 ? 100m : 0m)
                : Math.Round(
                    (kpis.CurrentMonthRevenue - kpis.PreviousMonthRevenue)
                    / kpis.PreviousMonthRevenue * 100m, 1);

            return new RevenueStatsDto
            {
                TotalRevenue = kpis.TotalRevenue,
                CurrentMonthRevenue = kpis.CurrentMonthRevenue,
                PreviousMonthRevenue = kpis.PreviousMonthRevenue,
                PendingRevenue = kpis.PendingRevenue,
                TotalTransactions = kpis.TotalTransactions,
                ActivePaidSubscriptions = kpis.ActivePaidSubscriptions,
                AvgRevenuePerBusiness = Math.Round(kpis.AvgRevenuePerBusiness, 2),
                MoMChangePercent = momPct,

                MonthlyRevenue = monthly.Select(m => new MonthlyRevenueDto
                {
                    MonthLabel = m.MonthLabel,
                    MonthDisplay = m.MonthDisplay,
                    Revenue = m.Revenue,
                    Transactions = m.Transactions,
                }).ToList(),

                PlanRevenue = byPlan.Select(p => new PlanRevenueDto
                {
                    Plan = p.Plan,
                    Revenue = p.Revenue,
                    Transactions = p.Transactions,
                    RevenuePercent = p.RevenuePercent,
                }).ToList(),

                RecentPayments = recent.Select(r => new RecentPaymentDto
                {
                    PaymentId = r.PaymentId,
                    BusinessName = r.BusinessName,
                    OwnerName = r.OwnerName,
                    Plan = r.Plan,
                    Amount = r.Amount,
                    Currency = r.Currency,
                    DurationMonths = r.DurationMonths,
                    PaymentMethod = r.PaymentMethod,
                    PaymentDate = r.PaymentDate,
                }).ToList(),
            };
        }

        // Replace MapPayment in the private mappers region
        private static PaymentHistoryItemDto MapPayment(SubscriptionPaymentRecord r) => new()
        {
            PaymentId = r.PaymentId,
            BusinessAccountId = r.BusinessAccountId,
            BusinessName = r.BusinessName,
            OwnerName = r.OwnerName,
            Plan = r.Plan,
            DurationMonths = r.DurationMonths,
            Amount = r.Amount,
            Currency = r.Currency,
            PaymentStatus = r.PaymentStatus,
            PaymentMethod = r.PaymentMethod,
            TransactionRef = r.TransactionRef,
            Notes = r.Notes,
            PaymentDate = r.PaymentDate,
            SubscriptionStartDate = r.SubscriptionStartDate,
            SubscriptionExpiry = r.SubscriptionExpiry,
        };

        // ── Private mappers ───────────────────────────────────────────────────────

        private static ShopListItemDto MapListItem(ShopListRecord r) => new()
        {
            BusinessAccountId = r.BusinessAccountId,
            BusinessName = r.BusinessName,
            OwnerName = r.OwnerName,
            BusinessPhone = r.BusinessPhone,
            BusinessEmail = r.BusinessEmail,
            IsActive = r.IsActive,
            CreatedAt = r.CreatedAt,
            ShopType = r.ShopType,
            SubscriptionPlan = r.SubscriptionPlan,
            SubscriptionStartDate = r.SubscriptionStartDate,
            SubscriptionExpiry = r.SubscriptionExpiry,
            DaysLeft = r.DaysLeft,
            UserCount = r.UserCount,
        };

        private static ShopDetailDto MapDetail(ShopDetailRecord r) => new()
        {
            BusinessAccountId = r.BusinessAccountId,
            BusinessName = r.BusinessName,
            OwnerName = r.OwnerName,
            BusinessPhone = r.BusinessPhone,
            BusinessEmail = r.BusinessEmail,
            IsActive = r.IsActive,
            CreatedAt = r.CreatedAt,
            ShopType = r.ShopType,
            SubscriptionPlan = r.SubscriptionPlan,
            SubscriptionStartDate = r.SubscriptionStartDate,
            SubscriptionExpiry = r.SubscriptionExpiry,
            DaysLeft = r.DaysLeft,
            UserCount = r.UserCount,
            Address = r.Address,
            Gstin = r.Gstin,
            ThemeColor = r.ThemeColor,
            Currency = r.Currency,
            TotalOrders = r.TotalOrders,
        };
    }
}
