namespace Shop.Web.API.Models.Responses
{
    /// <summary>Mirrors the Expense interface in shared-state.interfaces.ts.</summary>
    public class ExpenseDto
    {
        public int Id { get; init; }
        public int BusinessAccountId { get; init; }
        public string Type { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public DateTime Date { get; init; }
        //public DateOnly Date { get; init; }
        public string Notes { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public string RowVersion { get; init; } = string.Empty;
    }

    public class ExpenseSummaryByTypeDto
    {
        public string Type { get; init; } = string.Empty;
        public decimal Amount { get; init; }
    }

    public class ExpenseSummaryDto
    {
        public decimal TotalThisMonth { get; init; }
        public List<ExpenseSummaryByTypeDto> ByType { get; init; } = [];
    }

}
