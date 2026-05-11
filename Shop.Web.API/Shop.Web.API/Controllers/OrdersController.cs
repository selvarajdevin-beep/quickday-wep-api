using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Services;
using System.Security.Claims;

namespace Shop.Web.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _svc;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService svc, ILogger<OrdersController> logger)
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

        // GET api/orders?from=&to=
        //[HttpGet]
        //public async Task<IActionResult> GetAll(
        //    [FromQuery] DateTime? from = null,
        //    [FromQuery] DateTime? to = null)
        //{
        //    var list = await _svc.GetAllAsync(BusinessAccountId, from, to);
        //    return Ok(ApiResponse<List<OrderDto>>.Ok(list));
        //}

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] string? status = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var paged = await _svc.GetAllAsync(BusinessAccountId, from, to, page, pageSize, status, search);
            return Ok(ApiResponse<PagedResponse<OrderDto>>.Ok(paged));
        }

        // GET api/orders/today-summary
        [HttpGet("today-summary")]
        public async Task<IActionResult> GetTodaySummary()
        {
            var summary = await _svc.GetTodaySummaryAsync(BusinessAccountId);
            return Ok(ApiResponse<TodaySummaryDto>.Ok(summary));
        }

        // GET api/orders/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _svc.GetByIdAsync(id, BusinessAccountId);
            return Ok(ApiResponse<OrderDto>.Ok(dto));
        }

        // GET api/orders/by-customer/{customerId}
        //[HttpGet("by-customer/{customerId:int}")]
        //public async Task<IActionResult> GetByCustomer(int customerId)
        //{
        //    var list = await _svc.GetByCustomerAsync(customerId, BusinessAccountId);
        //    return Ok(ApiResponse<List<OrderDto>>.Ok(list));
        //}

        [HttpGet("by-customer/{customerId:int}")]
        public async Task<IActionResult> GetByCustomer(
            int customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var paged = await _svc.GetByCustomerAsync(customerId, BusinessAccountId, page, pageSize);
            return Ok(ApiResponse<PagedResponse<OrderDto>>.Ok(paged));
        }

        [HttpGet("by-customer/{customerId:int}/history")]
        public async Task<IActionResult> GetByCustomerFiltered(
            int customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] string? dateFrom = null,
            [FromQuery] string? dateTo = null,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null)
        {
            var result = await _svc.GetByCustomerFilteredAsync(
                customerId, BusinessAccountId,
                page, pageSize,
                dateFrom, dateTo, search, status);

            return Ok(ApiResponse<PagedOrderHistoryResponse>.Ok(result));
        }

        // POST api/orders
        // Workers can create orders (Billing is a Worker-accessible module)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var created = await _svc.CreateAsync(BusinessAccountId, UserId, req, ClientIp);
            return Ok(ApiResponse<OrderDto>.Ok(created, $"Order #{created.Id} saved successfully."));
        }

        // PUT api/orders/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var updated = await _svc.UpdateAsync(id, BusinessAccountId, UserId, req, ClientIp);
            return Ok(ApiResponse<OrderDto>.Ok(updated, $"Order #{updated.Id} updated."));
        }

        // GET api/orders/payments/by-customer/{customerId}
        //[HttpGet("payments/by-customer/{customerId:int}")]
        //public async Task<IActionResult> GetPaymentsByCustomer(int customerId)
        //{
        //    var list = await _svc.GetPaymentsByCustomerAsync(customerId, BusinessAccountId);
        //    return Ok(ApiResponse<List<PaymentDto>>.Ok(list));
        //}

        [HttpGet("payments/by-customer/{customerId:int}")]
        public async Task<IActionResult> GetPaymentsByCustomer(
            int customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var paged = await _svc.GetPaymentsByCustomerAsync(customerId, BusinessAccountId, page, pageSize);
            return Ok(ApiResponse<PagedResponse<PaymentDto>>.Ok(paged));
        }

        // POST api/orders/payments/{customerId}
        [HttpPost("payments/{customerId:int}")]
        public async Task<IActionResult> RecordPayment(
            int customerId, [FromBody] RecordPaymentRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var paymentId = await _svc.RecordPaymentAsync(
                BusinessAccountId, UserId, customerId, req, ClientIp);

            return Ok(ApiResponse<object>.Ok(
                new { paymentId },
                $"Payment of ₹{req.Amount} recorded successfully."));
        }

        // DELETE api/orders/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _svc.SoftDeleteAsync(id, BusinessAccountId, UserId, ClientIp);
            return Ok(ApiResponse<object>.Ok(null, $"Order #{id} deleted."));
        }

        // GET api/orders/payments/by-order/{orderId}
        [HttpGet("payments/by-order/{orderId:int}")]
        public async Task<IActionResult> GetPaymentsByOrder(int orderId)
        {
            var list = await _svc.GetPaymentsByOrderAsync(orderId, BusinessAccountId);
            return Ok(ApiResponse<List<PaymentDto>>.Ok(list));
        }

        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary([FromQuery] DateTime? from = null)
        {
            var result = await _svc.GetDashboardSummaryAsync(BusinessAccountId, from);
            return Ok(ApiResponse<OrderDashboardSummaryDto>.Ok(result));
        }
    }
}
