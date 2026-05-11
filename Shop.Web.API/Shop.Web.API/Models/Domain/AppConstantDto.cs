namespace Shop.Web.API.Models.Domain
{
    /// <summary>One item within a constant category.</summary>
    public sealed record AppConstantItem(
        string Value,
        string Label,
        string? Icon
    );

    /// <summary>
    /// Full constant catalogue returned by GET /api/settings/constants.
    /// Every property maps to one Category in AppConstants table.
    /// </summary>
    public sealed class AppConstantsDto
    {
        // ── Simple lookup lists ───────────────────────────────────────────────
        public List<AppConstantItem> PaymentTypes { get; init; } = [];
        public List<AppConstantItem> OrderStatuses { get; init; } = [];
        public List<AppConstantItem> PurchaseStatuses { get; init; } = [];
        public List<AppConstantItem> CustomerTypes { get; init; } = [];
        public List<AppConstantItem> ExpenseTypes { get; init; } = [];
        public List<AppConstantItem> GstTypes { get; init; } = [];
        public List<AppConstantItem> SubscriptionPlans { get; init; } = [];
        public List<AppConstantItem> Currencies { get; init; } = [];
        public List<AppConstantItem> ThemeColors { get; init; } = [];
        public List<AppConstantItem> SalaryTypes { get; init; } = [];
        public List<AppConstantItem> AppModules { get; init; } = [];

        // ── ShopType with icon ────────────────────────────────────────────────
        public List<AppConstantItem> ShopTypes { get; init; } = [];

        // ── ShopUnitTypes: shopType → list of unit strings ────────────────────
        // Value = ShopType name, Label = comma-separated units
        public Dictionary<string, List<string>> ShopUnitTypes { get; init; } = [];

        // ── ShopCategories: shopType → list of category strings ──────────────
        public Dictionary<string, List<string>> ShopCategories { get; init; } = [];

        // ── Plan features: plan name → list of feature strings ───────────────
        // Value = plan name, Label = pipe-separated features
        public Dictionary<string, List<string>> PlanFeatures { get; init; } = [];
        //public List<AppConstantItem> PlanPricing { get; init; } = [];

        public Dictionary<string, int> PlanPricing { get; set; } = new();
    }
}