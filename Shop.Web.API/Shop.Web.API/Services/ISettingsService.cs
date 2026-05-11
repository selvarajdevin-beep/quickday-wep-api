using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;

namespace Shop.Web.API.Services
{
    public interface ISettingsService
    {
        Task<SettingsDto> GetSettingsAsync(int businessAccountId, int requestingUserId);
        Task<SettingsDto> UpdateSettingsAsync(int businessAccountId, int requestingUserId, UpdateSettingsRequest req);
        Task<AllRolePermissionsDto> GetAllRolePermissionsAsync(int businessAccountId);
        Task<List<PermissionDto>> SaveRolePermissionsAsync(int businessAccountId, int requestingUserId, string role, SaveRolePermissionsRequest req);

        Task<MyRolePermissionsDto> GetMyRolePermissionsAsync(int businessAccountId, string role);
    }
}