using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;

namespace Shop.Web.API.Repositories
{
    public interface ISupplierRepository
    {
        //Task<List<SupplierRecord>> GetAllAsync(int businessAccountId, int requestingUserId);

        Task<(List<SupplierRecord> Items, int TotalCount)> GetAllAsync(
            int businessAccountId, int requestingUserId,
            string? search, string? status,
            int page, int pageSize);

        Task<SupplierRecord?> GetByIdAsync(int supplierId, int businessAccountId);
        Task<SupplierRecord?> CreateAsync(int businessAccountId, int requestingUserId, CreateSupplierRequest req, string ip);
        Task<SupplierRecord?> UpdateAsync(int supplierId, int businessAccountId, int requestingUserId, UpdateSupplierRequest req, string ip);
        Task<SupplierRecord?> ToggleStatusAsync(int supplierId, int businessAccountId, int requestingUserId, string ip);
        Task<List<PurchaseRecord>> GetPurchasesAsync(int supplierId, int businessAccountId, int maxRows = 10);
        Task<SupplierRecord?> RecordPaymentAsync(int supplierId, int businessAccountId, int requestingUserId, decimal amount, string ip);
    }
}
