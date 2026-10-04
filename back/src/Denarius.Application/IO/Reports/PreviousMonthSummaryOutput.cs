namespace Denarius.Application.IO.Reports;

/// <summary>
/// The month before a summary's month, and the percentage change from each of its values to the summary's: measured
/// against the size of the previous value, and null when that value is zero.
/// </summary>
public record PreviousMonthSummaryOutput
{
    public decimal TotalIncome { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal Balance { get; init; }
    public decimal? TotalIncomeChange { get; init; }
    public decimal? TotalExpenseChange { get; init; }
    public decimal? BalanceChange { get; init; }

    public PreviousMonthSummaryOutput(decimal totalIncome, decimal totalExpense, decimal balance, decimal? totalIncomeChange, decimal? totalExpenseChange, decimal? balanceChange)
    {
        TotalIncome = totalIncome;
        TotalExpense = totalExpense;
        Balance = balance;
        TotalIncomeChange = totalIncomeChange;
        TotalExpenseChange = totalExpenseChange;
        BalanceChange = balanceChange;
    }
}
