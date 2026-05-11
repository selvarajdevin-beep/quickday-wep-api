namespace Shop.Web.API.Models.Responses
{
    /// <summary>
    /// Safe user object returned to the Angular client.
    /// Never include PasswordHash, Salt, or RefreshToken here.
    /// </summary>
    public class AuthUserDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string Role { get; init; } = string.Empty;  // 'Admin' | 'Worker'
        public bool isSuperAdmin { get; init; } = false;
        public string BusinessName { get; init; } = string.Empty;
        public int BusinessAccountId { get; init; }
        public string? AvatarInitials { get; init; }
        public string ThemeColor { get; init; } = "#0057FF";
        public string? ShopType { get; init; }
        public string Currency { get; init; } = "INR";
        public string CurrencySymbol { get; init; } = "₹";
        public string SubscriptionPlan { get; init; } = string.Empty;
        public DateTime SubscriptionExpiry { get; init; }
    }
}
