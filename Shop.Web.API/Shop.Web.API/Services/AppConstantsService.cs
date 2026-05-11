using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Repositories;
using System.Text.RegularExpressions;

namespace Shop.Web.API.Services
{
    // Services/AppConstantsService.cs
    // ─────────────────────────────────────────────────────────────────────────────
    // Fetches all AppConstants rows once and maps them into AppConstantsDto.
    // Uses IMemoryCache so subsequent requests within the TTL pay zero DB cost.
    // Cache is invalidated automatically after 6 hours, or manually via
    // InvalidateCache() (call this if an admin edits the constants table).
    // ─────────────────────────────────────────────────────────────────────────────

    //public sealed class AppConstantsService : IAppConstantsService
    //{
    //    private const string CacheKey = "AppConstants_All";
    //    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(6);

    //    private readonly IAppConstantsRepository _repo;
    //    private readonly IMemoryCache _cache;
    //    private readonly ILogger<AppConstantsService> _logger;

    //    public AppConstantsService(
    //        IAppConstantsRepository repo,
    //        IMemoryCache cache,
    //        ILogger<AppConstantsService> logger)
    //    {
    //        _repo = repo;
    //        _cache = cache;
    //        _logger = logger;
    //    }

    //    // ── Public API ────────────────────────────────────────────────────────

    //    public async Task<AppConstantsDto> GetAllAsync()
    //    {
    //        if (_cache.TryGetValue(CacheKey, out AppConstantsDto? cached) && cached is not null)
    //            return cached;

    //        _logger.LogInformation("Cache miss — loading AppConstants from DB");
    //        var rows = await _repo.GetAllAsync();
    //        var dto = MapToDto(rows);

    //        _cache.Set(CacheKey, dto, CacheTtl);
    //        return dto;
    //    }

    //    /// <summary>
    //    /// Call this after any admin update to the AppConstants table to force
    //    /// the next request to re-fetch from the DB.
    //    /// </summary>
    //    public void InvalidateCache() => _cache.Remove(CacheKey);

    //    // ── Mapping ───────────────────────────────────────────────────────────

    //    private static AppConstantsDto MapToDto(IEnumerable<AppConstantRecord> rows)
    //    {
    //        // Group by category for easy lookup
    //        var byCategory = rows
    //            .GroupBy(r => r.Category)
    //            .ToDictionary(
    //                g => g.Key,
    //                g => g.OrderBy(r => r.SortOrder).ToList());

    //        // Helper: get a simple item list for a category
    //        static List<AppConstantItem> Items(
    //            Dictionary<string, List<AppConstantRecord>> dict, string key)
    //        {
    //            if (!dict.TryGetValue(key, out var list)) return [];
    //            return list.Select(r => new AppConstantItem(r.Value, r.Label, r.Icon)).ToList();
    //        }

    //        // Helper: build a dictionary where Value → split(Label) for multi-value categories
    //        static Dictionary<string, List<string>> SplitMap(
    //            Dictionary<string, List<AppConstantRecord>> dict,
    //            string key, char separator)
    //        {
    //            if (!dict.TryGetValue(key, out var list)) return [];
    //            return list.ToDictionary(
    //                r => r.Value,
    //                r => r.Label.Split(separator, StringSplitOptions.RemoveEmptyEntries)
    //                             .Select(s => s.Trim())
    //                             .ToList());
    //        }

    //        return new AppConstantsDto
    //        {
    //            PaymentTypes = Items(byCategory, "PaymentType"),
    //            OrderStatuses = Items(byCategory, "OrderStatus"),
    //            PurchaseStatuses = Items(byCategory, "PurchaseStatus"),
    //            CustomerTypes = Items(byCategory, "CustomerType"),
    //            ExpenseTypes = Items(byCategory, "ExpenseType"),
    //            GstTypes = Items(byCategory, "GstType"),
    //            SubscriptionPlans = Items(byCategory, "SubscriptionPlan"),
    //            Currencies = Items(byCategory, "Currency"),
    //            ThemeColors = Items(byCategory, "ThemeColor"),
    //            SalaryTypes = Items(byCategory, "SalaryType"),
    //            AppModules = Items(byCategory, "AppModule"),
    //            ShopTypes = Items(byCategory, "ShopType"),

    //            // ShopUnitTypes:  ShopType value → list of unit strings (comma-separated)
    //            ShopUnitTypes = SplitMap(byCategory, "ShopUnitTypes", ','),

    //            // ShopCategories: ShopType value → list of category strings (comma-separated)
    //            ShopCategories = SplitMap(byCategory, "ShopCategories", ','),

    //            // PlanFeatures:   plan name → list of feature strings (pipe-separated)
    //            PlanFeatures = SplitMap(byCategory, "PlanFeatures", '|'),
    //        };
    //    }
    //}

    public sealed class AppConstantsService : IAppConstantsService
    {
        private const string CacheKey = "AppConstants_All";
        private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(6);

        // IServiceScopeFactory is itself a singleton — safe to inject here.
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AppConstantsService> _logger;

        public AppConstantsService(
            IServiceScopeFactory scopeFactory,
            IMemoryCache cache,
            ILogger<AppConstantsService> logger)
        {
            _scopeFactory = scopeFactory;
            _cache = cache;
            _logger = logger;
        }

        // ── Public API ────────────────────────────────────────────────────────

        public async Task<AppConstantsDto> GetAllAsync()
        {
            // Fast path — already cached
            if (_cache.TryGetValue(CacheKey, out AppConstantsDto? cached) && cached is not null)
                return cached;

            _logger.LogInformation("AppConstants cache miss — loading from DB");

            // Create a transient scope just for this DB call.
            // The scope (and its SqlConnection) is disposed immediately after.
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IAppConstantsRepository>();

            var rows = await repo.GetAllAsync();
            var dto = MapToDto(rows);

            _cache.Set(CacheKey, dto, CacheTtl);
            return dto;
        }

        /// <summary>
        /// Forces the next request to re-fetch from the DB.
        /// Call this if an admin updates the AppConstants table at runtime.
        /// </summary>
        public void InvalidateCache() => _cache.Remove(CacheKey);

        // ── Mapping ───────────────────────────────────────────────────────────

        private static AppConstantsDto MapToDto(IEnumerable<AppConstantRecord> rows)
        {
            var byCategory = rows
                .GroupBy(r => r.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(r => r.SortOrder).ToList());

            static List<AppConstantItem> Items(
                Dictionary<string, List<AppConstantRecord>> dict, string key)
            {
                if (!dict.TryGetValue(key, out var list)) return [];
                return list.Select(r => new AppConstantItem(r.Value, r.Label, r.Icon)).ToList();
            }

            static Dictionary<string, List<string>> SplitMap(
                Dictionary<string, List<AppConstantRecord>> dict,
                string key, char separator)
            {
                if (!dict.TryGetValue(key, out var list)) return [];
                return list.ToDictionary(
                    r => r.Value,
                    r => r.Label.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(s => s.Trim())
                                 .ToList());
            }

            static Dictionary<string, int> ParseIntMap(
                Dictionary<string, List<AppConstantRecord>> dict,
                string key)
            {
                if (!dict.TryGetValue(key, out var list)) return new();

                return list.ToDictionary(
                    r => r.Value,
                    r => int.TryParse(r.Label, out var n) ? n : 0
                );
            }

            return new AppConstantsDto
            {
                PaymentTypes = Items(byCategory, "PaymentType"),
                OrderStatuses = Items(byCategory, "OrderStatus"),
                PurchaseStatuses = Items(byCategory, "PurchaseStatus"),
                CustomerTypes = Items(byCategory, "CustomerType"),
                ExpenseTypes = Items(byCategory, "ExpenseType"),
                GstTypes = Items(byCategory, "GstType"),
                SubscriptionPlans = Items(byCategory, "SubscriptionPlan"),
                Currencies = Items(byCategory, "Currency"),
                ThemeColors = Items(byCategory, "ThemeColor"),
                SalaryTypes = Items(byCategory, "SalaryType"),
                AppModules = Items(byCategory, "AppModule"),
                ShopTypes = Items(byCategory, "ShopType"),
                ShopUnitTypes = SplitMap(byCategory, "ShopUnitTypes", ','),
                ShopCategories = SplitMap(byCategory, "ShopCategories", ','),
                PlanFeatures = SplitMap(byCategory, "PlanFeatures", '|'),
                PlanPricing = ParseIntMap(byCategory, "PlanPricing"),
                //PlanPricing = Items(byCategory, "PlanPricing"),
            };
        }
    }
}
