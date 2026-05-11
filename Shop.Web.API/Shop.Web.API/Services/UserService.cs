using Shop.Web.Api.Models.Requests;
using Shop.Web.Api.Models.Responses;
using Shop.Web.API.Exceptions;
using Shop.Web.API.Helpers;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Repositories;
using Shop.Web.API.Services;
using System.Text.Json;

namespace AquaERP.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly ILogger<UserService> _logger;

        // JsonSerializerOptions reused across calls — camelCase matches Angular convention
        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public UserService(IUserRepository repo, ILogger<UserService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        // ── Get All ───────────────────────────────────────────────

        //public async Task<List<UserDto>> GetAllAsync(int businessAccountId, int requestingUserId)
        //{
        //    var records = await _repo.GetAllAsync(businessAccountId, requestingUserId);
        //    return records.Select(MapToDto).ToList();
        //}

        public async Task<PagedResponse<UserDto>> GetAllAsync(
            int businessAccountId,
            int requestingUserId,
            string? search,
            string? status,
            string? role,           // ← NEW
            int page,
            int pageSize)
        {
            var (items, totalCount) = await _repo.GetAllAsync(
                businessAccountId, requestingUserId,
                search, status, role, page, pageSize);   // ← pass role

            return new PagedResponse<UserDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }


        // ── Get By Id ─────────────────────────────────────────────

        public async Task<UserDto> GetByIdAsync(int userId, int businessAccountId)
        {
            var record = await _repo.GetByIdAsync(userId, businessAccountId);
            if (record is null)
                throw new NotFoundException($"User with ID {userId} not found.");
            return MapToDto(record);
        }

        // ── Create ────────────────────────────────────────────────

        public async Task<UserDto> CreateAsync(
            int businessAccountId, int requestingUserId,
            CreateUserRequest req, string ip)
        {
            // Service-layer validation on top of data annotations
            if (req.Password.Length < 6)
                throw new AppException("Password must be at least 6 characters.", "USER_WEAK_PASSWORD");

            var record = await _repo.CreateAsync(businessAccountId, requestingUserId, req, ip);

            if (record is null)
            {
                _logger.LogError(
                    "CreateAsync returned null for BusinessAccountId={Id}", businessAccountId);
                throw new AppException("Failed to create user. Please try again.", "USER_UNEXPECTED");
            }

            _logger.LogInformation(
                "User created: {Name} [{Role}] BusinessAccountId={BizId} by UserId={ReqId}",
                req.Username, req.Role, businessAccountId, requestingUserId);

            return MapToDto(record);
        }

        // ── Update ────────────────────────────────────────────────

        public async Task<UserDto> UpdateAsync(
            int userId, int businessAccountId, int requestingUserId,
            UpdateUserRequest req, string ip)
        {
            var record = await _repo.UpdateAsync(userId, businessAccountId, requestingUserId, req, ip);

            if (record is null)
                throw new NotFoundException($"User with ID {userId} not found.");

            _logger.LogInformation(
                "User updated: UserId={UserId} by RequestingUserId={ReqId}", userId, requestingUserId);

            return MapToDto(record);
        }

        // ── Toggle Status ─────────────────────────────────────────

        public async Task<UserDto> ToggleStatusAsync(
            int userId, int businessAccountId, int requestingUserId, string ip)
        {
            var record = await _repo.ToggleStatusAsync(userId, businessAccountId, requestingUserId, ip);

            if (record is null)
                throw new NotFoundException($"User with ID {userId} not found.");

            _logger.LogInformation(
                "User status toggled: UserId={UserId} → {Status} by RequestingUserId={ReqId}",
                userId, record.Status, requestingUserId);

            return MapToDto(record);
        }

        // ── Delete ────────────────────────────────────────────────

        public async Task DeleteAsync(
            int userId, int businessAccountId, int requestingUserId, string ip)
        {
            await _repo.DeleteAsync(userId, businessAccountId, requestingUserId, ip);

            _logger.LogInformation(
                "User deleted: UserId={UserId} by RequestingUserId={ReqId}", userId, requestingUserId);
        }

        // ── Reset Password ────────────────────────────────────────

        public async Task ResetPasswordAsync(
            int userId, int businessAccountId, int requestingUserId,
            ResetPasswordRequest req, string ip)
        {
            if (req.NewPassword.Length < 6)
                throw new AppException("Password must be at least 6 characters.", "USER_WEAK_PASSWORD");

            var (hash, salt) = PasswordHelper.Hash(req.NewPassword);

            await _repo.ResetPasswordAsync(
                userId, businessAccountId, requestingUserId, hash, salt, ip);

            _logger.LogInformation(
                "Password reset: UserId={UserId} by RequestingUserId={ReqId}", userId, requestingUserId);
        }

        // ── Mapping ───────────────────────────────────────────────

        private static UserDto MapToDto(UserRecord r)
        {
            // Parse the PermissionsJson column returned by FOR JSON PATH in the SP
            var permissions = new List<PermissionDto>();
            if (!string.IsNullOrWhiteSpace(r.PermissionsJson))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<List<PermissionDto>>(
                        r.PermissionsJson, _jsonOpts);
                    if (parsed is not null) permissions = parsed;
                }
                catch
                {
                    // Non-fatal — return empty permissions rather than 500
                }
            }

            //// Convert hex RowVersion string to Base64 for safe JSON transport
            //string rowVersion = string.Empty;
            //if (!string.IsNullOrWhiteSpace(r.RowVersion))
            //{
            //    try
            //    {
            //        // SP returns "0x00000000000007D2" — strip "0x", parse hex bytes, encode Base64
            //        var hex = r.RowVersion.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            //            ? r.RowVersion[2..] : r.RowVersion;
            //        var bytes = Convert.FromHexString(hex);
            //        rowVersion = Convert.ToBase64String(bytes);
            //    }
            //    catch { /* leave empty */ }
            //}

            return new UserDto
            {
                Id = r.Id,
                BusinessAccountId = r.BusinessAccountId,
                Name = r.Name,
                Phone = r.Phone,
                Email = r.Email ?? string.Empty,
                Role = r.Role,
                Status = r.Status,
                Designation = r.Designation,
                Department = r.Department,
                Address = r.Address,
                EmergencyContact = r.EmergencyContact,
                Notes = r.Notes,
                AvatarInitials = r.AvatarInitials,
                DateOfJoining = r.DateOfJoining,
                LastLogin = r.LastLoginAt,
                CreatedAt = r.CreatedAt,
                SalaryDetails = r.MonthlySalary > 0 || r.SalaryType is not null
                    ? new SalaryDetailDto
                    {
                        MonthlySalary = r.MonthlySalary,
                        SalaryType = r.SalaryType ?? "Fixed",
                        BankAccount = r.BankAccount,
                        BankName = r.BankName,
                        IFSC = r.IFSC,
                    }
                    : null,
                Permissions = permissions,
                //RowVersion = rowVersion,
                RowVersion = r.RowVersion is not null
                                ? Convert.ToBase64String(r.RowVersion)
                                : string.Empty
            };
        }
    }
}