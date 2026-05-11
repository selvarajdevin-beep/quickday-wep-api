using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Services;
using System.Security.Claims;

namespace Shop.Web.API.Controllers
{
    [ApiController]
    [Route("api/purchases")]
    [Authorize]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseService _svc;
        private readonly ILogger<PurchasesController> _logger;

        public PurchasesController(IPurchaseService svc, ILogger<PurchasesController> logger)
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

        // GET api/purchases?from=2024-01-01&to=2024-12-31
        //[HttpGet]
        //public async Task<IActionResult> GetAll(
        //    [FromQuery] DateOnly? from = null,
        //    [FromQuery] DateOnly? to = null)
        //{
        //    var list = await _svc.GetAllAsync(BusinessAccountId, UserId, from, to);
        //    return Ok(ApiResponse<List<PurchaseDto>>.Ok(list));
        //}

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status = null,
            [FromQuery] int? supplierId = null,
            [FromQuery] string? search = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var paged = await _svc.GetAllAsync(
                BusinessAccountId, UserId,
                status, supplierId, search,
                dateFrom, dateTo, page, pageSize);
            return Ok(ApiResponse<PagedResponse<PurchaseDto>>.Ok(paged));
        }


        // GET api/purchases/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _svc.GetSummaryAsync(BusinessAccountId);
            return Ok(ApiResponse<PurchaseSummaryDto>.Ok(summary));
        }

        // GET api/purchases/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _svc.GetByIdAsync(id, BusinessAccountId);
            return Ok(ApiResponse<PurchaseDto>.Ok(dto));
        }

        // POST api/purchases
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var created = await _svc.CreateAsync(BusinessAccountId, UserId, req, ClientIp);
            return Ok(ApiResponse<PurchaseDto>.Ok(created, $"Purchase #{created.Id} recorded successfully."));
        }

        // PUT api/purchases/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var updated = await _svc.UpdateAsync(id, BusinessAccountId, UserId, req, ClientIp);
            return Ok(ApiResponse<PurchaseDto>.Ok(updated, $"Purchase #{updated.Id} updated successfully."));
        }

        // PATCH api/purchases/{id}/mark-paid
        [HttpPatch("{id:int}/mark-paid")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var updated = await _svc.MarkPaidAsync(id, BusinessAccountId, UserId, ClientIp);
            return Ok(ApiResponse<PurchaseDto>.Ok(updated, $"Purchase #{updated.Id} marked as Paid."));
        }
    }

}
