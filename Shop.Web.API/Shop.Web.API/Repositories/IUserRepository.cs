using Shop.Web.Api.Models.Requests;
using Shop.Web.API.Models.Domain;

namespace Shop.Web.API.Repositories
{
    public interface IUserRepository
    {
        //Task<List<UserRecord>> GetAllAsync(int businessAccountId, int requestingUserId);

        Task<(List<UserRecord> Items, int TotalCount)> GetAllAsync(
            int businessAccountId,
            int requestingUserId,
            string? search,
            string? status,
            string? role,
            int page,
            int pageSize);

        Task<UserRecord?> GetByIdAsync(int userId, int businessAccountId);
        Task<UserRecord?> CreateAsync(int businessAccountId, int requestingUserId, CreateUserRequest req, string ip);
        Task<UserRecord?> UpdateAsync(int userId, int businessAccountId, int requestingUserId, UpdateUserRequest req, string ip);
        Task<UserRecord?> ToggleStatusAsync(int userId, int businessAccountId, int requestingUserId, string ip);
        Task DeleteAsync(int userId, int businessAccountId, int requestingUserId, string ip);
        Task ResetPasswordAsync(int userId, int businessAccountId, int requestingUserId, string newPasswordHash, string newPasswordSalt, string ip);
    }

}
