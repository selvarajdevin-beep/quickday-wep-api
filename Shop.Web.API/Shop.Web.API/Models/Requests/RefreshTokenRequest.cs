using System.ComponentModel.DataAnnotations;

namespace Shop.Web.API.Models.Requests
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; init; } = string.Empty;
    }
}