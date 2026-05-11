using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Services;
using System.Security.Claims;

namespace Shop.Web.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _svc;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService svc, ILogger<ProductsController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private int BusinessAccountId => int.Parse(User.FindFirstValue("businessAccountId")!);
        private string ClientIp => HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                                            ?? HttpContext.Connection.RemoteIpAddress?.ToString()
                                            ?? "unknown";

        private string FirstModelError() =>
            ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage).FirstOrDefault() ?? "Invalid request.";

        // GET api/products?activeOnly=true&category=Water+Cans
        //[HttpGet]
        //public async Task<IActionResult> GetAll(
        //    [FromQuery] bool? activeOnly = null,
        //    [FromQuery] string? category = null)
        //{
        //    var list = await _svc.GetAllAsync(BusinessAccountId, UserId, activeOnly, category);
        //    return Ok(ApiResponse<List<ProductDto>>.Ok(list));
        //}

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] bool? activeOnly = null,
            [FromQuery] string? category = null,
            [FromQuery] string? search = null,
            [FromQuery] bool? lowStockOnly = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var paged = await _svc.GetAllAsync(
                BusinessAccountId, UserId,
                activeOnly, category, search, lowStockOnly, page, pageSize);
            return Ok(ApiResponse<PagedResponse<ProductDto>>.Ok(paged));
        }

        // GET api/products/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _svc.GetSummaryAsync(BusinessAccountId);
            return Ok(ApiResponse<ProductSummaryDto>.Ok(summary));
        }

        // GET api/products/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _svc.GetByIdAsync(id, BusinessAccountId);
            return Ok(ApiResponse<ProductDto>.Ok(dto));
        }

        // POST api/products
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var created = await _svc.CreateAsync(BusinessAccountId, UserId, req, ClientIp);
            return Ok(ApiResponse<ProductDto>.Ok(created, $"Product '{created.Name}' created successfully."));
        }

        // PUT api/products/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var updated = await _svc.UpdateAsync(id, BusinessAccountId, UserId, req, ClientIp);
            return Ok(ApiResponse<ProductDto>.Ok(updated, $"Product '{updated.Name}' updated successfully."));
        }

        // PATCH api/products/{id}/toggle-status
        [HttpPatch("{id:int}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var updated = await _svc.ToggleStatusAsync(id, BusinessAccountId, UserId, ClientIp);
            var action = updated.Active ? "activated" : "deactivated";
            return Ok(ApiResponse<ProductDto>.Ok(updated, $"Product '{updated.Name}' {action}."));
        }
    }

}
