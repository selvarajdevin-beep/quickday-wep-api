using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Repositories;
using System.Text.Json;

namespace Shop.Web.API.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _repo;
        private readonly ILogger<SupplierService> _logger;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public SupplierService(ISupplierRepository repo, ILogger<SupplierService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        //public async Task<List<SupplierDto>> GetAllAsync(int businessAccountId, int requestingUserId)
        //{
        //    var records = await _repo.GetAllAsync(businessAccountId, requestingUserId);
        //    return records.Select(MapToDto).ToList();
        //}

        public async Task<PagedResponse<SupplierDto>> GetAllAsync(
            int businessAccountId, int requestingUserId,
            string? search, string? status,
            int page, int pageSize)
        {
            var (items, totalCount) = await _repo.GetAllAsync(
                businessAccountId, requestingUserId,
                search, status, page, pageSize);

            return new PagedResponse<SupplierDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        public async Task<SupplierDto> GetByIdAsync(int supplierId, int businessAccountId)
        {
            var record = await _repo.GetByIdAsync(supplierId, businessAccountId);
            if (record is null)
                throw new NotFoundException($"Supplier with ID {supplierId} not found.");
            return MapToDto(record);
        }

        public async Task<SupplierDto> CreateAsync(
            int businessAccountId, int requestingUserId,
            CreateSupplierRequest req, string ip)
        {
            var record = await _repo.CreateAsync(businessAccountId, requestingUserId, req, ip);
            if (record is null)
                throw new AppException("Failed to create supplier. Please try again.", "SUPPLIER_UNEXPECTED");

            _logger.LogInformation(
                "Supplier created: {Name} BusinessAccountId={BizId} by UserId={UserId}",
                req.Name, businessAccountId, requestingUserId);

            return MapToDto(record);
        }

        public async Task<SupplierDto> UpdateAsync(
            int supplierId, int businessAccountId, int requestingUserId,
            UpdateSupplierRequest req, string ip)
        {
            var record = await _repo.UpdateAsync(supplierId, businessAccountId, requestingUserId, req, ip);
            if (record is null)
                throw new NotFoundException($"Supplier with ID {supplierId} not found.");

            _logger.LogInformation(
                "Supplier updated: SupplierId={Id} by UserId={UserId}", supplierId, requestingUserId);

            return MapToDto(record);
        }

        public async Task<SupplierDto> ToggleStatusAsync(
            int supplierId, int businessAccountId, int requestingUserId, string ip)
        {
            var record = await _repo.ToggleStatusAsync(supplierId, businessAccountId, requestingUserId, ip);
            if (record is null)
                throw new NotFoundException($"Supplier with ID {supplierId} not found.");

            _logger.LogInformation(
                "Supplier status toggled: SupplierId={Id} → {Status} by UserId={UserId}",
                supplierId, record.Active ? "Active" : "Inactive", requestingUserId);

            return MapToDto(record);
        }

        public async Task<List<PurchaseDto>> GetPurchasesAsync(
            int supplierId, int businessAccountId, int maxRows = 10)
        {
            var records = await _repo.GetPurchasesAsync(supplierId, businessAccountId, maxRows);
            return records.Select(r => MapPurchaseToDto(r)).ToList();
        }

        public async Task<SupplierDto> RecordPaymentAsync(
            int supplierId, int businessAccountId, int requestingUserId,
            decimal amount, string ip)
        {
            var record = await _repo.RecordPaymentAsync(
                supplierId, businessAccountId, requestingUserId, amount, ip);

            if (record is null)
                throw new AppException("Failed to record payment. Please try again.", "SUPPLIER_UNEXPECTED");

            _logger.LogInformation(
                "Supplier payment recorded: SupplierId={Id} Amount={Amount} by UserId={UserId}",
                supplierId, amount, requestingUserId);

            return MapToDto(record);
        }

        // ── Mappers ───────────────────────────────────────────────

        private static SupplierDto MapToDto(SupplierRecord r) => new()
        {
            Id = r.Id,
            BusinessAccountId = r.BusinessAccountId,
            Name = r.Name,
            Phone = r.Phone,
            Email = r.Email ?? string.Empty,
            Address = r.Address ?? string.Empty,
            GSTIN = r.GSTIN ?? string.Empty,
            ContactPerson = r.ContactPerson ?? string.Empty,
            Notes = r.Notes ?? string.Empty,
            Active = r.Active,
            CreatedAt = r.CreatedAt,
            TotalPurchases = r.TotalPurchases,
            TotalAmount = r.TotalAmount,
            AmountDue = r.AmountDue,
            LastPurchaseDate = r.LastPurchaseDate,
            //RowVersion = HexToBase64(r.RowVersion),
            RowVersion = r.RowVersion is not null
                                ? Convert.ToBase64String(r.RowVersion)
                                : string.Empty
        };

        private static PurchaseDto MapPurchaseToDto(PurchaseRecord r)
        {
            var items = new List<PurchaseItemDto>();
            if (!string.IsNullOrWhiteSpace(r.ItemsJson))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<List<PurchaseItemDto>>(
                        r.ItemsJson, _jsonOpts);
                    if (parsed is not null) items = parsed;
                }
                catch { /* non-fatal — return empty */ }
            }

            return new PurchaseDto
            {
                Id = r.Id,
                SupplierId = r.SupplierId,
                SupplierName = r.SupplierName,
                Items = items,
                GrandTotal = r.GrandTotal,
                PaidAmount = r.PaidAmount,
                Balance = r.Balance,
                PaymentStatus = r.PaymentStatus,
                Notes = r.Notes,
                CreatedAt = r.CreatedAt,
                //RowVersion = HexToBase64(r.RowVersion),
                RowVersion = r.RowVersion is not null
                                ? Convert.ToBase64String(r.RowVersion)
                                : string.Empty
            };
        }

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
