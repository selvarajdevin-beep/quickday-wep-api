using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Repositories;
using System.Text.Json;

namespace Shop.Web.API.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _repo;
        private readonly ILogger<PurchaseService> _logger;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public PurchaseService(IPurchaseRepository repo, ILogger<PurchaseService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        //public async Task<List<PurchaseDto>> GetAllAsync(
        //    int businessAccountId, int requestingUserId,
        //    DateOnly? from = null, DateOnly? to = null)
        //{
        //    var records = await _repo.GetAllAsync(businessAccountId, requestingUserId, from, to);
        //    return records.Select(MapToDto).ToList();
        //}

        public async Task<PagedResponse<PurchaseDto>> GetAllAsync(
            int businessAccountId, int requestingUserId,
            string? status, int? supplierId, string? search,
            DateTime? dateFrom, DateTime? dateTo,
            int page, int pageSize)
        {
            var (items, totalCount) = await _repo.GetAllAsync(
                businessAccountId, requestingUserId,
                status, supplierId, search,
                dateFrom, dateTo, page, pageSize);

            return new PagedResponse<PurchaseDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        public async Task<PurchaseDto> GetByIdAsync(int purchaseId, int businessAccountId)
        {
            var record = await _repo.GetByIdAsync(purchaseId, businessAccountId);
            if (record is null)
                throw new NotFoundException($"Purchase with ID {purchaseId} not found.");
            return MapToDto(record);
        }

        public async Task<PurchaseSummaryDto> GetSummaryAsync(int businessAccountId)
        {
            var record = await _repo.GetSummaryAsync(businessAccountId);
            return new PurchaseSummaryDto
            {
                TotalThisMonth = record.TotalThisMonth,
                CreditPending = record.CreditPending,
                PurchaseCount = record.PurchaseCount,
            };
        }

        public async Task<PurchaseDto> CreateAsync(
            int businessAccountId, int requestingUserId,
            CreatePurchaseRequest req, string ip)
        {
            // Service-layer validation on top of data annotations
            if (req.Items.Count == 0)
                throw new AppException("At least one item is required.", "PURCHASE_NO_ITEMS");

            if (req.Items.Any(i => i.Quantity <= 0))
                throw new AppException("All item quantities must be greater than zero.", "PURCHASE_INVALID_QTY");

            // Validate calculated totals match submitted values (prevents tampering)
            var expectedTotal = req.Items.Sum(i => Math.Round(i.PricePerUnit * i.Quantity, 2));
            if (Math.Abs(expectedTotal - req.GrandTotal) > 0.01m)
                throw new AppException("Grand total does not match item totals.", "PURCHASE_TOTAL_MISMATCH");

            var expectedBalance = req.GrandTotal - req.PaidAmount;
            if (Math.Abs(expectedBalance - req.Balance) > 0.01m)
                throw new AppException("Balance does not match grand total minus paid amount.", "PURCHASE_BALANCE_MISMATCH");

            var record = await _repo.CreateAsync(businessAccountId, requestingUserId, req, ip);
            if (record is null)
                throw new AppException("Failed to create purchase. Please try again.", "PURCHASE_UNEXPECTED");

            _logger.LogInformation(
                "Purchase created: #{Id} Supplier={Supplier} Total=₹{Total} BusinessAccountId={BizId} by UserId={UserId}",
                record.Id, req.SupplierName, req.GrandTotal, businessAccountId, requestingUserId);

            return MapToDto(record);
        }

        public async Task<PurchaseDto> UpdateAsync(
            int purchaseId, int businessAccountId, int requestingUserId,
            UpdatePurchaseRequest req, string ip)
        {
            if (req.Items.Count == 0)
                throw new AppException("At least one item is required.", "PURCHASE_NO_ITEMS");

            var expectedBalance = req.GrandTotal - req.PaidAmount;
            if (Math.Abs(expectedBalance - req.Balance) > 0.01m)
                throw new AppException("Balance does not match grand total minus paid amount.", "PURCHASE_BALANCE_MISMATCH");

            var record = await _repo.UpdateAsync(purchaseId, businessAccountId, requestingUserId, req, ip);
            if (record is null)
                throw new NotFoundException($"Purchase with ID {purchaseId} not found.");

            _logger.LogInformation(
                "Purchase updated: #{Id} by UserId={UserId}", purchaseId, requestingUserId);

            return MapToDto(record);
        }

        public async Task<PurchaseDto> MarkPaidAsync(
            int purchaseId, int businessAccountId, int requestingUserId, string ip)
        {
            var record = await _repo.MarkPaidAsync(purchaseId, businessAccountId, requestingUserId, ip);
            if (record is null)
                throw new NotFoundException($"Purchase with ID {purchaseId} not found.");

            _logger.LogInformation(
                "Purchase marked paid: #{Id} by UserId={UserId}", purchaseId, requestingUserId);

            return MapToDto(record);
        }

        // ── Mapping ───────────────────────────────────────────────

        private static PurchaseDto MapToDto(PurchaseRecord r)
        {
            var items = new List<PurchaseItemDto>();
            if (!string.IsNullOrWhiteSpace(r.ItemsJson))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<List<PurchaseItemDto>>(r.ItemsJson, _jsonOpts);
                    if (parsed is not null) items = parsed;
                }
                catch { /* non-fatal — return empty */ }
            }

            return new PurchaseDto
            {
                Id = r.Id,
                BusinessAccountId = r.BusinessAccountId,
                SupplierId = r.SupplierId,
                SupplierName = r.SupplierName,
                Items = items,
                GrandTotal = r.GrandTotal,
                PaidAmount = r.PaidAmount,
                Balance = r.Balance,
                PaymentStatus = r.PaymentStatus,
                Notes = r.Notes,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
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
