using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Services;
using System.Security.Claims;

namespace Shop.Web.API.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _svc;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService svc, ILogger<ReportsController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private int BusinessAccountId => int.Parse(User.FindFirstValue("businessAccountId")!);
        private string ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        private string FirstModelError() =>
            ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage).FirstOrDefault() ?? "Invalid request.";

        [HttpGet("customer-wise")]
        public async Task<IActionResult> GetCustomerWise(
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null,
            [FromQuery] string? search = null,
            [FromQuery] string sortBy = "totalSales",
            [FromQuery] string sortDir = "DESC",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            // Whitelist sortBy to prevent SQL injection (SP uses dynamic ORDER BY)
            var allowedSortBy = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "totalOrders", "totalSales", "totalPaid", "totalDue", "lastOrderDate" };
            if (!allowedSortBy.Contains(sortBy)) sortBy = "totalSales";

            var sortDirSafe = sortDir?.ToUpperInvariant() == "ASC" ? "ASC" : "DESC";

            var paged = await _svc.GetCustomerWiseAsync(BusinessAccountId, new CustomerReportParams
            {
                DateFrom = from,
                DateTo = to,
                Search = search,
                SortBy = sortBy,
                SortDir = sortDirSafe,
                Page = Math.Max(1, page),
                PageSize = Math.Clamp(pageSize, 1, 100),
            });

            return Ok(ApiResponse<PagedResponse<CustomerReportRow>>.Ok(paged));
        }

        // ── GET /api/reports/purchase-wise ───────────────────────────────
        // Params: from, to, search, page, pageSize
        // Returns: ApiResponse<PagedPurchaseReportResponse>
        //          (includes GlobalSummary with full-period totals)

        [HttpGet("purchase-wise")]
        public async Task<IActionResult> GetPurchaseWise(
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var paged = await _svc.GetPurchaseWiseAsync(BusinessAccountId, new PurchaseReportParams
            {
                DateFrom = from,
                DateTo = to,
                Search = search,
                Page = Math.Max(1, page),
                PageSize = Math.Clamp(pageSize, 1, 100),
            });

            return Ok(ApiResponse<PagedPurchaseReportResponse>.Ok(paged));
        }

    }
}
