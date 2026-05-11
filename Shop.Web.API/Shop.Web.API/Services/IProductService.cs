using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;

namespace Shop.Web.API.Services
{
    public interface IProductService
    {
        //Task<List<ProductDto>> GetAllAsync(int businessAccountId, int requestingUserId, bool? activeOnly = null, string? category = null);
        Task<PagedResponse<ProductDto>> GetAllAsync(
            int businessAccountId, int requestingUserId,
            bool? activeOnly, string? category, string? search, bool? lowStockOnly,
            int page, int pageSize);
        Task<ProductDto> GetByIdAsync(int productId, int businessAccountId);
        Task<ProductSummaryDto> GetSummaryAsync(int businessAccountId);
        Task<ProductDto> CreateAsync(int businessAccountId, int requestingUserId, CreateProductRequest req, string ip);
        Task<ProductDto> UpdateAsync(int productId, int businessAccountId, int requestingUserId, UpdateProductRequest req, string ip);
        Task<ProductDto> ToggleStatusAsync(int productId, int businessAccountId, int requestingUserId, string ip);
    }
}
