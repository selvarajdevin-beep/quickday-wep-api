using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;

namespace Shop.Web.API.Repositories
{
    public interface IPurchaseRepository
    {
        //Task<List<PurchaseRecord>> GetAllAsync(int businessAccountId, int requestingUserId, DateOnly? from = null, DateOnly? to = null);

        Task<(List<PurchaseRecord> Items, int TotalCount)> GetAllAsync(
            int businessAccountId, int requestingUserId,
            string? status, int? supplierId, string? search,
            DateTime? dateFrom, DateTime? dateTo,
            int page, int pageSize);

        Task<PurchaseRecord?> GetByIdAsync(int purchaseId, int businessAccountId);
        Task<PurchaseSummaryRecord> GetSummaryAsync(int businessAccountId);
        Task<PurchaseRecord?> CreateAsync(int businessAccountId, int requestingUserId, CreatePurchaseRequest req, string ip);
        Task<PurchaseRecord?> UpdateAsync(int purchaseId, int businessAccountId, int requestingUserId, UpdatePurchaseRequest req, string ip);
        Task<PurchaseRecord?> MarkPaidAsync(int purchaseId, int businessAccountId, int requestingUserId, string ip);
    }
}
