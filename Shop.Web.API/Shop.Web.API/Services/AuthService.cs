using Microsoft.IdentityModel.Tokens;
using Shop.Web.API.Exceptions;
using Shop.Web.API.Helpers;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Shop.Web.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IAuthRepository repo,
            IConfiguration config,
            ILogger<AuthService> logger)
        {
            _repo = repo;
            _config = config;
            _logger = logger;
        }

        // ── Login ─────────────────────────────────────────────
        public async Task<AuthResponse> LoginAsync(LoginRequest req, string ip, string ua)
        {
            // 1. Fetch user + DB-level pre-checks (locked, inactive, etc.)
            var user = await _repo.GetUserForLoginAsync(req.Phone);

            if (user is null)
                throw new AppException(
                    "Invalid phone number or password.",
                    "LOGIN_INVALID");

            // 2. Verify password in API layer (BCrypt — never in SQL)
            bool passwordValid = PasswordHelper.Verify(req.Password, user.PasswordHash);

            if (!passwordValid)
            {
                // Record failure BEFORE throwing so lockout counter increments
                await _repo.RecordLoginFailureAsync(user.UserId, ip, ua);
                throw new AppException(
                    "Invalid phone number or password.",
                    "LOGIN_INVALID");
            }

            // 3. Generate short-lived JWT + long-lived refresh token
            string jwt = GenerateJwt(user);
            string refreshToken = GenerateRefreshToken();
            int refreshDays = _config.GetValue<int>("Jwt:RefreshTokenDays", 7);
            var refreshExpiry = DateTime.UtcNow.AddDays(refreshDays);

            // 4. Persist success (clears lockout, stores refresh token)
            await _repo.RecordLoginSuccessAsync(
                user.UserId, ip, ua, refreshToken, refreshExpiry);

            _logger.LogInformation(
                "Login success — UserId: {UserId}, BusinessAccountId: {BizId}, IP: {Ip}",
                user.UserId, user.BusinessAccountId, ip);

            return BuildAuthResponse(user, jwt, refreshToken);
        }

        // ── Register ──────────────────────────────────────────
        public async Task RegisterAsync(RegisterRequest req, string ip)
        {
            // Belt-and-suspenders — FluentValidation + DataAnnotations already checked these
            if (req.Password != req.ConfirmPassword)
                throw new AppException("Passwords do not match.", "REG_MISMATCH");

            // Hash password in API layer
            var (hash, salt) = PasswordHelper.Hash(req.Password);

            // Build avatar initials from username
            string initials = string.Join(string.Empty,
                req.Username
                   .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                   .Take(2)
                   .Select(w => char.ToUpper(w[0])));

            await _repo.RegisterAsync(new RegisterDbParams
            {
                BusinessName = req.BusinessName,
                OwnerName = req.OwnerName,
                BusinessPhone = req.BusinessPhone,
                BusinessEmail = req.BusinessEmail,
                Address = req.Address,
                GSTIN = req.GSTIN,
                ShopType = req.ShopType,
                Username = req.Username,
                UserPhone = req.Phone,
                UserEmail = req.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                AvatarInitials = initials,
                IpAddress = ip
            });

            _logger.LogInformation(
                "New registration — Business: {Business}, Owner: {Owner}, IP: {Ip}",
                req.BusinessName, req.OwnerName, ip);
        }

        // ── Refresh Token ─────────────────────────────────────
        public async Task<AuthResponse> RefreshAsync(string refreshToken, string ip)
        {
            string newRefreshToken = GenerateRefreshToken();
            int refreshDays = _config.GetValue<int>("Jwt:RefreshTokenDays", 7);
            var newExpiry = DateTime.UtcNow.AddDays(refreshDays);

            var user = await _repo.RefreshTokenAsync(
                refreshToken, newRefreshToken, newExpiry, ip);

            if (user is null)
                throw new UnauthorizedException("Session expired. Please log in again.");

            string jwt = GenerateJwt(user);

            _logger.LogInformation(
                "Token refreshed — UserId: {UserId}, IP: {Ip}",
                user.UserId, ip);

            return BuildAuthResponse(user, jwt, newRefreshToken);
        }

        // ── Logout ────────────────────────────────────────────
        public async Task LogoutAsync(int userId, string ip)
        {
            await _repo.LogoutAsync(userId, ip);
            _logger.LogInformation("Logout — UserId: {UserId}, IP: {Ip}", userId, ip);
        }

        // ── JWT Generation ────────────────────────────────────
        private string GenerateJwt(UserRecord user)
        {
            string secret = _config["Jwt:Secret"]
                ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            int expMin = _config.GetValue<int>("Jwt:ExpiryMinutes", 60);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,  user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new("userId",            user.UserId.ToString()),
            new("businessAccountId", user.BusinessAccountId.ToString()),
            new(ClaimTypes.Role,     user.Role),
            new Claim("isSuperAdmin", user.IsSuperAdmin ? "true" : "false"),
        };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(expMin),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        private static AuthResponse BuildAuthResponse(
            UserRecord user, string jwt, string refreshToken) =>
            new()
            {
                Token = jwt,
                RefreshToken = refreshToken,
                User = new AuthUserDto
                {
                    Id = user.UserId,
                    Name = user.Name,
                    Phone = user.Phone,
                    Email = user.Email,
                    Role = user.Role,
                    isSuperAdmin = user.IsSuperAdmin,
                    BusinessName = user.BusinessName,
                    BusinessAccountId = user.BusinessAccountId,
                    AvatarInitials = user.AvatarInitials,
                    ThemeColor = user.ThemeColor,
                    ShopType = user.ShopType,
                    Currency = user.Currency,
                    CurrencySymbol = user.CurrencySymbol,
                    SubscriptionPlan = user.SubscriptionPlan,
                    SubscriptionExpiry = user.SubscriptionExpiry,
                }
            };
    }
}
