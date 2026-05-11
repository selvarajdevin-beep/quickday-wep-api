using System.ComponentModel.DataAnnotations;

namespace Shop.Web.API.Models.Requests
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter a valid 10-digit mobile number.")]
        public string Phone { get; init; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; init; } = string.Empty;
    }
}