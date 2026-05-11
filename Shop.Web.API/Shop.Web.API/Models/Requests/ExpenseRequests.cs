using System.ComponentModel.DataAnnotations;

namespace Shop.Web.API.Models.Requests
{
    public class CreateExpenseRequest
    {
        [Required(ErrorMessage = "Expense type is required.")]
        [RegularExpression(
            @"^(Petrol|Salary|Vehicle Maintenance|Rent|Electricity|Misc)$",
            ErrorMessage = "Invalid expense type.")]
        public string Type { get; init; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; init; }

        [Required(ErrorMessage = "Expense date is required.")]
        public DateTime Date { get; init; }
        //public DateOnly Date { get; init; }

        [MaxLength(1000)]
        public string? Notes { get; init; }
    }

    public class UpdateExpenseRequest : CreateExpenseRequest
    {
        [Required(ErrorMessage = "Row version is required.")]
        public string RowVersion { get; init; } = string.Empty;
    }

    public sealed record GetExpensesParams
    {
        public DateOnly? From { get; init; }
        public DateOnly? To { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? Type { get; init; }
        public string? Search { get; init; }
    }

}
