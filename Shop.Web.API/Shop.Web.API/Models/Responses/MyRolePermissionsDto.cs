namespace Shop.Web.API.Models.Responses
{
    /// Returned by GET /api/settings/permissions/my-role
    /// Contains the caller's own role permissions + public settings
    /// (theme, businessName) needed for ShellComponent.
    public class MyRolePermissionsDto
    {
        public string BusinessName { get; init; } = string.Empty;
        public string ThemeColor { get; init; } = "#0057FF";
        public string Currency { get; init; } = "INR";
        public string CurrencySymbol { get; init; } = "₹";
        public string ShopType { get; init; } = "Other";
        public string Role { get; init; } = string.Empty;
        public List<PermissionDto> Permissions { get; init; } = [];
    }
}
