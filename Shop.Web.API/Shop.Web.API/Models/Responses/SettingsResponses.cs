// Models/Responses/SettingsResponses.cs
namespace Shop.Web.API.Models.Responses;

/// <summary>
/// Full settings DTO returned to the Angular client.
/// Mirrors BusinessSettings interface in shared-state.interfaces.ts.
/// </summary>
//public class SettingsDto
//{
//    public int     BusinessAccountId     { get; init; }
//    public string  BusinessName          { get; init; } = string.Empty;
//    public string  OwnerName             { get; init; } = string.Empty;
//    public string  Phone                 { get; init; } = string.Empty;
//    public string  Email                 { get; init; } = string.Empty;
//    public string  Address               { get; init; } = string.Empty;
//    public string  GSTIN                 { get; init; } = string.Empty;
//    public string  ShopType              { get; init; } = "Other";
//    public string  ThemeColor            { get; init; } = "#0057FF";
//    public string  Currency              { get; init; } = "INR";
//    public string  CurrencySymbol        { get; init; } = "₹";
//    public string  SubscriptionPlan      { get; init; } = "Free";
//    public string? SubscriptionStartDate { get; init; }
//    public string  SubscriptionExpiry    { get; init; } = string.Empty;

//    /// <summary>
//    /// Base64-encoded RowVersion — must be sent back on update for
//    /// optimistic concurrency control.
//    /// </summary>
//    public string RowVersion { get; init; } = string.Empty;
//}

public class SettingsDto
{
    // BusinessAccounts
    public int BusinessAccountId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string GSTIN { get; set; } = string.Empty;

    // BusinessSettings (existing)
    public string ShopType { get; set; } = "Other";
    public string ThemeColor { get; set; } = "#0057FF";
    public string Currency { get; set; } = "INR";
    public string CurrencySymbol { get; set; } = "₹";
    public string SubscriptionPlan { get; set; } = "Free";
    public string? SubscriptionStartDate { get; set; }
    public string SubscriptionExpiry { get; set; } = string.Empty;
    public string RowVersion { get; set; } = string.Empty;

    // BusinessSettings (NEW)
    public bool GstEnabled { get; set; } = false;
    public string GstType { get; set; } = "None";
    public decimal CgstRate { get; set; } = 0m;
    public decimal SgstRate { get; set; } = 0m;
    public decimal IgstRate { get; set; } = 0m;
    public bool ShowGstOnInvoice { get; set; } = true;
    public string? LogoUrl { get; set; }
    public bool ShowLogoOnInvoice { get; set; } = true;
    public bool InvoiceShowTime { get; set; } = true;
}


/// <summary>
/// One permission row for a role+module combination.
/// Mirrors the Permission interface in shared-state.interfaces.ts.
/// </summary>
public class PermissionDto
{
    public string Module    { get; init; } = string.Empty;
    public bool   CanView   { get; init; }
    public bool   CanCreate { get; init; }
    public bool   CanEdit   { get; init; }
    public bool   CanDelete { get; init; }
}

/// <summary>
/// All permissions for both Admin and Worker roles — returned on settings load.
/// </summary>
public class AllRolePermissionsDto
{
    public List<PermissionDto> Admin  { get; init; } = [];
    public List<PermissionDto> Worker { get; init; } = [];
}
