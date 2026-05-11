// Models/Domain/SettingsRecord.cs
namespace Shop.Web.API.Models.Domain;

/// <summary>
/// Flat record returned by usp_Settings_Get.
/// Maps exactly to the SELECT columns in the stored procedure.
/// </summary>
//public class SettingsRecord
//{
//    public int     BusinessAccountId     { get; init; }
//    public string  BusinessName          { get; init; } = string.Empty;
//    public string  OwnerName             { get; init; } = string.Empty;
//    public string? Phone                 { get; init; }
//    public string? Email                 { get; init; }
//    public string? Address               { get; init; }
//    public string? GSTIN                 { get; init; }
//    public string  ShopType              { get; init; } = "Other";
//    public string  ThemeColor            { get; init; } = "#0057FF";
//    public string  Currency              { get; init; } = "INR";
//    public string  CurrencySymbol        { get; init; } = "₹";
//    public string  SubscriptionPlan      { get; init; } = "Free";
//    public string? SubscriptionStartDate { get; init; }
//    public string  SubscriptionExpiry    { get; init; } = string.Empty;

//    /// <summary>Optimistic concurrency token — sent back to client, required on update.</summary>
//    public byte[]? RowVersion            { get; init; }
//}

public class SettingsRecord
{
    // BusinessAccounts
    public int BusinessAccountId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? GSTIN { get; set; }

    // BusinessSettings (existing)
    public string ShopType { get; set; } = "Other";
    public string ThemeColor { get; set; } = "#0057FF";
    public string Currency { get; set; } = "INR";
    public string CurrencySymbol { get; set; } = "₹";
    public string SubscriptionPlan { get; set; } = "Free";
    public string? SubscriptionStartDate { get; set; }
    public string SubscriptionExpiry { get; set; } = string.Empty;
    public byte[]? RowVersion { get; set; }

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
/// One permission row returned by usp_RolePermissions_Get.
/// </summary>
public class RolePermissionRecord
{
    public int    RolePermissionId  { get; init; }
    public int    BusinessAccountId { get; init; }
    public string Role              { get; init; } = string.Empty;
    public string Module            { get; init; } = string.Empty;
    public bool   CanView           { get; init; }
    public bool   CanCreate         { get; init; }
    public bool   CanEdit           { get; init; }
    public bool   CanDelete         { get; init; }
}
