namespace Denarius.Application.IO.Reports;

public record ListMonthlyTransactionsInput
{
    /// <summary>The month to list; the current month when null.</summary>
    public YearMonth? Month { get; init; }

    public ListMonthlyTransactionsInput(YearMonth? month = null)
    {
        Month = month;
    }
}
