using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Repositories;
using Shop.Web.API.Services;

namespace Shop.Web.Api.Services;

public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _repo;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(ISettingsRepository repo, ILogger<SettingsService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // ── Get Settings ──────────────────────────────────────────

    public async Task<SettingsDto> GetSettingsAsync(int businessAccountId, int requestingUserId)
    {
        var record = await _repo.GetSettingsAsync(businessAccountId, requestingUserId);

        if (record is null)
        {
            _logger.LogWarning(
                "Settings not found for BusinessAccountId={Id}", businessAccountId);
            throw new NotFoundException("Business settings not found.");
        }

        return MapToDto(record);
    }

    // ── Update Settings ───────────────────────────────────────

    //public async Task<SettingsDto> UpdateSettingsAsync(
    //    int businessAccountId, int requestingUserId, UpdateSettingsRequest req)
    //{
    //    // Additional business-rule validation beyond data annotations
    //    var validShopTypes = new HashSet<string>
    //    {
    //        "Water Can Supplier","Bakery","Mobile Shop","Grocery Store","Pharmacy",
    //        "Stationery","Fruit & Vegetable","Dairy & Milk","Restaurant / Mess",
    //        "Hardware Store","Clothing & Textiles","Electronics","General Store","Other"
    //    };

    //    if (!validShopTypes.Contains(req.ShopType))
    //        throw new AppException("Invalid shop type selected.", "SETTINGS_2005");

    //    var record = await _repo.UpdateSettingsAsync(businessAccountId, requestingUserId, req);

    //    if (record is null)
    //    {
    //        _logger.LogError(
    //            "UpdateSettingsAsync returned null unexpectedly for BusinessAccountId={Id}",
    //            businessAccountId);
    //        throw new AppException("Settings update failed. Please try again.", "SETTINGS_UNEXPECTED");
    //    }

    //    _logger.LogInformation(
    //        "Settings updated for BusinessAccountId={Id} by UserId={UserId}",
    //        businessAccountId, requestingUserId);

    //    return MapToDto(record);
    //}

    public async Task<SettingsDto> UpdateSettingsAsync(
    int businessAccountId, int requestingUserId, UpdateSettingsRequest req)
    {
        var validShopTypes = new HashSet<string>
    {
        "Water Can Supplier","Bakery","Mobile Shop","Grocery Store","Pharmacy",
        "Stationery","Fruit & Vegetable","Dairy & Milk","Restaurant / Mess",
        "Hardware Store","Clothing & Textiles","Electronics","General Store","Other"
    };

        if (!validShopTypes.Contains(req.ShopType))
            throw new AppException("Invalid shop type selected.", "SETTINGS_2005");

        // Logo size guard — Base64 ~1.37 × original; 500 KB image ≈ 685 K chars
        if (req.LogoUrl is { Length: > 750_000 })
            throw new AppException(
                "Logo image is too large. Please use an image under 500 KB.", "SETTINGS_2011");

        var record = await _repo.UpdateSettingsAsync(businessAccountId, requestingUserId, req);

        if (record is null)
        {
            _logger.LogError(
                "UpdateSettingsAsync returned null for BusinessAccountId={Id}",
                businessAccountId);
            throw new AppException("Settings update failed. Please try again.", "SETTINGS_UNEXPECTED");
        }

        _logger.LogInformation(
            "Settings updated for BusinessAccountId={Id} by UserId={UserId}",
            businessAccountId, requestingUserId);

        return MapToDto(record);
    }

    // ── Get All Role Permissions (both roles in one call) ─────

    public async Task<AllRolePermissionsDto> GetAllRolePermissionsAsync(int businessAccountId)
    {
        // Fetch all rows for this business (both Admin and Worker)
        var allRows = await _repo.GetRolePermissionsAsync(businessAccountId, role: null);

        return new AllRolePermissionsDto
        {
            Admin = allRows.Where(r => r.Role == "Admin").Select(MapPermDto).ToList(),
            Worker = allRows.Where(r => r.Role == "Worker").Select(MapPermDto).ToList(),
        };
    }

    // ── Save Role Permissions ─────────────────────────────────

    public async Task<List<PermissionDto>> SaveRolePermissionsAsync(
        int businessAccountId, int requestingUserId,
        string role, SaveRolePermissionsRequest req)
    {
        // Normalise role casing and validate
        var normalisedRole = NormaliseRole(role);

        if (normalisedRole is null)
            throw new AppException("Invalid role. Must be 'Admin' or 'Worker'.", "PERMS_INVALID_ROLE");

        if (req.Permissions.Count == 0)
            throw new AppException("At least one permission entry is required.", "PERMS_EMPTY");

        // Ensure no Admin user can be completely locked out of Settings
        if (normalisedRole == "Admin")
        {
            var settingsPerm = req.Permissions.FirstOrDefault(
                p => p.Module.Equals("Settings", StringComparison.OrdinalIgnoreCase));

            if (settingsPerm is null || !settingsPerm.CanView)
                throw new AppException(
                    "Admins must retain view access to the Settings module.",
                    "PERMS_ADMIN_LOCKOUT");
        }

        var rows = await _repo.SaveRolePermissionsAsync(
            businessAccountId, requestingUserId, normalisedRole, req.Permissions);

        _logger.LogInformation(
            "Role permissions saved for Role={Role} BusinessAccountId={Id} by UserId={UserId}",
            normalisedRole, businessAccountId, requestingUserId);

        return rows.Select(MapPermDto).ToList();
    }

    public async Task<MyRolePermissionsDto> GetMyRolePermissionsAsync(
    int businessAccountId, string role)
    {
        // Fetch both in parallel — independent queries
        var permTask = _repo.GetMyRolePermissionsAsync(businessAccountId, role);
        var settingsTask = _repo.GetPublicSettingsAsync(businessAccountId);
        await Task.WhenAll(permTask, settingsTask);

        var perms = permTask.Result.Select(MapPermDto).ToList();
        var settings = settingsTask.Result;

        return new MyRolePermissionsDto
        {
            BusinessName = settings?.BusinessName ?? string.Empty,
            ThemeColor = settings?.ThemeColor ?? "#0057FF",
            Currency = settings?.Currency ?? "INR",
            CurrencySymbol = settings?.CurrencySymbol ?? "₹",
            ShopType = settings?.ShopType ?? "Other",
            Role = role,
            Permissions = perms,
        };
    }

    // ── Mapping helpers ───────────────────────────────────────

    //private static SettingsDto MapToDto(SettingsRecord r) => new()
    //{
    //    BusinessAccountId = r.BusinessAccountId,
    //    BusinessName = r.BusinessName,
    //    OwnerName = r.OwnerName,
    //    Phone = r.Phone ?? string.Empty,
    //    Email = r.Email ?? string.Empty,
    //    Address = r.Address ?? string.Empty,
    //    GSTIN = r.GSTIN ?? string.Empty,
    //    ShopType = r.ShopType,
    //    ThemeColor = r.ThemeColor,
    //    Currency = r.Currency,
    //    CurrencySymbol = r.CurrencySymbol,
    //    SubscriptionPlan = r.SubscriptionPlan,
    //    SubscriptionStartDate = r.SubscriptionStartDate,
    //    SubscriptionExpiry = r.SubscriptionExpiry,
    //    // Encode binary RowVersion as Base64 for safe JSON transport
    //    RowVersion = r.RowVersion is not null
    //                            ? Convert.ToBase64String(r.RowVersion)
    //                            : string.Empty,
    //};

    private static SettingsDto MapToDto(SettingsRecord r) => new()
    {
        BusinessAccountId = r.BusinessAccountId,
        BusinessName = r.BusinessName,
        OwnerName = r.OwnerName,
        Phone = r.Phone ?? string.Empty,
        Email = r.Email ?? string.Empty,
        Address = r.Address ?? string.Empty,
        GSTIN = r.GSTIN ?? string.Empty,
        ShopType = r.ShopType,
        ThemeColor = r.ThemeColor,
        Currency = r.Currency,
        CurrencySymbol = r.CurrencySymbol,
        SubscriptionPlan = r.SubscriptionPlan,
        SubscriptionStartDate = r.SubscriptionStartDate,
        SubscriptionExpiry = r.SubscriptionExpiry,
        RowVersion = r.RowVersion is not null
                           ? Convert.ToBase64String(r.RowVersion)
                           : string.Empty,
        // NEW
        GstEnabled = r.GstEnabled,
        GstType = r.GstType,
        CgstRate = r.CgstRate,
        SgstRate = r.SgstRate,
        IgstRate = r.IgstRate,
        ShowGstOnInvoice = r.ShowGstOnInvoice,
        LogoUrl = r.LogoUrl,
        ShowLogoOnInvoice = r.ShowLogoOnInvoice,
        InvoiceShowTime = r.InvoiceShowTime,
    };

    private static PermissionDto MapPermDto(RolePermissionRecord r) => new()
    {
        Module = r.Module,
        CanView = r.CanView,
        CanCreate = r.CanCreate,
        CanEdit = r.CanEdit,
        CanDelete = r.CanDelete,
    };

    private static string? NormaliseRole(string role) => role.ToLower() switch
    {
        "admin" => "Admin",
        "worker" => "Worker",
        _ => null
    };
}
