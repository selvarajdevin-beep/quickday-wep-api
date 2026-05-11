using FluentValidation;
using Shop.Web.API.Models.Requests;


namespace Shop.Web.API.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        private static readonly string[] ValidShopTypes =
        [
            "Water Can Supplier", "Bakery", "Mobile Shop", "Grocery Store",
        "Pharmacy", "Stationery", "Fruit & Vegetable", "Dairy & Milk",
        "Restaurant / Mess", "Hardware Store", "Clothing & Textiles",
        "Electronics", "General Store", "Other"
        ];

        public RegisterRequestValidator()
        {
            // ── Business Info ─────────────────────────────────
            RuleFor(x => x.BusinessName)
                .NotEmpty().WithMessage("Business name is required.")
                .MaximumLength(200);

            RuleFor(x => x.OwnerName)
                .NotEmpty().WithMessage("Owner name is required.")
                .MaximumLength(200);

            RuleFor(x => x.BusinessPhone)
                .Matches(@"^\d{10}$").WithMessage("Enter a valid 10-digit business phone.")
                .When(x => !string.IsNullOrWhiteSpace(x.BusinessPhone));

            RuleFor(x => x.BusinessEmail)
                .EmailAddress().WithMessage("Enter a valid business email.")
                .When(x => !string.IsNullOrWhiteSpace(x.BusinessEmail));

            RuleFor(x => x.GSTIN)
                .Matches(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$")
                .WithMessage("Enter a valid GSTIN (e.g. 33AABCA1234A1Z5).")
                .When(x => !string.IsNullOrWhiteSpace(x.GSTIN));

            RuleFor(x => x.ShopType)
                .NotEmpty().WithMessage("Please select your shop type.")
                .Must(t => ValidShopTypes.Contains(t)).WithMessage("Invalid shop type selected.");

            // ── Account Setup ─────────────────────────────────
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MaximumLength(100);

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Mobile number is required.")
                .Matches(@"^\d{10}$").WithMessage("Enter a valid 10-digit mobile number.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Enter a valid email address.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Please confirm your password.")
                .Equal(x => x.Password).WithMessage("Passwords do not match.");
        }
    }
}
