using System.ComponentModel.DataAnnotations;

namespace Shop.Web.API.Models.Requests
{
    public class CreateCustomerRequest
    {
        [Required(ErrorMessage = "Customer name is required.")]
        [MaxLength(200)]
        public string Name { get; init; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be a 10-digit number.")]
        public string Phone { get; init; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; init; }

        [Required]
        [RegularExpression(@"^(Hotel|Home)$", ErrorMessage = "Customer type must be Hotel or Home.")]
        public string CustomerType { get; init; } = "Home";

        [Range(0.01, double.MaxValue, ErrorMessage = "Default price must be greater than zero.")]
        public decimal DefaultPricePerCan { get; init; } = 35;

        [Range(1, int.MaxValue, ErrorMessage = "Default price product ID is required.")]
        public int DefaultPriceProductId { get; init; } = 1;

        public bool UsePriceFromProduct { get; init; }
    }

    public class UpdateCustomerRequest : CreateCustomerRequest
    {
        [Required(ErrorMessage = "Row version is required.")]
        public string RowVersion { get; init; } = string.Empty;
    }
}
