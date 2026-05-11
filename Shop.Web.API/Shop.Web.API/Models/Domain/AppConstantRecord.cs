namespace Shop.Web.API.Models.Domain
{
    public sealed record AppConstantRecord(
        string Category,
        string Value,
        string Label,
        string? Icon,
        int SortOrder
    );
}
