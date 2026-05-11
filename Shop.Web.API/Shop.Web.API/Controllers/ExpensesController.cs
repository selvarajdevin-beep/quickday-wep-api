using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Services;
using System.Security.Claims;

namespace Shop.Web.API.Controllers
{
    [ApiController]
    [Route("api/expenses")]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _svc;
        private readonly ILogger<ExpensesController> _logger;

        public ExpensesController(IExpenseService svc, ILogger<ExpensesController> logger)
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

        // GET api/expenses?from=2024-01-01&to=2024-12-31
        //[HttpGet]
        //public async Task<IActionResult> GetAll(
        //    [FromQuery] DateOnly? from = null,
        //    [FromQuery] DateOnly? to = null)
        //{
        //    var list = await _svc.GetAllAsync(BusinessAccountId, from, to);
        //    return Ok(ApiResponse<List<ExpenseDto>>.Ok(list));
        //}

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? type = null,
            [FromQuery] string? search = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var paged = await _svc.GetAllAsync(
                BusinessAccountId, UserId,
                type, search,
                dateFrom, dateTo,
                page, pageSize);

            return Ok(ApiResponse<PagedResponse<ExpenseDto>>.Ok(paged));
        }


        // GET api/expenses/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _svc.GetSummaryAsync(BusinessAccountId);
            return Ok(ApiResponse<ExpenseSummaryDto>.Ok(summary));
        }

        // GET api/expenses/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _svc.GetByIdAsync(id, BusinessAccountId);
            return Ok(ApiResponse<ExpenseDto>.Ok(dto));
        }

        // POST api/expenses
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateExpenseRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var created = await _svc.CreateAsync(BusinessAccountId, UserId, req, ClientIp);
            return Ok(ApiResponse<ExpenseDto>.Ok(created, "Expense saved successfully."));
        }

        // PUT api/expenses/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var updated = await _svc.UpdateAsync(id, BusinessAccountId, UserId, req, ClientIp);
            return Ok(ApiResponse<ExpenseDto>.Ok(updated, "Expense updated successfully."));
        }

        // DELETE api/expenses/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _svc.DeleteAsync(id, BusinessAccountId, UserId, ClientIp);
            return Ok(ApiResponse<object>.Ok(null, "Expense deleted successfully."));
        }
    }

}
