// Models/Requests/SettingsRequests.cs
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shop.Web.API.Models.Requests;

/// <summary>
/// Payload for PUT api/settings — updates business profile + preferences.
/// </summary>
//public class UpdateSettingsRequest
//{
//    // ── Business profile ──────────────────────────────────────
//    [Required(ErrorMessage = "Business name is required.")]
//    [MaxLength(200, ErrorMessage = "Business name must be 200 characters or less.")]
//    public string BusinessName { get; init; } = string.Empty;

//    [Required(ErrorMessage = "Owner name is required.")]
//    [MaxLength(200, ErrorMessage = "Owner name must be 200 characters or less.")]
//    public string OwnerName { get; init; } = string.Empty;

//    [MaxLength(20)]
//    [RegularExpression(@"^\d{10}$",
//        ErrorMessage = "Phone must be a 10-digit number.")]
//    public string? BusinessPhone { get; init; }

//    [MaxLength(255)]
//    [EmailAddress(ErrorMessage = "Invalid email address.")]
//    public string? BusinessEmail { get; init; }

//    [MaxLength(500)]
//    public string? Address { get; init; }

//    [MaxLength(20)]
//    public string? GSTIN { get; init; }

//    // ── Preferences ───────────────────────────────────────────
//    [Required(ErrorMessage = "Shop type is required.")]
//    [MaxLength(100)]
//    public string ShopType { get; init; } = string.Empty;

//    [Required(ErrorMessage = "Theme color is required.")]
//    [RegularExpression(@"^#[0-9A-Fa-f]{6}$",
//        ErrorMessage = "Theme color must be a valid hex color (e.g. #0057FF).")]
//    public string ThemeColor { get; init; } = "#0057FF";

//    [Required]
//    [RegularExpression(@"^(INR|USD|EUR|GBP)$",
//        ErrorMessage = "Currency must be one of: INR, USD, EUR, GBP.")]
//    public string Currency { get; init; } = "INR";

//    /// <summary>
//    /// Base64-encoded RowVersion for optimistic concurrency.
//    /// Must be supplied from the value returned by GET /settings.
//    /// </summary>
//    [Required(ErrorMessage = "Row version is required for concurrency control.")]
//    public string RowVersion { get; init; } = string.Empty;
//}

public class UpdateSettingsRequest
{
    // ── Business profile ──────────────────────────────────────────────────

    [Required(ErrorMessage = "Business name is required.")]
    [MaxLength(200)]
    public string BusinessName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Owner name is required.")]
    [MaxLength(200)]
    public string OwnerName { get; init; } = string.Empty;

    [MaxLength(20)]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be a 10-digit number.")]
    public string? BusinessPhone { get; init; }

    [MaxLength(255)]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string? BusinessEmail { get; init; }

    [MaxLength(500)]
    public string? Address { get; init; }

    [MaxLength(20)]
    public string? GSTIN { get; init; }

    // ── Preferences ───────────────────────────────────────────────────────

    [Required(ErrorMessage = "Shop type is required.")]
    [MaxLength(100)]
    public string ShopType { get; init; } = string.Empty;

    [Required]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$",
        ErrorMessage = "Theme color must be a valid hex color (e.g. #0057FF).")]
    public string ThemeColor { get; init; } = "#0057FF";

    [Required]
    [RegularExpression(@"^(INR|USD|EUR|GBP)$",
        ErrorMessage = "Currency must be one of: INR, USD, EUR, GBP.")]
    public string Currency { get; init; } = "INR";

    /// <summary>Base64-encoded RowVersion for optimistic concurrency.</summary>
    [Required(ErrorMessage = "Row version is required for concurrency control.")]
    public string RowVersion { get; init; } = string.Empty;

    // ── NEW: GST ──────────────────────────────────────────────────────────

    public bool GstEnabled { get; init; } = false;

    /// <summary>'None' | 'GST' | 'IGST'</summary>
    [RegularExpression(@"^(None|GST|IGST)$",
        ErrorMessage = "GstType must be None, GST, or IGST.")]
    public string GstType { get; init; } = "None";

    [Range(0, 50, ErrorMessage = "CGST rate must be between 0 and 50.")]
    public decimal CgstRate { get; init; } = 0m;

    [Range(0, 50, ErrorMessage = "SGST rate must be between 0 and 50.")]
    public decimal SgstRate { get; init; } = 0m;

    [Range(0, 50, ErrorMessage = "IGST rate must be between 0 and 50.")]
    public decimal IgstRate { get; init; } = 0m;

    public bool ShowGstOnInvoice { get; init; } = true;

    // ── NEW: Logo ─────────────────────────────────────────────────────────

    /// <summary>
    /// CDN URL or Base64 data-URL (max ~500 KB image ≈ 680 K chars encoded).
    /// Null/empty to remove the logo.
    /// </summary>
    public string? LogoUrl { get; init; }

    public bool ShowLogoOnInvoice { get; init; } = true;

    // ── NEW: Invoice display ──────────────────────────────────────────────

    /// <summary>When true, show date AND time on the printed invoice.</summary>
    public bool InvoiceShowTime { get; init; } = true;
}

/// <summary>
/// One permission entry inside SaveRolePermissionsRequest.
/// </summary>
public class PermissionEntry
{
    [Required]
    [MaxLength(100)]
    public string Module    { get; init; } = string.Empty;
    public bool   CanView   { get; init; }
    public bool   CanCreate { get; init; }
    public bool   CanEdit   { get; init; }
    public bool   CanDelete { get; init; }
}

/// <summary>
/// Payload for PUT api/settings/permissions/{role}.
/// </summary>
public class SaveRolePermissionsRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one permission entry is required.")]
    public List<PermissionEntry> Permissions { get; init; } = [];
}
