namespace Shop.Web.API.Models.Responses
{
    /// <summary>
    /// Returned after successful login or token refresh.
    /// Angular stores Token in memory and RefreshToken in an HttpOnly cookie
    /// (or localStorage for simplicity in dev).
    /// </summary>
    public class AuthResponse
    {
        public string Token { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
        public AuthUserDto User { get; init; } = new();
    }
}
