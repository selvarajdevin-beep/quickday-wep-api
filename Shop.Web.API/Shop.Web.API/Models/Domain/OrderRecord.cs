namespace Shop.Web.API.Models.Domain
{
    public class OrderRecord
    {
        public int Id { get; init; }
        public int BusinessAccountId { get; init; }
        public int CustomerId { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public string ItemsJson { get; init; } = "[]";
        public decimal GrandTotal { get; init; }
        public decimal PaidAmount { get; init; }
        public decimal Balance { get; init; }
        public string PaymentType { get; init; } = "Cash";
        public string Status { get; init; } = "Paid";
        public string? DeliveryNote { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        //public string? RowVersion { get; init; }
        public byte[]? RowVersion { get; init; }

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

    public class PaymentRecord
    {
        public int Id { get; init; }
        public int BusinessAccountId { get; init; }
        public int CustomerId { get; init; }
        public int? OrderId { get; init; }
        public decimal Amount { get; init; }
        public string PaymentType { get; init; } = "Cash";
        public string Note { get; init; } = string.Empty;
        public DateTime Date { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public class TodaySummaryRecord
    {
        public decimal TodaySales { get; init; }
        public int TodayOrders { get; init; }
        public decimal CashAmount { get; init; }
        public decimal UpiAmount { get; init; }
        public decimal CreditAmount { get; init; }
    }
}
