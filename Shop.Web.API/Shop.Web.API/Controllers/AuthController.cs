using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Services;
using System.Security.Claims;

namespace Shop.Web.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService auth, ILogger<AuthController> logger)
        {
            _auth = auth;
            _logger = logger;
        }

        private string ClientIp =>
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        private string ClientUserAgent =>
            Request.Headers.UserAgent.ToString();

        // ── POST api/auth/login ───────────────────────────────
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    GetFirstModelError(), "VALIDATION_ERROR"));

            var result = await _auth.LoginAsync(req, ClientIp, ClientUserAgent);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
        }

        // ── POST api/auth/register ────────────────────────────
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 409)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    GetFirstModelError(), "VALIDATION_ERROR"));

            await _auth.RegisterAsync(req, ClientIp);
            return Ok(ApiResponse<object>.Ok(null,
                "Registration successful. Please log in."));
        }

        // ── POST api/auth/refresh ─────────────────────────────
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    GetFirstModelError(), "VALIDATION_ERROR"));

            var result = await _auth.RefreshAsync(req.RefreshToken, ClientIp);
            return Ok(ApiResponse<AuthResponse>.Ok(result));
        }

        // ── POST api/auth/logout ──────────────────────────────
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Logout()
        {
            int userId = int.Parse(
                User.FindFirstValue("userId")
                ?? throw new InvalidOperationException("userId claim missing."));

            await _auth.LogoutAsync(userId, ClientIp);
            return Ok(ApiResponse<object>.Ok(null, "Logged out successfully."));
        }

        // ── GET api/auth/me ───────────────────────────────────
        // Useful for Angular to re-hydrate state after a page refresh
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult Me()
        {
            var claims = new
            {
                UserId = User.FindFirstValue("userId"),
                BusinessAccountId = User.FindFirstValue("businessAccountId"),
                Role = User.FindFirstValue(ClaimTypes.Role),
            };
            return Ok(ApiResponse<object>.Ok(claims));
        }

        private string GetFirstModelError() =>
            ModelState.Values
                      .SelectMany(v => v.Errors)
                      .Select(e => e.ErrorMessage)
                      .FirstOrDefault() ?? "Invalid request.";
    }
}
