namespace Shop.Web.API.Models.Domain
{
    public class ExpenseRecord
    {
        public int Id { get; init; }
        public int BusinessAccountId { get; init; }
        public string Type { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public DateTime Date { get; init; }
        //public DateOnly Date { get; init; }
        public string Notes { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        //public string? RowVersion { get; init; }
        public byte[]? RowVersion { get; init; }
    }

    public class ExpenseSummaryTotalRecord
    {
        public decimal TotalThisMonth { get; init; }
    }

    public class ExpenseSummaryTypeRecord
    {
        public string Type { get; init; } = string.Empty;
        public decimal Amount { get; init; }
    }
}
