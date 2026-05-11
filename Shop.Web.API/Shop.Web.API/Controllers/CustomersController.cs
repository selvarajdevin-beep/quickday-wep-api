using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Services;
using System.Security.Claims;

namespace Shop.Web.API.Controllers
{
    [ApiController]
    [Route("api/customers")]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _svc;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomerService svc, ILogger<CustomersController> logger)
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

        // GET api/customers
        // Open to all authenticated roles — Workers need customer list for Billing
        //[HttpGet]
        //public async Task<IActionResult> GetAll()
        //{
        //    var list = await _svc.GetAllAsync(BusinessAccountId, UserId);
        //    return Ok(ApiResponse<List<CustomerDto>>.Ok(list));
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetAll(
        //    [FromQuery] int page = 1,
        //    [FromQuery] int pageSize = 10,
        //    [FromQuery] string? search = null,
        //    [FromQuery] string? status = null,
        //    [FromQuery] string? type = null,
        //    [FromQuery] bool? hasDue = null)
        //{
        //    var paged = await _svc.GetAllAsync(
        //        BusinessAccountId, UserId,
        //        page, pageSize, search, status, type, hasDue);
        //    return Ok(ApiResponse<PagedResponse<CustomerDto>>.Ok(paged));
        //}

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] string? type = null,
            [FromQuery] bool? hasDue = null)
        {
            var paged = await _svc.GetAllAsync(
                BusinessAccountId, UserId,
                page, pageSize, search, status, type, hasDue);

            // ApiResponse wraps PagedCustomerResponse which includes Summary
            return Ok(ApiResponse<PagedCustomerResponse>.Ok(paged));
        }

        // GET api/customers/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _svc.GetByIdAsync(id, BusinessAccountId);
            return Ok(ApiResponse<CustomerDto>.Ok(dto));
        }

        // POST api/customers
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var created = await _svc.CreateAsync(BusinessAccountId, UserId, req, ClientIp);
            return Ok(ApiResponse<CustomerDto>.Ok(created, "Customer created successfully."));
        }

        // PUT api/customers/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var updated = await _svc.UpdateAsync(id, BusinessAccountId, UserId, req, ClientIp);
            return Ok(ApiResponse<CustomerDto>.Ok(updated, "Customer updated successfully."));
        }

        // PATCH api/customers/{id}/toggle-status
        [HttpPatch("{id:int}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var updated = await _svc.ToggleStatusAsync(id, BusinessAccountId, UserId, ClientIp);
            return Ok(ApiResponse<CustomerDto>.Ok(
                updated,
                $"Customer {(updated.Active ? "activated" : "deactivated")} successfully."));
        }

        // GET /api/customers/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _svc.GetSummaryAsync(
                BusinessAccountId, UserId);
            return Ok(ApiResponse<CustomerSummaryDto>.Ok(summary));
        }
    }
}
