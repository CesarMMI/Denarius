using Denarius.Application.IO.Reports;
using Denarius.Domain.Repositories;

namespace Denarius.Application.UseCases.Reports.GetMonthlySummary;

internal class GetMonthlySummaryUseCase(ITransactionRepository transactionRepository, TimeProvider timeProvider) : IGetMonthlySummaryUseCase
{
    public async Task<MonthlySummaryOutput> Execute(GetMonthlySummaryInput input)
    {
        var today = timeProvider.GetTodayInSaoPaulo();
        var currentMonth = YearMonth.FromDate(today);
        var month = input.Month ?? currentMonth;
        var previousMonth = month.AddMonths(-1);

        var totals = (await transactionRepository.SumByMonthAsync(previousMonth.FirstDay, month.AddMonths(1).FirstDay))
            .ToDictionary(t => new YearMonth(t.Year, t.Month), t => (t.Income, t.Expense));
        var (income, expense) = totals.GetValueOrDefault(month);
        var (previousIncome, previousExpense) = totals.GetValueOrDefault(previousMonth);
        var balance = income - expense;
        var previousBalance = previousIncome - previousExpense;

        // A past month closed with its actual values, and a future one has spent nothing to project.
        var (projectedExpense, projectedBalance) = month.CompareTo(currentMonth) switch
        {
            < 0 => (expense, balance),
            > 0 => (0m, 0m),
            _ => await ProjectAsync(month, today, income, expense)
        };

        return new MonthlySummaryOutput(
            month,
            income,
            expense,
            balance,
            income == 0 ? null : Percentage(balance, income),
            projectedExpense,
            projectedBalance,
            new PreviousMonthSummaryOutput(
                previousIncome,
                previousExpense,
                previousBalance,
                Change(previousIncome, income),
                Change(previousExpense, expense),
                Change(previousBalance, balance)));
    }

    /// <summary>
    /// The current month keeps the pace of the expense dated up to today, today included, through its last day, plus the
    /// expense already dated after today, which is certain and not extrapolated. Only the expense is projected: the
    /// month's income is taken as known.
    /// </summary>
    private async Task<(decimal Expense, decimal Balance)> ProjectAsync(YearMonth month, DateOnly today, decimal income, decimal monthExpense)
    {
        var expenseToDate = (await transactionRepository.SumByMonthAsync(month.FirstDay, today.AddDays(1))).Sum(t => t.Expense);
        var expense = Round(expenseToDate * month.DayCount / today.Day) + (monthExpense - expenseToDate);

        return (expense, income - expense);
    }

    /// <summary>Measured against the size of the previous value, so a balance going from −100 to 50 rose 150%.</summary>
    private static decimal? Change(decimal previous, decimal current) =>
        previous == 0 ? null : Percentage(current - previous, Math.Abs(previous));

    private static decimal Percentage(decimal part, decimal whole) => Round(part * 100 / whole);

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
