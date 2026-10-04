namespace Denarius.Application.IO.Reports;

public record GetCumulativeExpenseComparisonInput
{
    /// <summary>The month to compare with the one before; the current month when null.</summary>
    public YearMonth? Month { get; init; }

    public GetCumulativeExpenseComparisonInput(YearMonth? month = null)
    {
        Month = month;
    }
}
