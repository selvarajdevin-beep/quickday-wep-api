using Shop.Web.API.Models.Domain;

namespace Shop.Web.API.Repositories
{
    public interface IAuthRepository
    {
        /// <summary>
        /// Fetches user record for login. SP handles locked/inactive checks.
        /// Throws AppException if SP returns non-zero error code.
        /// </summary>
        Task<UserRecord?> GetUserForLoginAsync(string phone);

        /// <summary>
        /// Atomically creates BusinessAccount + Admin User in one transaction.
        /// </summary>
        Task RegisterAsync(RegisterDbParams p);

        /// <summary>
        /// Called after BCrypt confirms password is correct.
        /// Resets failed attempts, stores refresh token, records last login.
        /// </summary>
        Task RecordLoginSuccessAsync(
            int userId, string ip, string ua,
            string refreshToken, DateTime refreshExpiry);

        /// <summary>
        /// Called after BCrypt rejects password.
        /// Increments failed attempts, locks account after threshold.
        /// </summary>
        Task RecordLoginFailureAsync(int userId, string ip, string ua);

        /// <summary>
        /// Clears refresh token from DB (logout / token revocation).
        /// </summary>
        Task LogoutAsync(int userId, string ip);

        /// <summary>
        /// Validates existing refresh token, rotates it, returns user for new JWT.
        /// </summary>
        Task<UserRecord?> RefreshTokenAsync(
            string token, string newToken,
            DateTime newExpiry, string ip);
    }
}
