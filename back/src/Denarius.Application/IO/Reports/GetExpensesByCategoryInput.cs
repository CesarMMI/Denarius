namespace Denarius.Application.IO.Reports;

public record GetExpensesByCategoryInput
{
    /// <summary>The month to break down; the current month when null.</summary>
    public YearMonth? Month { get; init; }

    public GetExpensesByCategoryInput(YearMonth? month = null)
    {
        Month = month;
    }
}
