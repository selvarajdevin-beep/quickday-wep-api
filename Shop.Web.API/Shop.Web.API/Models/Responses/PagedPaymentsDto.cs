//namespace Shop.Web.API.Models.Responses
//{
//    public sealed class PagedPaymentsDto
//    {
//        public List<PaymentHistoryItemDto> Items { get; init; } = [];
//        public int TotalCount { get; init; }
//        public int Page { get; init; }
//        public int PageSize { get; init; }
//    }
//}

namespace Shop.Web.API.Models.Responses
{
    public sealed class PagedPaymentsDto
    {
        public List<PaymentHistoryItemDto> Items { get; init; } = [];
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
    }
}