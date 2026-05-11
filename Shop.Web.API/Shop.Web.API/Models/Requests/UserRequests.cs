// Models/Requests/UserRequests.cs
using System.ComponentModel.DataAnnotations;

namespace Shop.Web.Api.Models.Requests
{
    /// <summary>Payload for POST api/users — create a new user.</summary>
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100, ErrorMessage = "Name must be 100 characters or less.")]
        public string Username { get; init; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be a 10-digit number.")]
        public string Phone { get; init; } = string.Empty;

        [MaxLength(255)]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; init; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; init; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression(@"^(Admin|Worker)$", ErrorMessage = "Role must be Admin or Worker.")]
        public string Role { get; init; } = "Worker";

        // HR & Salary (all optional)
        [MaxLength(100)] public string? Designation { get; init; }
        [MaxLength(100)] public string? Department { get; init; }
        [MaxLength(500)] public string? Address { get; init; }
        [MaxLength(100)] public string? EmergencyContact { get; init; }
        [MaxLength(1000)] public string? Notes { get; init; }
        public string? DateOfJoining { get; init; }   // "yyyy-MM-dd"
        public decimal? MonthlySalary { get; init; }
        [MaxLength(20)] public string? SalaryType { get; init; }
        [MaxLength(50)] public string? BankAccount { get; init; }
        [MaxLength(100)] public string? BankName { get; init; }
        [MaxLength(20)] public string? IFSC { get; init; }
    }

    /// <summary>Payload for PUT api/users/{id} — update existing user (no password change).</summary>
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100)]
        public string Username { get; init; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be a 10-digit number.")]
        public string Phone { get; init; } = string.Empty;

        [MaxLength(255)]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; init; }

        [Required]
        [RegularExpression(@"^(Admin|Worker)$", ErrorMessage = "Role must be Admin or Worker.")]
        public string Role { get; init; } = "Worker";

        [MaxLength(100)] public string? Designation { get; init; }
        [MaxLength(100)] public string? Department { get; init; }
        [MaxLength(500)] public string? Address { get; init; }
        [MaxLength(100)] public string? EmergencyContact { get; init; }
        [MaxLength(1000)] public string? Notes { get; init; }
        public string? DateOfJoining { get; init; }
        public decimal? MonthlySalary { get; init; }
        [MaxLength(20)] public string? SalaryType { get; init; }
        [MaxLength(50)] public string? BankAccount { get; init; }
        [MaxLength(100)] public string? BankName { get; init; }
        [MaxLength(20)] public string? IFSC { get; init; }

        /// <summary>Base64-encoded RowVersion — required for optimistic concurrency.</summary>
        [Required(ErrorMessage = "Row version is required.")]
        public string RowVersion { get; init; } = string.Empty;
    }

    /// <summary>Payload for POST api/users/{id}/reset-password.</summary>
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string NewPassword { get; init; } = string.Empty;
    }

}