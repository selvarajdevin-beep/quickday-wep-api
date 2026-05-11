using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Repositories;

namespace Shop.Web.API.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _repo;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(IInventoryRepository repo, ILogger<InventoryService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        //public async Task<List<InventoryLogDto>> GetLogsAsync(
        //    int businessAccountId, DateOnly? from, DateOnly? to)
        //{
        //    var records = await _repo.GetLogsAsync(businessAccountId, from, to);
        //    return records.Select(MapLogToDto).ToList();
        //}

        public async Task<PagedResponse<InventoryLogDto>> GetLogsAsync(
            int businessAccountId,
            DateOnly? from, DateOnly? to, string? search,
            int page, int pageSize)
        {
            var (items, totalCount) = await _repo.GetLogsAsync(
                businessAccountId, from, to, search, page, pageSize);

            return new PagedResponse<InventoryLogDto>
            {
                Items = items.Select(MapLogToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        public async Task<ProductDto> AdjustStockAsync(
            int productId, int businessAccountId, int requestingUserId,
            AdjustStockRequest req, string ip)
        {
            var record = await _repo.AdjustStockAsync(
                productId, businessAccountId, requestingUserId, req, ip);

            if (record is null)
                throw new AppException("Failed to adjust stock. Please try again.", "INVENTORY_UNEXPECTED");

            _logger.LogInformation(
                "Stock {Type}: ProductId={Id} Qty={Qty} Reason={Reason} by UserId={UserId}",
                req.Type, productId, req.Quantity, req.Reason, requestingUserId);

            return MapToDto(record);
        }

        public async Task<ProductDto> UpdateMinStockAlertAsync(
            int productId, int businessAccountId, int requestingUserId, int minStock, string ip)
        {
            var record = await _repo.UpdateMinStockAlertAsync(
                productId, businessAccountId, requestingUserId, minStock, ip);

            if (record is null)
                throw new NotFoundException($"Product with ID {productId} not found.");

            _logger.LogInformation(
                "Min stock alert updated: ProductId={Id} → {Min} by UserId={UserId}",
                productId, minStock, requestingUserId);

            return MapToDto(record);
        }

        // ── Mappers ───────────────────────────────────────────────

        private static ProductDto MapToDto(ProductRecord r) => new()
        {
            Id = r.Id,
            BusinessAccountId = r.BusinessAccountId,
            Name = r.Name,
            UnitType = r.UnitType,
            Capacity = r.Capacity,
            Category = string.IsNullOrWhiteSpace(r.Category) ? null : r.Category,
            SellingPrice = r.SellingPrice,
            PurchasePrice = r.PurchasePrice,
            CurrentStock = r.CurrentStock,
            MinStockAlert = r.MinStockAlert,
            TotalOrders = r.TotalOrders,
            Active = r.Active,
            CreatedAt = r.CreatedAt,
            //RowVersion = HexToBase64(r.RowVersion),
            RowVersion = r.RowVersion is not null
                                ? Convert.ToBase64String(r.RowVersion)
                                : string.Empty
        };

        private static InventoryLogDto MapLogToDto(InventoryLogRecord r) => new()
        {
            Id = r.Id,
            ProductId = r.ProductId,
            ProductName = r.ProductName,
            Type = r.Type,
            Quantity = r.Quantity,
            Reason = r.Reason,
            Reference = string.IsNullOrWhiteSpace(r.Reference) ? null : r.Reference,
            Date = r.Date,
        };

        private static string HexToBase64(string? hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return string.Empty;
            try
            {
                var h = hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? hex[2..] : hex;
                return Convert.ToBase64String(Convert.FromHexString(h));
            }
            catch { return string.Empty; }
        }
    }
}
