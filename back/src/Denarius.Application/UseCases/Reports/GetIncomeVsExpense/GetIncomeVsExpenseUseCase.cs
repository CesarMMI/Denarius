using Denarius.Application.IO.Reports;
using Denarius.Domain.Repositories;

namespace Denarius.Application.UseCases.Reports.GetIncomeVsExpense;

internal class GetIncomeVsExpenseUseCase(ITransactionRepository transactionRepository, TimeProvider timeProvider) : IGetIncomeVsExpenseUseCase
{
    public async Task<IEnumerable<IncomeVsExpenseOutput>> Execute(GetIncomeVsExpenseInput input)
    {
        var lastMonth = input.Month ?? YearMonth.FromDate(timeProvider.GetTodayInSaoPaulo());
        var firstMonth = lastMonth.AddMonths(1 - input.Months);

        var totals = (await transactionRepository.SumByMonthAsync(firstMonth.FirstDay, lastMonth.AddMonths(1).FirstDay))
            .ToDictionary(t => new YearMonth(t.Year, t.Month), t => (t.Income, t.Expense));

        // A month without transactions has nothing to sum, so the series fills it in with zeros.
        return Enumerable.Range(0, input.Months)
            .Select(firstMonth.AddMonths)
            .Select(month =>
            {
                var (income, expense) = totals.GetValueOrDefault(month);
                return new IncomeVsExpenseOutput(month, income, expense, income - expense);
            })
            .ToList();
    }
}
