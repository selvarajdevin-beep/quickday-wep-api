using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;

namespace Shop.Web.API.Services
{
    public interface IInventoryService
    {
        //Task<List<InventoryLogDto>> GetLogsAsync(int businessAccountId, DateOnly? from, DateOnly? to);

        Task<PagedResponse<InventoryLogDto>> GetLogsAsync(
            int businessAccountId,
            DateOnly? from, DateOnly? to, string? search,
            int page, int pageSize); 
        
        Task<ProductDto> AdjustStockAsync(int productId, int businessAccountId, int requestingUserId, AdjustStockRequest req, string ip);
        Task<ProductDto> UpdateMinStockAlertAsync(int productId, int businessAccountId, int requestingUserId, int minStock, string ip);

    }
}
