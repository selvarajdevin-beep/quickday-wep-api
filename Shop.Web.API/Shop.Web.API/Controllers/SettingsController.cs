// Controllers/SettingsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Services;
using System.Security.Claims;

namespace AquaERP.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]   // All settings endpoints require a valid JWT
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _svc;
    private readonly IAppConstantsService _constants;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(ISettingsService svc, ILogger<SettingsController> logger, IAppConstantsService constants)
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

    // ── GET api/settings ──────────────────────────────────────
    // Returns full business settings + both role permission sets.
    // Accessible by Admin only (Workers have no reason to view settings).

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _svc.GetSettingsAsync(BusinessAccountId, UserId);
        var perms = await _svc.GetAllRolePermissionsAsync(BusinessAccountId);

        return Ok(ApiResponse<object>.Ok(new { settings, permissions = perms }));
    }

    // ── PUT api/settings ──────────────────────────────────────
    // Updates business profile + preferences atomically.
    // Admin only.

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(
                ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .First()));

        var updated = await _svc.UpdateSettingsAsync(BusinessAccountId, UserId, req);
        return Ok(ApiResponse<SettingsDto>.Ok(updated, "Settings saved successfully."));
    }

    // ── GET api/settings/permissions ─────────────────────────
    // Returns permissions for both roles (Admin + Worker).

    [HttpGet("permissions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPermissions()
    {
        var perms = await _svc.GetAllRolePermissionsAsync(BusinessAccountId);
        return Ok(ApiResponse<AllRolePermissionsDto>.Ok(perms));
    }

    // ── PUT api/settings/permissions/{role} ───────────────────
    // Saves updated permission set for a specific role.
    // Admin only.

    [HttpPut("permissions/{role}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SavePermissions(
        string role,
        [FromBody] SaveRolePermissionsRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(
                ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .First()));

        var saved = await _svc.SaveRolePermissionsAsync(
            BusinessAccountId, UserId, role, req);

        return Ok(ApiResponse<List<PermissionDto>>.Ok(
            saved, $"Permissions saved for role '{role}'."));
    }

    // ── GET api/settings/export ───────────────────────────────
    // Returns a plain-text data export (stub — extend for real CSV/JSON).
    // Admin only.

    [HttpGet("export")]
    [Authorize(Roles = "Admin")]
    public IActionResult Export()
    {
        var fileName = $"aquaerp-backup-{DateTime.UtcNow:yyyy-MM-dd}.txt";
        var content = $"AquaERP Data Export\nGenerated: {DateTime.UtcNow:O}\nBusinessAccountId: {BusinessAccountId}";
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);

        _logger.LogInformation(
            "Data export requested by UserId={UserId} BusinessAccountId={BizId}",
            UserId, BusinessAccountId);

        return File(bytes, "text/plain", fileName);
    }

    [HttpGet("permissions/my-role")]
    [Authorize]   // ← no [Authorize(Roles = "Admin")] — open to all roles
    public async Task<IActionResult> GetMyRolePermissions()
    {
        string role = UserRole;   // "Admin" | "Worker" from JWT claim

        var result = await _svc.GetMyRolePermissionsAsync(BusinessAccountId, role);
        return Ok(ApiResponse<MyRolePermissionsDto>.Ok(result));
    }

    // ── NEW: GET /api/settings/constants ─────────────────────────────────
    /// <summary>
    /// Returns the full application constant catalogue from the AppConstants
    /// table.  No business-account-specific data is returned here.
    ///
    /// AllowAnonymous so the Register page can populate its shop-type
    /// dropdown before the user logs in.  If you want authenticated-only,
    /// replace [AllowAnonymous] with [Authorize].
    /// </summary>
    [HttpGet("constants")]
    [AllowAnonymous]
    public async Task<IActionResult> GetConstants()
    {
        var dto = await _constants.GetAllAsync();
        return Ok(ApiResponse<AppConstantsDto>.Ok(dto));
    }
}
