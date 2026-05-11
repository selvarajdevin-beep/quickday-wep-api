using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;

namespace Shop.Web.API.Repositories
{
    public interface IReportRepository
    {
        Task<(int TotalCount, List<CustomerReportRow> Items)> GetCustomerWiseAsync(
            int businessAccountId, CustomerReportParams p);

        Task<(PurchaseReportGlobalSummary GlobalSummary, int TotalCount, List<PurchaseReportRow> Items)> GetPurchaseWiseAsync(int businessAccountId, PurchaseReportParams p);

    }
}
