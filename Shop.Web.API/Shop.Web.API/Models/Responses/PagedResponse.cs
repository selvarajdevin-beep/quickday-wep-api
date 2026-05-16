// Models/Responses/PagedResponse.cs
namespace Shop.Web.API.Models.Responses
{
    public class PagedResponse<T>
    {
        public List<T> Items { get; init; } = [];
        public int TotalCount { get; init; }
        public int TotalStockUnits { get; init; }
        public int LowStockCount { get; init; }
        public decimal TotalAmount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNext => Page < TotalPages;
        public bool HasPrev => Page > 1;
    }
}