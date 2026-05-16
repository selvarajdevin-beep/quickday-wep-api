using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Repositories;

namespace Shop.Web.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository repo, ILogger<ProductService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        //public async Task<List<ProductDto>> GetAllAsync(
        //    int businessAccountId, int requestingUserId,
        //    bool? activeOnly = null, string? category = null)
        //{
        //    var records = await _repo.GetAllAsync(businessAccountId, requestingUserId, activeOnly, category);
        //    return records.Select(MapToDto).ToList();
        //}

        public async Task<PagedResponse<ProductDto>> GetAllAsync(
            int businessAccountId, int requestingUserId,
            bool? activeOnly, string? category, string? search, bool? lowStockOnly,
            int page, int pageSize)
        {
            var (items, totalCount, totalStockUnits, lowStockCount) = await _repo.GetAllAsync(
                businessAccountId, requestingUserId,
                activeOnly, category, search, lowStockOnly, page, pageSize);

            return new PagedResponse<ProductDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                TotalStockUnits = totalStockUnits,
                LowStockCount = lowStockCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        public async Task<ProductDto> GetByIdAsync(int productId, int businessAccountId)
        {
            var record = await _repo.GetByIdAsync(productId, businessAccountId);
            if (record is null)
                throw new NotFoundException($"Product with ID {productId} not found.");
            return MapToDto(record);
        }

        public async Task<ProductSummaryDto> GetSummaryAsync(int businessAccountId)
        {
            var r = await _repo.GetSummaryAsync(businessAccountId);
            return new ProductSummaryDto
            {
                TotalProducts = r.TotalProducts,
                ActiveCount = r.ActiveCount,
                InactiveCount = r.InactiveCount,
                LowStockCount = r.LowStockCount,
                CategoryCount = r.CategoryCount,
            };
        }

        public async Task<ProductDto> CreateAsync(
            int businessAccountId, int requestingUserId,
            CreateProductRequest req, string ip)
        {
            // Service-layer validation in addition to data annotations
            if (string.IsNullOrWhiteSpace(req.Name))
                throw new AppException("Product name is required.", "PRODUCT_NO_NAME");

            if (string.IsNullOrWhiteSpace(req.UnitType))
                throw new AppException("Unit type is required.", "PRODUCT_NO_UNIT");

            if (req.SellingPrice < 0)
                throw new AppException("Selling price cannot be negative.", "PRODUCT_INVALID_PRICE");

            if (req.PurchasePrice < 0)
                throw new AppException("Purchase price cannot be negative.", "PRODUCT_INVALID_PURCHASE_PRICE");

            var record = await _repo.CreateAsync(businessAccountId, requestingUserId, req, ip);
            if (record is null)
                throw new AppException("Failed to create product. Please try again.", "PRODUCT_UNEXPECTED");

            _logger.LogInformation(
                "Product created: #{Id} Name={Name} BusinessAccountId={BizId} by UserId={UserId}",
                record.Id, req.Name, businessAccountId, requestingUserId);

            return MapToDto(record);
        }

        public async Task<ProductDto> UpdateAsync(
            int productId, int businessAccountId, int requestingUserId,
            UpdateProductRequest req, string ip)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                throw new AppException("Product name is required.", "PRODUCT_NO_NAME");

            if (string.IsNullOrWhiteSpace(req.UnitType))
                throw new AppException("Unit type is required.", "PRODUCT_NO_UNIT");

            var record = await _repo.UpdateAsync(productId, businessAccountId, requestingUserId, req, ip);
            if (record is null)
                throw new NotFoundException($"Product with ID {productId} not found.");

            _logger.LogInformation(
                "Product updated: #{Id} by UserId={UserId}", productId, requestingUserId);

            return MapToDto(record);
        }

        public async Task<ProductDto> ToggleStatusAsync(
            int productId, int businessAccountId, int requestingUserId, string ip)
        {
            var record = await _repo.ToggleStatusAsync(productId, businessAccountId, requestingUserId, ip);
            if (record is null)
                throw new NotFoundException($"Product with ID {productId} not found.");

            _logger.LogInformation(
                "Product status toggled: #{Id} Active={Active} by UserId={UserId}",
                productId, record.Active, requestingUserId);

            return MapToDto(record);
        }

        // ── Mapping ───────────────────────────────────────────────

        private static ProductDto MapToDto(ProductRecord r) => new()
        {
            Id = r.Id,
            BusinessAccountId = r.BusinessAccountId,
            Name = r.Name,
            UnitType = r.UnitType,
            Capacity = r.Capacity,
            Category = r.Category,
            SellingPrice = r.SellingPrice,
            PurchasePrice = r.PurchasePrice,
            CurrentStock = r.CurrentStock,
            MinStockAlert = r.MinStockAlert,
            Active = r.Active,
            TotalOrders = r.TotalOrders,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            //RowVersion = HexToBase64(r.RowVersion),
            RowVersion = r.RowVersion is not null
                                ? Convert.ToBase64String(r.RowVersion)
                                : string.Empty
        };

        private static string HexToBase64(string? hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return string.Empty;
            try
            {
                var h = hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? hex[2..] : hex;
                return Convert.ToBase64String(Convert.FromHexString(h));
            }
            catch
            {
                // Log warning — returning empty string is safe; the client will
                // be unable to submit an update until the page is refreshed.
                return string.Empty;
            }
        }
    }

}
