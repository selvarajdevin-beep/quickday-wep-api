namespace Shop.Web.API.Models.Responses
{
    /// <summary>Mirrors the Customer interface in shared-state.interfaces.ts.</summary>
    public class CustomerDto
    {
        public int Id { get; init; }
        public int BusinessAccountId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public string CustomerType { get; init; } = "Home";
        public decimal DefaultPricePerCan { get; init; }
        public int DefaultPriceProductId { get; init; }
        public bool UsePriceFromProduct { get; init; }
        public int TotalOrders { get; init; }
        public decimal TotalDue { get; init; }
        public DateTime? LastOrderDate { get; init; }
        public bool Active { get; init; }
        public DateTime CreatedAt { get; init; }
        public string RowVersion { get; init; } = string.Empty;
    }

    /// <summary>
    /// Full-dataset KPI summary — always reflects ALL customers,
    /// not just the current page. Returned alongside paged rows.
    /// </summary>
    public class CustomerDueItemDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public string Phone { get; init; } = "";
        public decimal TotalDue { get; init; }
        public DateTime? LastOrderDate { get; init; }
    }

    public class CustomerSummaryDto
    {
        public int TotalCount { get; init; }
        public int ActiveCount { get; init; }
        public int InactiveCount { get; init; }
        public int HotelCount { get; init; }
        public int HomeCount { get; init; }
        public int CustomersWithDue { get; init; }
        public decimal TotalDueAmount { get; init; }

        /// <summary>
        /// Top 5 customers sorted by TotalDue descending.
        /// Populated only on summary-only calls (Dashboard KPI card).
        /// Empty list on paginated list calls.
        /// </summary>
        public List<CustomerDueItemDto> TopDueCustomers { get; init; } = [];
    }

    /// <summary>
    /// Paginated customer list response.
    /// Extends PagedResponse with a full-dataset summary so the Angular
    /// KPI strip can show accurate totals independent of pagination.
    /// </summary>
    public class PagedCustomerResponse : PagedResponse<CustomerDto>
    {
        /// <summary>
        /// KPI counts computed over ALL customers (unfiltered).
        /// Use this to populate the Active / Hotel / Home / Due strip.
        /// TopDueCustomers is empty here — not needed on the list page.
        /// </summary>
        public CustomerSummaryDto Summary { get; init; } = new();
    }
}
