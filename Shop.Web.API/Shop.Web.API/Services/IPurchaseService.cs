using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;

namespace Shop.Web.API.Services
{
    public interface IPurchaseService
    {
        //Task<List<PurchaseDto>> GetAllAsync(int businessAccountId, int requestingUserId, DateOnly? from = null, DateOnly? to = null);

        Task<PagedResponse<PurchaseDto>> GetAllAsync(
            int businessAccountId, int requestingUserId,
            string? status, int? supplierId, string? search,
            DateTime? dateFrom, DateTime? dateTo,
            int page, int pageSize);

        Task<PurchaseDto> GetByIdAsync(int purchaseId, int businessAccountId);
        Task<PurchaseSummaryDto> GetSummaryAsync(int businessAccountId);
        Task<PurchaseDto> CreateAsync(int businessAccountId, int requestingUserId, CreatePurchaseRequest req, string ip);
        Task<PurchaseDto> UpdateAsync(int purchaseId, int businessAccountId, int requestingUserId, UpdatePurchaseRequest req, string ip);
        Task<PurchaseDto> MarkPaidAsync(int purchaseId, int businessAccountId, int requestingUserId, string ip);
    }
}
