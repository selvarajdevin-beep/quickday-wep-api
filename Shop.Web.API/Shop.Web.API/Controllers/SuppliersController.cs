using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Services;
using System.Security.Claims;

namespace Shop.Web.API.Controllers
{
    [ApiController]
    [Route("api/suppliers")]
    [Authorize]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _svc;
        private readonly ILogger<SuppliersController> _logger;

        public SuppliersController(ISupplierService svc, ILogger<SuppliersController> logger)
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

        // GET api/suppliers
        //[HttpGet]
        //public async Task<IActionResult> GetAll()
        //{
        //    var list = await _svc.GetAllAsync(BusinessAccountId, UserId);
        //    return Ok(ApiResponse<List<SupplierDto>>.Ok(list));
        //}

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var paged = await _svc.GetAllAsync(
                BusinessAccountId, UserId,
                search, status, page, pageSize);
            return Ok(ApiResponse<PagedResponse<SupplierDto>>.Ok(paged));
        }

        // GET api/suppliers/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _svc.GetByIdAsync(id, BusinessAccountId);
            return Ok(ApiResponse<SupplierDto>.Ok(dto));
        }

        // POST api/suppliers
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSupplierRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var created = await _svc.CreateAsync(BusinessAccountId, UserId, req, ClientIp);
            return Ok(ApiResponse<SupplierDto>.Ok(created, "Supplier created successfully."));
        }

        // PUT api/suppliers/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var updated = await _svc.UpdateAsync(id, BusinessAccountId, UserId, req, ClientIp);
            return Ok(ApiResponse<SupplierDto>.Ok(updated, "Supplier updated successfully."));
        }

        // PATCH api/suppliers/{id}/toggle-status
        [HttpPatch("{id:int}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var updated = await _svc.ToggleStatusAsync(id, BusinessAccountId, UserId, ClientIp);
            return Ok(ApiResponse<SupplierDto>.Ok(
                updated,
                $"Supplier {(updated.Active ? "activated" : "deactivated")} successfully."));
        }

        // GET api/suppliers/{id}/purchases?maxRows=10
        [HttpGet("{id:int}/purchases")]
        public async Task<IActionResult> GetPurchases(int id, [FromQuery] int maxRows = 10)
        {
            var purchases = await _svc.GetPurchasesAsync(id, BusinessAccountId, maxRows);
            return Ok(ApiResponse<List<PurchaseDto>>.Ok(purchases));
        }

        // POST api/suppliers/{id}/payment
        [HttpPost("{id:int}/payment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RecordPayment(
            int id, [FromBody] RecordSupplierPaymentRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var updated = await _svc.RecordPaymentAsync(
                id, BusinessAccountId, UserId, req.Amount, ClientIp);

            return Ok(ApiResponse<SupplierDto>.Ok(
                updated, $"Payment of ₹{req.Amount:N2} recorded successfully."));
        }
    }

}
