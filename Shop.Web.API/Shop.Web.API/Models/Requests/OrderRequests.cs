using System.ComponentModel.DataAnnotations;

namespace Shop.Web.API.Models.Requests
{
    public class OrderItemRequest
    {
        [Range(1, int.MaxValue)] public int ProductId { get; init; }
        [Required][MaxLength(200)] public string ProductName { get; init; } = string.Empty;
        [Range(1, int.MaxValue)] public int Quantity { get; init; }
        [Range(0, double.MaxValue)] public decimal PricePerUnit { get; init; }
        public decimal Total { get; init; }
    }

    public class CreateOrderRequest
    {
        [Range(0, int.MaxValue)]
        public int CustomerId { get; init; }  // 0 = walk-in

        [Required]
        [MaxLength(200)]
        public string CustomerName { get; init; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required.")]
        public List<OrderItemRequest> Items { get; init; } = [];

        [Range(0.01, double.MaxValue)] public decimal GrandTotal { get; init; }
        [Range(0, double.MaxValue)] public decimal PaidAmount { get; init; }
        [Range(0, double.MaxValue)] public decimal Balance { get; init; }

        [Required]
        [RegularExpression(@"^(Cash|UPI|Credit)$")]
        public string PaymentType { get; init; } = "Cash";

        [Required]
        [RegularExpression(@"^(Paid|Partial|Credit)$")]
        public string Status { get; init; } = "Paid";

        [MaxLength(500)]
        public string? DeliveryNote { get; init; }

        // NEW — GST snapshot sent from Angular
        public decimal SubTotal { get; init; }
        public decimal TaxableAmount { get; init; }
        public string GstType { get; init; } = "None";
        public decimal CgstRate { get; init; }
        public decimal SgstRate { get; init; }
        public decimal IgstRate { get; init; }
        public decimal CgstAmount { get; init; }
        public decimal SgstAmount { get; init; }
        public decimal IgstAmount { get; init; }
        public decimal TotalGst { get; init; }
    }

    public class UpdateOrderRequest : CreateOrderRequest
    {
        [Required] public string RowVersion { get; init; } = string.Empty;
    }

    public class RecordPaymentRequest
    {
        [Range(0.01, double.MaxValue)] public decimal Amount { get; init; }
        [Required][RegularExpression(@"^(Cash|UPI)$")] public string PaymentType { get; init; } = "Cash";
        [MaxLength(500)] public string? Note { get; init; }
        public int? OrderId { get; init; }
    }

}
