using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;

namespace Shop.Web.API.Repositories
{
    public interface IProductRepository
    {
        //Task<List<ProductRecord>> GetAllAsync(int businessAccountId, int requestingUserId, bool? activeOnly = null, string? category = null);

        // IProductRepo.cs
        //Task<(List<ProductRecord> Items, int TotalCount)> GetAllAsync(
        //    int businessAccountId, int requestingUserId,
        //    bool? activeOnly, string? category, string? search, bool? lowStockOnly,
        //    int page, int pageSize);

        Task<(
            List<ProductRecord> Items,
            int TotalCount,
            int TotalStockUnits,
            int LowStockCount
        )> GetAllAsync(
            int businessAccountId,
            int requestingUserId,
            bool? activeOnly,
            string? category,
            string? search,
            bool? lowStockOnly,
            int page,
            int pageSize);

        Task<ProductRecord?> GetByIdAsync(int productId, int businessAccountId);
        Task<ProductSummaryRecord> GetSummaryAsync(int businessAccountId);
        Task<ProductRecord?> CreateAsync(int businessAccountId, int requestingUserId, CreateProductRequest req, string ip);
        Task<ProductRecord?> UpdateAsync(int productId, int businessAccountId, int requestingUserId, UpdateProductRequest req, string ip);
        Task<ProductRecord?> ToggleStatusAsync(int productId, int businessAccountId, int requestingUserId, string ip);
    }
}
