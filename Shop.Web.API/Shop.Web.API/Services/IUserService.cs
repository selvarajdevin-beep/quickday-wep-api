using Shop.Web.Api.Models.Requests;
using Shop.Web.Api.Models.Responses;
using Shop.Web.API.Models.Responses;

namespace Shop.Web.API.Services
{
    public interface IUserService
    {
        //Task<List<UserDto>> GetAllAsync(int businessAccountId, int requestingUserId);

        Task<PagedResponse<UserDto>> GetAllAsync(
            int businessAccountId,
            int requestingUserId,
            string? search,
            string? status,
            string? role,
            int page,
            int pageSize);

        Task<UserDto> GetByIdAsync(int userId, int businessAccountId);
        Task<UserDto> CreateAsync(int businessAccountId, int requestingUserId, CreateUserRequest req, string ip);
        Task<UserDto> UpdateAsync(int userId, int businessAccountId, int requestingUserId, UpdateUserRequest req, string ip);
        Task<UserDto> ToggleStatusAsync(int userId, int businessAccountId, int requestingUserId, string ip);
        Task DeleteAsync(int userId, int businessAccountId, int requestingUserId, string ip);
        Task ResetPasswordAsync(int userId, int businessAccountId, int requestingUserId, ResetPasswordRequest req, string ip);
    }
}
