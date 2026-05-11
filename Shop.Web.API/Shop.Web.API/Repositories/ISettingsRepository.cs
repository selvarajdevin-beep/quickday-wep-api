
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;

namespace Shop.Web.API.Repositories
{
    public interface ISettingsRepository
    {
        Task<SettingsRecord?> GetSettingsAsync(int businessAccountId, int requestingUserId);
        Task<SettingsRecord?> UpdateSettingsAsync(int businessAccountId, int requestingUserId, UpdateSettingsRequest req);
        Task<List<RolePermissionRecord>> GetRolePermissionsAsync(int businessAccountId, string? role = null);
        Task<List<RolePermissionRecord>> SaveRolePermissionsAsync(int businessAccountId, int requestingUserId, string role, List<PermissionEntry> permissions);

        Task<List<RolePermissionRecord>> GetMyRolePermissionsAsync(int businessAccountId, string role);
        Task<PublicSettingsRecord?> GetPublicSettingsAsync(int businessAccountId);
    }
}