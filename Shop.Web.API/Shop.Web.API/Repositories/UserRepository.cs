using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Shop.Web.Api.Models.Requests;
using Shop.Web.API.Exceptions;
using Shop.Web.API.Helpers;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Repositories;

namespace Shop.Web.Api.Repositories
{

    public class UserRepository : IUserRepository
    {
        private readonly string _conn;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IConfiguration config, ILogger<UserRepository> logger)
        {
            _conn = config.GetConnectionString("Default")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
            _logger = logger;
        }

        // ── Get All ───────────────────────────────────────────────

        //public async Task<List<UserRecord>> GetAllAsync(int businessAccountId, int requestingUserId)
        //{
        //    using var db = new SqlConnection(_conn);
        //    try
        //    {
        //        var rows = await db.QueryAsync<UserRecord>(
        //            "dbo.usp_Users_GetAll",
        //            new { BusinessAccountId = businessAccountId, RequestingUserId = requestingUserId },
        //            commandType: CommandType.StoredProcedure);
        //        return rows.AsList();
        //    }
        //    catch (SqlException ex)
        //    {
        //        _logger.LogError(ex, "SQL error in GetAllAsync BusinessAccountId={Id}", businessAccountId);
        //        throw;
        //    }
        //}

        public async Task<(List<UserRecord> Items, int TotalCount)> GetAllAsync(
            int businessAccountId,
            int requestingUserId,
            string? search,
            string? status,
            string? role,           // ← NEW
            int page,
            int pageSize)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                using var multi = await db.QueryMultipleAsync(
                    "dbo.usp_Users_GetAll",
                    new
                    {
                        BusinessAccountId = businessAccountId,
                        RequestingUserId = requestingUserId,
                        Search = string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : search,
                        Status = string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status,
                        Role = string.IsNullOrWhiteSpace(role) ? (object)DBNull.Value : role,   // ← NEW
                        Page = page,
                        PageSize = pageSize,
                    },
                    commandType: CommandType.StoredProcedure);

                var totalCount = await multi.ReadSingleAsync<int>();
                var items = (await multi.ReadAsync<UserRecord>()).AsList();
                return (items, totalCount);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex,
                    "SQL error in Users.GetAllAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }


        // ── Get By Id ─────────────────────────────────────────────

        public async Task<UserRecord?> GetByIdAsync(int userId, int businessAccountId)
        {
            using var db = new SqlConnection(_conn);
            try
            {
                return await db.QuerySingleOrDefaultAsync<UserRecord>(
                    "dbo.usp_Users_GetById",
                    new { UserId = userId, BusinessAccountId = businessAccountId },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetByIdAsync UserId={Id}", userId);
                throw;
            }
        }

        // ── Create ────────────────────────────────────────────────

        public async Task<UserRecord?> CreateAsync(
            int businessAccountId, int requestingUserId,
            CreateUserRequest req, string ip)
        {
            using var db = new SqlConnection(_conn);

            var (hash, salt) = PasswordHelper.Hash(req.Password);
            var initials = BuildInitials(req.Username);

            DateOnly? doj = null;
            if (!string.IsNullOrWhiteSpace(req.DateOfJoining) &&
                DateOnly.TryParse(req.DateOfJoining, out var parsed))
                doj = parsed;

            var p = new DynamicParameters();
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@Username", req.Username.Trim());
            p.Add("@Phone", req.Phone.Trim());
            p.Add("@Email", req.Email?.Trim());
            p.Add("@PasswordHash", hash);
            p.Add("@PasswordSalt", salt);
            p.Add("@Role", req.Role);
            p.Add("@Designation", req.Designation?.Trim());
            p.Add("@Department", req.Department?.Trim());
            p.Add("@Address", req.Address?.Trim());
            p.Add("@EmergencyContact", req.EmergencyContact?.Trim());
            p.Add("@Notes", req.Notes?.Trim());
            p.Add("@AvatarInitials", initials);
            p.Add("@DateOfJoining", doj, DbType.Date);
            p.Add("@MonthlySalary", req.MonthlySalary ?? 0m);
            p.Add("@SalaryType", req.SalaryType ?? "Fixed");
            p.Add("@BankAccount", req.BankAccount?.Trim());
            p.Add("@BankName", req.BankName?.Trim());
            p.Add("@IFSC", req.IFSC?.Trim());
            p.Add("@IpAddress", ip);
            p.Add("@NewUserId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<UserRecord>(
                    "dbo.usp_Users_Create", p, commandType: CommandType.StoredProcedure);

                ThrowIfSpError(p, "USER");
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in CreateAsync BusinessAccountId={Id}", businessAccountId);
                throw;
            }
        }

        // ── Update ────────────────────────────────────────────────

        public async Task<UserRecord?> UpdateAsync(
            int userId, int businessAccountId, int requestingUserId,
            UpdateUserRequest req, string ip)
        {
            using var db = new SqlConnection(_conn);

            byte[] rowVersionBytes = DecodeRowVersion(req.RowVersion);

            DateOnly? doj = null;
            if (!string.IsNullOrWhiteSpace(req.DateOfJoining) &&
                DateOnly.TryParse(req.DateOfJoining, out var parsed))
                doj = parsed;

            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@Username", req.Username.Trim());
            p.Add("@Phone", req.Phone.Trim());
            p.Add("@Email", req.Email?.Trim());
            p.Add("@Role", req.Role);
            p.Add("@Designation", req.Designation?.Trim());
            p.Add("@Department", req.Department?.Trim());
            p.Add("@Address", req.Address?.Trim());
            p.Add("@EmergencyContact", req.EmergencyContact?.Trim());
            p.Add("@Notes", req.Notes?.Trim());
            p.Add("@AvatarInitials", BuildInitials(req.Username));
            p.Add("@DateOfJoining", doj, DbType.Date);
            p.Add("@MonthlySalary", req.MonthlySalary ?? 0m);
            p.Add("@SalaryType", req.SalaryType ?? "Fixed");
            p.Add("@BankAccount", req.BankAccount?.Trim());
            p.Add("@BankName", req.BankName?.Trim());
            p.Add("@IFSC", req.IFSC?.Trim());
            p.Add("@RowVersion", rowVersionBytes, DbType.Binary, size: 8);
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<UserRecord>(
                    "dbo.usp_Users_Update", p, commandType: CommandType.StoredProcedure);

                ThrowIfSpError(p, "USER", errorCode => errorCode == 4009 ? 409 : 400);
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in UpdateAsync UserId={Id}", userId);
                throw;
            }
        }

        // ── Toggle Status ─────────────────────────────────────────

        public async Task<UserRecord?> ToggleStatusAsync(
            int userId, int businessAccountId, int requestingUserId, string ip)
        {
            using var db = new SqlConnection(_conn);

            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                var record = await db.QuerySingleOrDefaultAsync<UserRecord>(
                    "dbo.usp_Users_ToggleStatus", p, commandType: CommandType.StoredProcedure);

                ThrowIfSpError(p, "USER");
                return record;
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in ToggleStatusAsync UserId={Id}", userId);
                throw;
            }
        }

        // ── Delete ────────────────────────────────────────────────

        public async Task DeleteAsync(
            int userId, int businessAccountId, int requestingUserId, string ip)
        {
            using var db = new SqlConnection(_conn);

            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                await db.ExecuteAsync("dbo.usp_Users_Delete", p,
                    commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "USER");
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in DeleteAsync UserId={Id}", userId);
                throw;
            }
        }

        // ── Reset Password ────────────────────────────────────────

        public async Task ResetPasswordAsync(
            int userId, int businessAccountId, int requestingUserId,
            string newPasswordHash, string newPasswordSalt, string ip)
        {
            using var db = new SqlConnection(_conn);

            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@BusinessAccountId", businessAccountId);
            p.Add("@RequestingUserId", requestingUserId);
            p.Add("@NewPasswordHash", newPasswordHash);
            p.Add("@NewPasswordSalt", newPasswordSalt);
            p.Add("@IpAddress", ip);
            p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            try
            {
                await db.ExecuteAsync("dbo.usp_Users_ResetPassword", p,
                    commandType: CommandType.StoredProcedure);
                ThrowIfSpError(p, "USER");
            }
            catch (AppException) { throw; }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in ResetPasswordAsync UserId={Id}", userId);
                throw;
            }
        }

        // ── Helpers ───────────────────────────────────────────────

        private static string BuildInitials(string name) =>
            string.Join("",
                name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Take(2)
                    .Select(w => char.ToUpper(w[0])));

        private static byte[] DecodeRowVersion(string base64)
        {
            try { return Convert.FromBase64String(base64); }
            catch { throw new ArgumentException("Invalid row version format.", nameof(base64)); }
        }

        /// <summary>
        /// Reads @ErrorCode and @ErrorMessage output parameters and throws
        /// AppException if errorCode != 0.
        /// </summary>
        private static void ThrowIfSpError(
            DynamicParameters p,
            string prefix,
            Func<int, int>? httpStatusResolver = null)
        {
            int code = p.Get<int>("@ErrorCode");
            string message = p.Get<string>("@ErrorMessage") ?? string.Empty;

            if (code == 0) return;

            int httpStatus = httpStatusResolver?.Invoke(code) ?? 400;
            throw new AppException(message, $"{prefix}_{code}", httpStatus);
        }
    }
}