using Dapper;
using Microsoft.Data.SqlClient;
using Shop.Web.API.Models.Domain;
using System.Data;

namespace Shop.Web.API.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly string _connStr;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(IConfiguration config, ILogger<AuthRepository> logger)
        {
            _connStr = config.GetConnectionString("Default")
                ?? throw new InvalidOperationException(
                    "Connection string 'Default' is not configured in appsettings.json.");
            _logger = logger;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connStr);

        // ── GetUserForLogin ───────────────────────────────────
        public async Task<UserRecord?> GetUserForLoginAsync(string phone)
        {
            using var conn = CreateConnection();

            var p = new DynamicParameters();
            p.Add("@Phone", phone, DbType.String);
            p.Add("@IpAddress", string.Empty, DbType.String);
            p.Add("@UserAgent", string.Empty, DbType.String);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            var user = await conn.QueryFirstOrDefaultAsync<UserRecord>(
                "usp_Login",
                p,
                commandType: CommandType.StoredProcedure);

            ThrowIfSpError(p, "GetUserForLogin");
            return user;
        }

        // ── Register ──────────────────────────────────────────
        public async Task RegisterAsync(RegisterDbParams data)
        {
            using var conn = CreateConnection();

            var p = new DynamicParameters();
            p.Add("@BusinessName", data.BusinessName, DbType.String);
            p.Add("@OwnerName", data.OwnerName, DbType.String);
            p.Add("@BusinessPhone", data.BusinessPhone, DbType.String);
            p.Add("@BusinessEmail", data.BusinessEmail, DbType.String);
            p.Add("@Address", data.Address, DbType.String);
            p.Add("@GSTIN", data.GSTIN, DbType.String);
            p.Add("@ShopType", data.ShopType, DbType.String);
            p.Add("@Username", data.Username, DbType.String);
            p.Add("@UserPhone", data.UserPhone, DbType.String);
            p.Add("@UserEmail", data.UserEmail, DbType.String);
            p.Add("@PasswordHash", data.PasswordHash, DbType.String);
            p.Add("@PasswordSalt", data.PasswordSalt, DbType.String);
            p.Add("@AvatarInitials", data.AvatarInitials, DbType.String);
            p.Add("@IpAddress", data.IpAddress, DbType.String);

            p.Add("@BusinessAccountId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            await conn.ExecuteAsync(
                "usp_Register",
                p,
                commandType: CommandType.StoredProcedure);

            ThrowIfSpError(p, "Register");

            _logger.LogInformation(
                "Register SP success — BusinessAccountId: {Biz}, UserId: {User}",
                p.Get<int>("@BusinessAccountId"),
                p.Get<int>("@UserId"));
        }

        // ── Login Success ─────────────────────────────────────
        public async Task RecordLoginSuccessAsync(
            int userId, string ip, string ua,
            string refreshToken, DateTime refreshExpiry)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync(
                "usp_LoginSuccess",
                new
                {
                    UserId = userId,
                    IpAddress = ip,
                    UserAgent = ua,
                    RefreshToken = refreshToken,
                    RefreshTokenExpiresAt = refreshExpiry
                },
                commandType: CommandType.StoredProcedure);
        }

        // ── Login Failure ─────────────────────────────────────
        public async Task RecordLoginFailureAsync(int userId, string ip, string ua)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync(
                "usp_LoginFailure",
                new { UserId = userId, IpAddress = ip, UserAgent = ua },
                commandType: CommandType.StoredProcedure);
        }

        // ── Logout ────────────────────────────────────────────
        public async Task LogoutAsync(int userId, string ip)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync(
                "usp_Logout",
                new { UserId = userId, IpAddress = ip },
                commandType: CommandType.StoredProcedure);
        }

        // ── Refresh Token ─────────────────────────────────────
        public async Task<UserRecord?> RefreshTokenAsync(
            string token, string newToken,
            DateTime newExpiry, string ip)
        {
            using var conn = CreateConnection();

            var p = new DynamicParameters();
            p.Add("@RefreshToken", token, DbType.String);
            p.Add("@NewRefreshToken", newToken, DbType.String);
            p.Add("@NewRefreshTokenExpiry", newExpiry, DbType.DateTime2);
            p.Add("@IpAddress", ip, DbType.String);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            var user = await conn.QueryFirstOrDefaultAsync<UserRecord>(
                "usp_RefreshToken",
                p,
                commandType: CommandType.StoredProcedure);

            ThrowIfSpError(p, "RefreshToken");
            return user;
        }

        // ── Shared helper ─────────────────────────────────────
        private void ThrowIfSpError(DynamicParameters p, string context)
        {
            int code = p.Get<int>("@ErrorCode");
            if (code == 0) return;

            string msg = p.Get<string>("@ErrorMessage") ?? "An error occurred.";
            _logger.LogWarning("SP error in {Context} — Code: {Code}, Message: {Message}",
                context, code, msg);

            throw code switch
            {
                2001 or 2002 or 2004 => new Exceptions.AppException(msg, $"SP_{code}"),
                2003 => new Exceptions.AppException(msg, $"SP_{code}"),
                2005 => new Exceptions.AppException(msg, $"SP_{code}"),
                3001 or 3002 or 3003 => new Exceptions.UnauthorizedException(msg),
                1001 or 1002 => new Exceptions.ConflictException(msg),
                _ => new Exceptions.AppException(msg, $"SP_{code}")
            };
        }
    }
}
