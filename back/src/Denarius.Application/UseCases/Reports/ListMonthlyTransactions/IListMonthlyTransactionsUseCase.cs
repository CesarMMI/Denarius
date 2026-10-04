using Denarius.Application.IO.Reports;

namespace Denarius.Application.UseCases.Reports.ListMonthlyTransactions;

public interface IListMonthlyTransactionsUseCase : IUseCase<ListMonthlyTransactionsInput, Task<IEnumerable<MonthlyTransactionOutput>>>
{
}
