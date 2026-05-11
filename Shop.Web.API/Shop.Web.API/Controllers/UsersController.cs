// Controllers/UsersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Web.Api.Models.Requests;
using Shop.Web.Api.Models.Responses;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Services;
using System.Security.Claims;

namespace AquaERP.Api.Controllers
{

    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "Admin")]   // All user-management endpoints are Admin-only
    public class UsersController : ControllerBase
    {
        private readonly IUserService _svc;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService svc, ILogger<UsersController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        // ── Claim helpers ─────────────────────────────────────────

        private int UserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? throw new UnauthorizedAccessException("UserId claim missing."));

        private int BusinessAccountId =>
            int.Parse(User.FindFirstValue("businessAccountId")
                      ?? throw new UnauthorizedAccessException("BusinessAccountId claim missing."));

        private string ClientIp =>
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // ── GET api/users ─────────────────────────────────────────
        // Returns all non-deleted users for the business account.

        //[HttpGet]
        //public async Task<IActionResult> GetAll()
        //{
        //    var users = await _svc.GetAllAsync(BusinessAccountId, UserId);
        //    return Ok(ApiResponse<List<UserDto>>.Ok(users));
        //}

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] string? role = null,    // ← NEW
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var paged = await _svc.GetAllAsync(
                BusinessAccountId, UserId,
                search, status, role,               // ← pass role
                page, pageSize);

            return Ok(ApiResponse<PagedResponse<UserDto>>.Ok(paged));
        }

        // ── GET api/users/{id} ────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _svc.GetByIdAsync(id, BusinessAccountId);
            return Ok(ApiResponse<UserDto>.Ok(user));
        }

        // ── POST api/users ────────────────────────────────────────
        // Creates a new user under the current business account.

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var created = await _svc.CreateAsync(BusinessAccountId, UserId, req, ClientIp);
            return Ok(ApiResponse<UserDto>.Ok(created, "User created successfully."));
        }

        // ── PUT api/users/{id} ────────────────────────────────────
        // Updates profile + HR info. Does not change password.

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            var updated = await _svc.UpdateAsync(id, BusinessAccountId, UserId, req, ClientIp);
            return Ok(ApiResponse<UserDto>.Ok(updated, "User updated successfully."));
        }

        // ── PATCH api/users/{id}/toggle-status ───────────────────
        // Activates or deactivates a user. PATCH is semantically correct
        // (partial state change) vs PUT (full replacement).

        [HttpPatch("{id:int}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var updated = await _svc.ToggleStatusAsync(id, BusinessAccountId, UserId, ClientIp);
            return Ok(ApiResponse<UserDto>.Ok(
                updated,
                $"User {(updated.Status == "Active" ? "activated" : "deactivated")} successfully."));
        }

        // ── DELETE api/users/{id} ─────────────────────────────────
        // Soft-deletes the user. Returns 204 No Content on success.

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _svc.DeleteAsync(id, BusinessAccountId, UserId, ClientIp);
            return NoContent();
        }

        // ── POST api/users/{id}/reset-password ───────────────────
        // Resets a user's password. Body: { "newPassword": "..." }

        [HttpPost("{id:int}/reset-password")]
        public async Task<IActionResult> ResetPassword(
            int id, [FromBody] ResetPasswordRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(FirstModelError()));

            await _svc.ResetPasswordAsync(id, BusinessAccountId, UserId, req, ClientIp);
            return Ok(ApiResponse<object>.Ok(null, "Password reset successfully."));
        }

        // ── Helper ────────────────────────────────────────────────

        private string FirstModelError() =>
            ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Invalid request.";
    }
}