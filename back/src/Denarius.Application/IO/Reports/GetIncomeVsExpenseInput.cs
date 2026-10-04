namespace Denarius.Application.IO.Reports;

public record GetIncomeVsExpenseInput
{
    /// <summary>The last month of the series; the current month when null.</summary>
    public YearMonth? Month { get; init; }
    /// <summary>How many months the series covers, from 1 to 24 (checked by the controller).</summary>
    public int Months { get; init; }

    public GetIncomeVsExpenseInput(YearMonth? month = null, int months = 12)
    {
        Month = month;
        Months = months;
    }
}
