// Models/Responses/UserResponses.cs
using Shop.Web.API.Models.Responses;

namespace Shop.Web.Api.Models.Responses
{

    /// <summary>
    /// Full user DTO returned to the Angular client.
    /// Mirrors AppUser interface in shared-state.interfaces.ts.
    /// </summary>
    public class UserDto
    {
        public int Id { get; init; }
        public int BusinessAccountId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Role { get; init; } = "Worker";
        public string Status { get; init; } = "Active";
        public string? Designation { get; init; }
        public string? Department { get; init; }
        public string? Address { get; init; }
        public string? EmergencyContact { get; init; }
        public string? Notes { get; init; }
        public string? AvatarInitials { get; init; }
        public string? DateOfJoining { get; init; }
        public DateTime? LastLogin { get; init; }
        public DateTime CreatedAt { get; init; }

        /// <summary>Salary details — nested object matching Angular SalaryDetail interface.</summary>
        public SalaryDetailDto? SalaryDetails { get; init; }

        /// <summary>Effective permissions for this user's role.</summary>
        public List<PermissionDto> Permissions { get; init; } = [];

        /// <summary>
        /// Base64-encoded RowVersion — must be sent back on UPDATE for
        /// optimistic concurrency control.
        /// </summary>
        public string RowVersion { get; init; } = string.Empty;
    }

    public class SalaryDetailDto
    {
        public decimal MonthlySalary { get; init; }
        public string SalaryType { get; init; } = "Fixed";
        public string? BankAccount { get; init; }
        public string? BankName { get; init; }
        public string? IFSC { get; init; }
    }
}