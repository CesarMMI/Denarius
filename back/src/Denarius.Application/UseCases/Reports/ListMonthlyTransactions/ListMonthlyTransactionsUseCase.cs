using Denarius.Application.IO.Reports;
using Denarius.Domain.Repositories;

namespace Denarius.Application.UseCases.Reports.ListMonthlyTransactions;

internal class ListMonthlyTransactionsUseCase(ITransactionRepository transactionRepository, TimeProvider timeProvider) : IListMonthlyTransactionsUseCase
{
    public async Task<IEnumerable<MonthlyTransactionOutput>> Execute(ListMonthlyTransactionsInput input)
    {
        var month = input.Month ?? YearMonth.FromDate(timeProvider.GetTodayInSaoPaulo());

        var transactions = await transactionRepository.GetWithCategoryNameAsync(month.FirstDay, month.AddMonths(1).FirstDay);

        // Every transaction of the month, newest first; on the same date, the one recorded last comes first.
        return transactions
            .OrderByDescending(t => t.Transaction.Date)
            .ThenByDescending(t => t.Transaction.CreatedAt)
            .Select(t => new MonthlyTransactionOutput(t.Transaction, t.CategoryName))
            .ToList();
    }
}
