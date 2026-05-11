using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;

namespace Shop.Web.API.Services
{

    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest req, string ip, string ua);
        Task RegisterAsync(RegisterRequest req, string ip);
        Task<AuthResponse> RefreshAsync(string refreshToken, string ip);
        Task LogoutAsync(int userId, string ip);
    }
}
