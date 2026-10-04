namespace Denarius.Application.IO.Reports;

/// <summary>A month of the income-vs-expense series; money out is a positive amount.</summary>
public record IncomeVsExpenseOutput
{
    public string Month { get; init; }
    public decimal Income { get; init; }
    public decimal Expense { get; init; }
    public decimal Balance { get; init; }

    public IncomeVsExpenseOutput(YearMonth month, decimal income, decimal expense, decimal balance)
    {
        Month = month.ToString();
        Income = income;
        Expense = expense;
        Balance = balance;
    }
}
