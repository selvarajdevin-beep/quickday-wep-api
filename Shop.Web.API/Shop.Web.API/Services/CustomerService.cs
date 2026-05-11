using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Repositories;

namespace Shop.Web.API.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repo;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(ICustomerRepository repo, ILogger<CustomerService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        //public async Task<List<CustomerDto>> GetAllAsync(int businessAccountId, int requestingUserId)
        //{
        //    var records = await _repo.GetAllAsync(businessAccountId, requestingUserId);
        //    return records.Select(MapToDto).ToList();
        //}

        //public async Task<PagedResponse<CustomerDto>> GetAllAsync(
        //    int businessAccountId, int requestingUserId,
        //    int page, int pageSize,
        //    string? search, string? status, string? type, bool? hasDue)
        //{
        //    var (items, totalCount) = await _repo.GetAllAsync(
        //        businessAccountId, requestingUserId,
        //        page, pageSize, search, status, type, hasDue);

        //    return new PagedResponse<CustomerDto>
        //    {
        //        Items = items.Select(MapToDto).ToList(),
        //        TotalCount = totalCount,
        //        Page = page,
        //        PageSize = pageSize,
        //    };
        //}

        public async Task<PagedCustomerResponse> GetAllAsync(
            int businessAccountId,
            int requestingUserId,
            int page,
            int pageSize,
            string? search,
            string? status,
            string? type,
            bool? hasDue)
        {
            var (summary, filteredCount, items) = await _repo.GetAllAsync(
                businessAccountId, requestingUserId,
                page, pageSize, search, status, type, hasDue);

            return new PagedCustomerResponse
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = filteredCount,   // ← filtered count for pagination
                Page = page,
                PageSize = pageSize,
                Summary = new CustomerSummaryDto
                {
                    TotalCount = summary.TotalCount,
                    ActiveCount = summary.ActiveCount,
                    InactiveCount = summary.InactiveCount,
                    HotelCount = summary.HotelCount,
                    HomeCount = summary.HomeCount,
                    CustomersWithDue = summary.CustomersWithDue,
                    TotalDueAmount = summary.TotalDueAmount,
                },
            };
        }

        public async Task<CustomerDto> GetByIdAsync(int customerId, int businessAccountId)
        {
            var record = await _repo.GetByIdAsync(customerId, businessAccountId);
            if (record is null)
                throw new NotFoundException($"Customer with ID {customerId} not found.");
            return MapToDto(record);
        }

        public async Task<CustomerDto> CreateAsync(
            int businessAccountId, int requestingUserId,
            CreateCustomerRequest req, string ip)
        {
            var record = await _repo.CreateAsync(businessAccountId, requestingUserId, req, ip);
            if (record is null)
                throw new AppException("Failed to create customer. Please try again.", "CUSTOMER_UNEXPECTED");

            _logger.LogInformation(
                "Customer created: {Name} ({Type}) BusinessAccountId={BizId} by UserId={UserId}",
                req.Name, req.CustomerType, businessAccountId, requestingUserId);

            return MapToDto(record);
        }

        public async Task<CustomerDto> UpdateAsync(
            int customerId, int businessAccountId, int requestingUserId,
            UpdateCustomerRequest req, string ip)
        {
            var record = await _repo.UpdateAsync(customerId, businessAccountId, requestingUserId, req, ip);
            if (record is null)
                throw new NotFoundException($"Customer with ID {customerId} not found.");

            _logger.LogInformation(
                "Customer updated: CustomerId={Id} by UserId={UserId}", customerId, requestingUserId);

            return MapToDto(record);
        }

        public async Task<CustomerDto> ToggleStatusAsync(
            int customerId, int businessAccountId, int requestingUserId, string ip)
        {
            var record = await _repo.ToggleStatusAsync(customerId, businessAccountId, requestingUserId, ip);
            if (record is null)
                throw new NotFoundException($"Customer with ID {customerId} not found.");

            _logger.LogInformation(
                "Customer status toggled: CustomerId={Id} → {Status} by UserId={UserId}",
                customerId, record.Active ? "Active" : "Inactive", requestingUserId);

            return MapToDto(record);
        }

        public async Task<CustomerSummaryDto> GetSummaryAsync(int businessAccountId, int requestingUserId)
            => await _repo.GetSummaryAsync(businessAccountId, requestingUserId);

        // ── Mapper ────────────────────────────────────────────────

        private static CustomerDto MapToDto(CustomerRecord r) => new()
        {
            Id = r.Id,
            BusinessAccountId = r.BusinessAccountId,
            Name = r.Name,
            Phone = r.Phone,
            Address = r.Address,
            CustomerType = r.CustomerType,
            DefaultPricePerCan = r.DefaultPricePerCan,
            DefaultPriceProductId = r.DefaultPriceProductId,
            UsePriceFromProduct = r.UsePriceFromProduct,
            TotalOrders = r.TotalOrders,
            TotalDue = r.TotalDue,
            LastOrderDate = r.LastOrderDate,
            Active = r.Active,
            CreatedAt = r.CreatedAt,
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
            catch { return string.Empty; }
        }
    }

}
