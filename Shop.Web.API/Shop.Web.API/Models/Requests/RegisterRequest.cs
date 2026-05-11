using System.ComponentModel.DataAnnotations;

namespace Shop.Web.API.Models.Requests
{
    public class RegisterRequest
    {
        // ── Step 1: Business Info ─────────────────────────────
        [Required(ErrorMessage = "Business name is required.")]
        [MaxLength(200)]
        public string BusinessName { get; init; } = string.Empty;

        [Required(ErrorMessage = "Owner name is required.")]
        [MaxLength(200)]
        public string OwnerName { get; init; } = string.Empty;

        [MaxLength(20)]
        public string? BusinessPhone { get; init; }

        [EmailAddress(ErrorMessage = "Enter a valid business email.")]
        [MaxLength(255)]
        public string? BusinessEmail { get; init; }

        [MaxLength(500)]
        public string? Address { get; init; }

        [MaxLength(20)]
        public string? GSTIN { get; init; }

        [Required(ErrorMessage = "Shop type is required.")]
        [MaxLength(100)]
        public string ShopType { get; init; } = string.Empty;

        // ── Step 2: Account Setup ─────────────────────────────
        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(100)]
        public string Username { get; init; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter a valid 10-digit mobile number.")]
        public string Phone { get; init; } = string.Empty;

        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [MaxLength(255)]
        public string? Email { get; init; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; init; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; init; } = string.Empty;
    }
}
