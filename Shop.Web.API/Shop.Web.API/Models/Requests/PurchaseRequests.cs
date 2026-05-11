using System.ComponentModel.DataAnnotations;

namespace Shop.Web.API.Models.Requests
{
    public class PurchaseItemRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Product ID must be valid.")]
        public int ProductId { get; init; }

        [Required]
        [MaxLength(200)]
        public string ProductName { get; init; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; init; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal PricePerUnit { get; init; }

        public decimal Total { get; init; }
    }

    public class CreatePurchaseRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Supplier ID is required.")]
        public int SupplierId { get; init; }

        [Required(ErrorMessage = "Supplier name is required.")]
        [MaxLength(200)]
        public string SupplierName { get; init; } = string.Empty;

        [Required(ErrorMessage = "At least one item is required.")]
        [MinLength(1, ErrorMessage = "At least one item is required.")]
        public List<PurchaseItemRequest> Items { get; init; } = [];

        [Range(0.01, double.MaxValue, ErrorMessage = "Grand total must be greater than zero.")]
        public decimal GrandTotal { get; init; }

        [Range(0, double.MaxValue)]
        public decimal PaidAmount { get; init; }

        [Range(0, double.MaxValue)]
        public decimal Balance { get; init; }

        [Required]
        [RegularExpression(@"^(Paid|Credit)$", ErrorMessage = "Payment status must be Paid or Credit.")]
        public string PaymentStatus { get; init; } = "Paid";

        [MaxLength(1000)]
        public string? Notes { get; init; }
    }

    public class UpdatePurchaseRequest : CreatePurchaseRequest
    {
        [Required(ErrorMessage = "Row version is required.")]
        public string RowVersion { get; init; } = string.Empty;
    }

}
