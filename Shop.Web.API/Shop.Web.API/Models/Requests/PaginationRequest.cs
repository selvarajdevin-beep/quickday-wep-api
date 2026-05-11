namespace Shop.Web.API.Models.Requests
{
    public class PaginationRequest
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public class GetCustomersRequest : PaginationRequest
    {
        public string? Search { get; init; }
        public string? Status { get; init; }   // "active" | "inactive" | null = all
        public string? Type { get; init; }     // "Hotel" | "Home" | null = all
        public bool? HasDue { get; init; }
    }

    public class GetSuppliersRequest : PaginationRequest
    {
        public string? Search { get; init; }
        public string? Status { get; init; }
    }

    public class GetProductsRequest : PaginationRequest
    {
        public string? Search { get; init; }
        public string? Status { get; init; }   // "active" | "inactive" | null = all
        public string? Category { get; init; }
    }

    public class GetInventoryLogsRequest : PaginationRequest
    {
        public string? From { get; init; }
        public string? To { get; init; }
        public string? Search { get; init; }
    }

    public class GetOrdersRequest : PaginationRequest
    {
        public string? From { get; init; }
        public string? To { get; init; }
    }
}