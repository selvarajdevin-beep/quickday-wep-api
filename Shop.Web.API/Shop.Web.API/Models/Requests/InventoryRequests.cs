using System.ComponentModel.DataAnnotations;

namespace Shop.Web.API.Models.Requests
{
    public class AdjustStockRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; init; }

        [Required]
        [RegularExpression(@"^(IN|OUT)$", ErrorMessage = "Type must be IN or OUT.")]
        public string Type { get; init; } = string.Empty;

        [Required(ErrorMessage = "Reason is required.")]
        [MaxLength(500)]
        public string Reason { get; init; } = string.Empty;

        [MaxLength(100)]
        public string? Reference { get; init; }
    }

    public class UpdateMinStockAlertRequest
    {
        [Range(0, int.MaxValue, ErrorMessage = "Min stock alert cannot be negative.")]
        public int MinStockAlert { get; init; }
    }

}
