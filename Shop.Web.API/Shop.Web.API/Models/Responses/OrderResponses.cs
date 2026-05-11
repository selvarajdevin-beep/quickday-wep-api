namespace Shop.Web.API.Models.Responses
{
    public class OrderItemDto
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public decimal PricePerUnit { get; init; }
        public decimal Total { get; init; }
    }

    public class OrderDto
    {
        public int Id { get; init; }
        public int BusinessAccountId { get; init; }
        public int CustomerId { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public List<OrderItemDto> Items { get; init; } = [];
        public decimal GrandTotal { get; init; }
        public decimal PaidAmount { get; init; }
        public decimal Balance { get; init; }
        public string PaymentType { get; init; } = "Cash";
        public string Status { get; init; } = "Paid";
        public string? DeliveryNote { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public string RowVersion { get; init; } = string.Empty;

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

    public class PaymentDto
    {
        public int Id { get; init; }
        public int CustomerId { get; init; }
        public int? OrderId { get; init; }
        public decimal Amount { get; init; }
        public string PaymentType { get; init; } = "Cash";
        public string Note { get; init; } = string.Empty;
        public DateTime Date { get; init; }
    }

    public class TodaySummaryDto
    {
        public decimal TodaySales { get; init; }
        public int TodayOrders { get; init; }
        public decimal CashAmount { get; init; }
        public decimal UpiAmount { get; init; }
        public decimal CreditAmount { get; init; }
        public int TotalCustomers { get; init; }
        public decimal CreditPending { get; init; }
    }

    public sealed class OrderHistorySummaryDto
    {
        public int TotalOrders { get; init; }
        public decimal TotalSales { get; init; }
        public decimal TotalPaid { get; init; }
        public decimal TotalDue { get; init; }
    }

    public sealed class PagedOrderHistoryResponse
    {
        public List<OrderDto> Items { get; init; } = [];
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 1;
        public bool HasNext => Page < TotalPages;
        public bool HasPrev => Page > 1;
        public OrderHistorySummaryDto Summary { get; init; } = new();
    }

    public sealed class OrderFilteredSummary
    {
        public int TotalCount { get; init; }
        public decimal TotalSales { get; init; }
        public decimal TotalPaid { get; init; }
        public decimal TotalDue { get; init; }
    }

}
