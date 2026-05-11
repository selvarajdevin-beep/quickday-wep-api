namespace Shop.Web.API.Models.Domain
{
    /// <summary>
    /// Maps directly to the result set returned by usp_Login and usp_RefreshToken.
    /// Property names must match SQL column aliases exactly (Dapper maps by name).
    /// </summary>
    //public class UserRecord
    //{
    //    public int UserId { get; init; }
    //    public int BusinessAccountId { get; init; }
    //    public string Name { get; init; } = string.Empty;
    //    public string Phone { get; init; } = string.Empty;
    //    public string? Email { get; init; }
    //    public string Role { get; init; } = string.Empty;
    //    public string Status { get; init; } = string.Empty;
    //    public string? AvatarInitials { get; init; }
    //    public string PasswordHash { get; init; } = string.Empty;
    //    public string PasswordSalt { get; init; } = string.Empty;
    //    public int FailedLoginAttempts { get; init; }
    //    public DateTime? LockedUntil { get; init; }
    //    public string BusinessName { get; init; } = string.Empty;
    //    public string ThemeColor { get; init; } = "#0057FF";
    //    public string SubscriptionPlan { get; init; } = string.Empty;
    //    public DateTime SubscriptionExpiry { get; init; }
    //    public string? ShopType { get; init; }
    //    public string Currency { get; init; } = "INR";
    //    public string CurrencySymbol { get; init; } = "₹";
    //}

    /// <summary>
    /// Flat record returned by usp_Users_GetAll / usp_Users_GetById.
    /// PermissionsJson is a raw JSON string from FOR JSON PATH — parsed in the
    /// repository before being mapped to the response DTO.
    /// </summary>

    public class UserRecord
    {
        // Core IDs
        public int Id { get; init; }                 // Primary key
        public int UserId { get; init; }             // (Optional legacy compatibility)
        public int BusinessAccountId { get; init; }

        // Basic Info
        public string Name { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string Role { get; init; } = "Worker";
        public string Status { get; init; } = "Active";
        public string? AvatarInitials { get; init; }

        // Additional Profile Info
        public string? Designation { get; init; }
        public string? Department { get; init; }
        public string? Address { get; init; }
        public string? EmergencyContact { get; init; }
        public string? Notes { get; init; }
        public string? DateOfJoining { get; init; }   // ISO format string

        // Salary & Banking
        public decimal MonthlySalary { get; init; }
        public string? SalaryType { get; init; }
        public string? BankAccount { get; init; }
        public string? BankName { get; init; }
        public string? IFSC { get; init; }

        // Authentication & Security
        public string PasswordHash { get; init; } = string.Empty;
        public string PasswordSalt { get; init; } = string.Empty;
        public int FailedLoginAttempts { get; init; }
        public DateTime? LockedUntil { get; init; }
        public DateTime? LastLoginAt { get; init; }
        public bool IsSuperAdmin { get; init; }

        // Business Info
        public string BusinessName { get; init; } = string.Empty;
        public string? ShopType { get; init; }

        // Subscription & Theme
        public string ThemeColor { get; init; } = "#0057FF";
        public string SubscriptionPlan { get; init; } = string.Empty;
        public DateTime SubscriptionExpiry { get; init; }

        // Currency
        public string Currency { get; init; } = "INR";
        public string CurrencySymbol { get; init; } = "₹";

        // Metadata
        public DateTime CreatedAt { get; init; }

        /// <summary>
        /// JSON string: [{"module":"Billing","canView":true,…},…]
        /// </summary>
        public string? PermissionsJson { get; init; }

        /// <summary>
        /// Hex RowVersion for optimistic concurrency
        /// </summary>
        //public string? RowVersion { get; init; }
        public byte[]? RowVersion { get; init; }
    }
}
