using System.ComponentModel.DataAnnotations;

namespace Shop.Web.API.Models.Requests
{
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Product name is required.")]
        [MaxLength(200, ErrorMessage = "Product name cannot exceed 200 characters.")]
        public string Name { get; init; } = string.Empty;

        [Required(ErrorMessage = "Unit type is required.")]
        [MaxLength(50, ErrorMessage = "Unit type cannot exceed 50 characters.")]
        public string UnitType { get; init; } = string.Empty;

        [MaxLength(50)]
        public string? Capacity { get; init; }

        [MaxLength(100)]
        public string? Category { get; init; }

        [Range(0, double.MaxValue, ErrorMessage = "Selling price cannot be negative.")]
        public decimal SellingPrice { get; init; }

        [Range(0, double.MaxValue, ErrorMessage = "Purchase price cannot be negative.")]
        public decimal PurchasePrice { get; init; }

        [Range(0, int.MaxValue, ErrorMessage = "Minimum stock alert cannot be negative.")]
        public int MinStockAlert { get; init; } = 10;

        public bool Active { get; init; } = true;
    }

    public class UpdateProductRequest : CreateProductRequest
    {
        [Required(ErrorMessage = "Row version is required for updates.")]
        public string RowVersion { get; init; } = string.Empty;
    }
}
