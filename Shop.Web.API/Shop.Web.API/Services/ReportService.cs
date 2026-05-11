using AquaERP.Api.Services;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Repositories;
using System.Text.Json;

namespace Shop.Web.API.Services
{
    public class ReportService: IReportService
    {
        private readonly IReportRepository _repo;
        private readonly ILogger<ReportService> _logger;

        // JsonSerializerOptions reused across calls — camelCase matches Angular convention
        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public ReportService(IReportRepository repo, ILogger<ReportService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        // ── Customer-wise report ──────────────────────────────────────────

        public async Task<PagedResponse<CustomerReportRow>> GetCustomerWiseAsync(
            int businessAccountId, CustomerReportParams p)
        {
            var (totalCount, items) = await _repo.GetCustomerWiseAsync(businessAccountId, p);

            return new PagedResponse<CustomerReportRow>
            {
                Items = items,
                TotalCount = totalCount,
                Page = p.Page,
                PageSize = p.PageSize,
            };
        }

        // ── Purchase-wise report ──────────────────────────────────────────

        public async Task<PagedPurchaseReportResponse> GetPurchaseWiseAsync(
            int businessAccountId, PurchaseReportParams p)
        {
            var (globalSummary, totalCount, items) =
                await _repo.GetPurchaseWiseAsync(businessAccountId, p);

            return new PagedPurchaseReportResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = p.Page,
                PageSize = p.PageSize,
                GlobalSummary = globalSummary,
            };
        }

    }
}
