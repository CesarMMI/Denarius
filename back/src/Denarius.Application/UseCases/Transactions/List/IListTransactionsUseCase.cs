using Denarius.Application.IO.Transactions;

namespace Denarius.Application.UseCases.Transactions.List;

public interface IListTransactionsUseCase : IUseCase<ListTransactionsInput, Task<IEnumerable<TransactionOutput>>>
{
}
