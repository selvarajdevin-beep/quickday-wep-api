using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;

namespace Shop.Web.API.Services
{
    public interface IReportService
    {
        Task<PagedResponse<CustomerReportRow>> GetCustomerWiseAsync(
            int businessAccountId, CustomerReportParams p);

        Task<PagedPurchaseReportResponse> GetPurchaseWiseAsync(
            int businessAccountId, PurchaseReportParams p);
    }
}
