namespace Shop.Web.API.Models.Responses
{
    public class OrderDailySummaryDto
    {
        public DateTime Date { get; init; }
        public decimal Total { get; init; }
        public int Id { get; init; }
    }

    public class OrderRecentItemDto
    {
        public int Id { get; init; }
        public int CustomerId { get; init; }
        public string CustomerName { get; init; } = "";
        public string ItemsJson { get; init; } = "";
        public decimal GrandTotal { get; init; }
        public string Status { get; init; } = "";
        public DateTime CreatedAt { get; init; }
    }

    public class OrderDashboardSummaryDto
    {
        public List<OrderDailySummaryDto> DailyTotals { get; init; } = [];
        public List<OrderRecentItemDto> RecentOrders { get; init; } = [];
    }
}
