namespace Shop.Web.API.Models.Requests
{
    public class CustomerReportParams
    {
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
        public string? Search { get; set; }
        /// <summary>totalOrders | totalSales | totalPaid | totalDue | lastOrderDate</summary>
        public string SortBy { get; set; } = "totalSales";
        /// <summary>ASC | DESC</summary>
        public string SortDir { get; set; } = "DESC";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PurchaseReportParams
    {
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
