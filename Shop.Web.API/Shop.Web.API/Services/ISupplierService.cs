using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;

namespace Shop.Web.API.Services
{
    public interface ISupplierService
    {
        //Task<List<SupplierDto>> GetAllAsync(int businessAccountId, int requestingUserId);

        Task<PagedResponse<SupplierDto>> GetAllAsync(
            int businessAccountId, int requestingUserId,
            string? search, string? status,
            int page, int pageSize);

        Task<SupplierDto> GetByIdAsync(int supplierId, int businessAccountId);
        Task<SupplierDto> CreateAsync(int businessAccountId, int requestingUserId, CreateSupplierRequest req, string ip);
        Task<SupplierDto> UpdateAsync(int supplierId, int businessAccountId, int requestingUserId, UpdateSupplierRequest req, string ip);
        Task<SupplierDto> ToggleStatusAsync(int supplierId, int businessAccountId, int requestingUserId, string ip);
        Task<List<PurchaseDto>> GetPurchasesAsync(int supplierId, int businessAccountId, int maxRows = 10);
        Task<SupplierDto> RecordPaymentAsync(int supplierId, int businessAccountId, int requestingUserId, decimal amount, string ip);
    }
}
