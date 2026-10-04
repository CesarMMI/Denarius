namespace Denarius.Application.IO.Reports;

/// <summary>
/// The running total of money out on each day of a month and of the month before. A series stops at today in the current
/// month and has no days in a month that hasn't started.
/// </summary>
public record CumulativeExpenseComparisonOutput
{
    public IEnumerable<AccumulatedExpenseOutput> CurrentMonth { get; init; }
    public IEnumerable<AccumulatedExpenseOutput> PreviousMonth { get; init; }
    public int DaysInCurrentMonth { get; init; }
    public int DaysInPreviousMonth { get; init; }

    public CumulativeExpenseComparisonOutput(IEnumerable<AccumulatedExpenseOutput> currentMonth, IEnumerable<AccumulatedExpenseOutput> previousMonth, int daysInCurrentMonth, int daysInPreviousMonth)
    {
        CurrentMonth = currentMonth;
        PreviousMonth = previousMonth;
        DaysInCurrentMonth = daysInCurrentMonth;
        DaysInPreviousMonth = daysInPreviousMonth;
    }
}
