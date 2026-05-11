using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;

namespace Shop.Web.API.Repositories
{
    public interface IInventoryRepository
    {
        //Task<List<InventoryLogRecord>> GetLogsAsync(int businessAccountId, DateOnly? from, DateOnly? to);
        Task<(List<InventoryLogRecord> Items, int TotalCount)> GetLogsAsync(
            int businessAccountId,
            DateOnly? from, DateOnly? to, string? search,
            int page, int pageSize); 
        
        Task<ProductRecord?> AdjustStockAsync(int productId, int businessAccountId, int requestingUserId, AdjustStockRequest req, string ip);
        Task<ProductRecord?> UpdateMinStockAlertAsync(int productId, int businessAccountId, int requestingUserId, int minStock, string ip);
    }
}
