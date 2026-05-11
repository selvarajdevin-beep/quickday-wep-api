namespace Shop.Web.API.Models.Domain
{
    /// <summary>
    /// Internal parameter object passed from AuthService → AuthRepository → usp_Register.
    /// Keeps the repository method signature clean.
    /// </summary>
    public class RegisterDbParams
    {
        // ── Step 1: Business Info ─────────────────────────────
        public string BusinessName { get; init; } = string.Empty;
        public string OwnerName { get; init; } = string.Empty;
        public string? BusinessPhone { get; init; }
        public string? BusinessEmail { get; init; }
        public string? Address { get; init; }
        public string? GSTIN { get; init; }
        public string ShopType { get; init; } = string.Empty;

        // ── Step 2: Admin User Info ───────────────────────────
        public string Username { get; init; } = string.Empty;
        public string UserPhone { get; init; } = string.Empty;
        public string? UserEmail { get; init; }
        public string PasswordHash { get; init; } = string.Empty;
        public string PasswordSalt { get; init; } = string.Empty;
        public string? AvatarInitials { get; init; }

        // ── Audit ─────────────────────────────────────────────
        public string? IpAddress { get; init; }
    }
}
