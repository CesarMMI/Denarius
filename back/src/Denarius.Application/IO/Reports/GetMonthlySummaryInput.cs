namespace Denarius.Application.IO.Reports;

public record GetMonthlySummaryInput
{
    /// <summary>The month to summarize; the current month when null.</summary>
    public YearMonth? Month { get; init; }

    public GetMonthlySummaryInput(YearMonth? month = null)
    {
        Month = month;
    }
}
