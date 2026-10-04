namespace Denarius.Application.IO.Reports;

/// <summary>Money out is a positive amount; percentages are on a 0–100 scale, null where they don't apply.</summary>
public record MonthlySummaryOutput
{
    public string Month { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal Balance { get; init; }
    /// <summary>The balance over the income; null without income.</summary>
    public decimal? SavingsRate { get; init; }
    public decimal ProjectedExpense { get; init; }
    public decimal ProjectedBalance { get; init; }
    public PreviousMonthSummaryOutput PreviousMonth { get; init; }

    public MonthlySummaryOutput(YearMonth month, decimal totalIncome, decimal totalExpense, decimal balance, decimal? savingsRate, decimal projectedExpense, decimal projectedBalance, PreviousMonthSummaryOutput previousMonth)
    {
        Month = month.ToString();
        TotalIncome = totalIncome;
        TotalExpense = totalExpense;
        Balance = balance;
        SavingsRate = savingsRate;
        ProjectedExpense = projectedExpense;
        ProjectedBalance = projectedBalance;
        PreviousMonth = previousMonth;
    }
}
