using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Services;
using System.Security.Claims;

namespace Shop.Web.API.Controllers
{ 
    [ApiController]
    [Route("api/inventory")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _svc;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IInventoryService svc, ILogger<InventoryController> logger)
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

        // GET api/inventory/logs?from=2024-01-01&to=2024-12-31
        //[HttpGet("logs")]
        //public async Task<IActionResult> GetLogs(
        //    [FromQuery] DateOnly? from = null,
        //    [FromQuery] DateOnly? to = null)
        //{
        //    var logs = await _svc.GetLogsAsync(BusinessAccountId, from, to);
        //    return Ok(ApiResponse<List<InventoryLogDto>>.Ok(logs));
        //}

        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs(
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var paged = await _svc.GetLogsAsync(BusinessAccountId, from, to, search, page, pageSize);
            return Ok(ApiResponse<PagedResponse<InventoryLogDto>>.Ok(paged));
        }


        // POST api/inventory/{productId}/adjust
        [HttpPost("{productId:int}/adjust")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdjustStock(
            int productId, [FromBody] AdjustStockRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var updated = await _svc.AdjustStockAsync(
                productId, BusinessAccountId, UserId, req, ClientIp);

            return Ok(ApiResponse<ProductDto>.Ok(
                updated,
                $"Stock {(req.Type == "IN" ? "added" : "removed")} successfully."));
        }

        // PATCH api/inventory/{productId}/min-stock
        [HttpPatch("{productId:int}/min-stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMinStock(
            int productId, [FromBody] UpdateMinStockAlertRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var updated = await _svc.UpdateMinStockAlertAsync(
                productId, BusinessAccountId, UserId, req.MinStockAlert, ClientIp);

            return Ok(ApiResponse<ProductDto>.Ok(updated, "Alert threshold updated successfully."));
        }
    }
}
