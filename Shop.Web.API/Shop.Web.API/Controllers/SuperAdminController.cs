namespace Shop.Web.API.Controllers
{
    // Controllers/SuperAdminController.cs
    // ─────────────────────────────────────────────────────────────────────────────
    // All endpoints are restricted to users with the "SuperAdmin" claim.
    // The policy is registered in Program.cs (see snippet at end of this file).
    // ─────────────────────────────────────────────────────────────────────────────
    using AquaERP.Api.Controllers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Shop.Web.API.Models.Requests;
    using Shop.Web.API.Models.Responses;
    using Shop.Web.API.Services;
    using System.Security.Claims;


    [ApiController]
    [Route("api/superadmin")]
    [Authorize(Policy = "SuperAdminOnly")]   // ← custom policy — only IsSuperAdmin=true users
    public sealed class SuperAdminController : ControllerBase
    {
        private readonly ISuperAdminService _svc;
        private readonly IAppConstantsService _constants;
        private readonly ILogger<SuperAdminController> _logger;

        public SuperAdminController(ISuperAdminService svc, ILogger<SuperAdminController> logger, IAppConstantsService constants)
        {
            _svc = svc;
            _logger = logger;
            _constants = constants;

        }

        // ── Claim helpers ─────────────────────────────────────────

        private int UserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? throw new UnauthorizedAccessException());

        private int BusinessAccountId =>
            int.Parse(User.FindFirstValue("businessAccountId")
                      ?? throw new UnauthorizedAccessException());

        private string UserRole =>
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;



        // ── GET /api/superadmin/dashboard ─────────────────────────────────────
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var dto = await _svc.GetDashboardAsync();
            return Ok(ApiResponse<SuperAdminDashboardDto>.Ok(dto));
        }

        // ── GET /api/superadmin/shops?page=&pageSize=&search=&plan=&status= ──
        [HttpGet("shops")]
        public async Task<IActionResult> GetShops(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? plan = null,
            [FromQuery] string? status = null)
        {
            var dto = await _svc.GetShopsAsync(page, pageSize, search, plan, status);
            return Ok(ApiResponse<PagedShopsDto>.Ok(dto));
        }

        // ── GET /api/superadmin/shops/{id} ────────────────────────────────────
        [HttpGet("shops/{id:int}")]
        public async Task<IActionResult> GetShop(int id)
        {
            var dto = await _svc.GetShopByIdAsync(id);
            return Ok(ApiResponse<ShopDetailDto>.Ok(dto));
        }

        // ── PUT /api/superadmin/shops/{id}/subscription ───────────────────────
        [HttpPut("shops/{id:int}/subscription")]
        public async Task<IActionResult> UpdateSubscription(
            int id, [FromBody] UpdateSubscriptionRequest req)
        {
            var dto = await _svc.UpdateSubscriptionAsync(id, req, UserId);
            return Ok(ApiResponse<ShopDetailDto>.Ok(dto));
        }

        // ── PATCH /api/superadmin/shops/{id}/toggle-status ───────────────────
        [HttpPatch("shops/{id:int}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var dto = await _svc.ToggleShopStatusAsync(id, UserId);
            return Ok(ApiResponse<ShopDetailDto>.Ok(dto));
        }

        // ── GET /api/superadmin/payments ──────────────────────────────────────────
        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? businessAccountId = null,
            [FromQuery] string? plan = null,
            [FromQuery] string? status = null)
        {
            var dto = await _svc.GetPaymentsAsync(page, pageSize, businessAccountId, plan, status);
            return Ok(ApiResponse<PagedPaymentsDto>.Ok(dto));
        }

        // ── POST /api/superadmin/payments ─────────────────────────────────────────
        [HttpPost("payments")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest req)
        {
            var dto = await _svc.CreatePaymentAsync(req, UserId);
            return Ok(ApiResponse<PaymentHistoryItemDto>.Ok(dto));
        }

        // ── PUT /api/superadmin/payments/{id} ─────────────────────────────────────
        [HttpPut("payments/{id:int}")]
        public async Task<IActionResult> UpdatePayment(
            int id, [FromBody] UpdatePaymentRequest req)
        {
            var dto = await _svc.UpdatePaymentAsync(id, req, UserId);
            return Ok(ApiResponse<PaymentHistoryItemDto>.Ok(dto));
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueStats()
        {
            var dto = await _svc.GetRevenueStatsAsync();
            return Ok(ApiResponse<RevenueStatsDto>.Ok(dto));
        }

    }

    /*
     * ── Program.cs additions ──────────────────────────────────────────────────────
     * Add these in your builder.Services block (BEFORE builder.Build()):
     *
     * // 1. SuperAdmin authorization policy
     * builder.Services.AddAuthorization(options =>
     * {
     *     options.AddPolicy("SuperAdminOnly", policy =>
     *         policy.RequireAuthenticatedUser()
     *               .RequireClaim("isSuperAdmin", "true"));
     * });
     *
     * // 2. DI registrations
     * builder.Services.AddScoped<ISuperAdminRepository, SuperAdminRepository>();
     * builder.Services.AddScoped<ISuperAdminService,    SuperAdminService>();
     *
     * ── JWT claim — add this when building the token in AuthService.cs ────────────
     * new Claim("isSuperAdmin", user.IsSuperAdmin ? "true" : "false"),
     */
}
