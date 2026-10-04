using Denarius.Application.IO.Reports;
using Denarius.Domain.Repositories;

namespace Denarius.Application.UseCases.Reports.GetCumulativeExpenseComparison;

internal class GetCumulativeExpenseComparisonUseCase(ITransactionRepository transactionRepository, TimeProvider timeProvider) : IGetCumulativeExpenseComparisonUseCase
{
    public async Task<CumulativeExpenseComparisonOutput> Execute(GetCumulativeExpenseComparisonInput input)
    {
        var today = timeProvider.GetTodayInSaoPaulo();
        var month = input.Month ?? YearMonth.FromDate(today);
        var previousMonth = month.AddMonths(-1);

        var expenses = (await transactionRepository.SumExpensesByDayAsync(previousMonth.FirstDay, month.AddMonths(1).FirstDay))
            .ToDictionary(e => e.Date, e => e.Expense);

        return new CumulativeExpenseComparisonOutput(
            Accumulate(month, today, expenses),
            Accumulate(previousMonth, today, expenses),
            month.DayCount,
            previousMonth.DayCount);
    }

    /// <summary>The running total of each day of the month that has already come: up to today in the current month.</summary>
    private static List<AccumulatedExpenseOutput> Accumulate(YearMonth month, DateOnly today, Dictionary<DateOnly, decimal> expenses)
    {
        var days = month.CompareTo(YearMonth.FromDate(today)) switch
        {
            < 0 => month.DayCount,
            > 0 => 0,
            _ => today.Day
        };

        var series = new List<AccumulatedExpenseOutput>();
        var accumulated = 0m;
        for (var day = 1; day <= days; day++)
        {
            accumulated += expenses.GetValueOrDefault(new DateOnly(month.Year, month.Month, day));
            series.Add(new AccumulatedExpenseOutput(day, accumulated));
        }

        return series;
    }
}
