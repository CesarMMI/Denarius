namespace Denarius.Application.IO.Reports;

/// <summary>The money out of a month from its first day through <see cref="Day"/>.</summary>
public record AccumulatedExpenseOutput
{
    public int Day { get; init; }
    public decimal Accumulated { get; init; }

    public AccumulatedExpenseOutput(int day, decimal accumulated)
    {
        Day = day;
        Accumulated = accumulated;
    }
}
