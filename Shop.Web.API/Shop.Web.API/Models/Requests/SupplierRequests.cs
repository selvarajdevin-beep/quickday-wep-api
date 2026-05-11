using System.ComponentModel.DataAnnotations;

namespace Shop.Web.API.Models.Requests
{
    public class CreateSupplierRequest
    {
        [Required(ErrorMessage = "Supplier name is required.")]
        [MaxLength(200)]
        public string Name { get; init; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be a 10-digit number.")]
        public string Phone { get; init; } = string.Empty;

        [MaxLength(255)]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; init; }

        [MaxLength(500)] public string? Address { get; init; }
        [MaxLength(20)] public string? GSTIN { get; init; }
        [MaxLength(200)] public string? ContactPerson { get; init; }
        [MaxLength(1000)] public string? Notes { get; init; }
    }

    public class UpdateSupplierRequest
    {
        [Required(ErrorMessage = "Supplier name is required.")]
        [MaxLength(200)]
        public string Name { get; init; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be a 10-digit number.")]
        public string Phone { get; init; } = string.Empty;

        [MaxLength(255)]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; init; }

        [MaxLength(500)] public string? Address { get; init; }
        [MaxLength(20)] public string? GSTIN { get; init; }
        [MaxLength(200)] public string? ContactPerson { get; init; }
        [MaxLength(1000)] public string? Notes { get; init; }
        public bool Active { get; init; }

        [Required(ErrorMessage = "Row version is required.")]
        public string RowVersion { get; init; } = string.Empty;
    }

    public class RecordSupplierPaymentRequest
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; init; }
    }

}
