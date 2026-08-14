using Denarius.Application.IO.Transactions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.UseCases.Transactions.List;

internal class ListTransactionsUseCase(ITransactionRepository transactionRepository) : IListTransactionsUseCase
{
    public async Task<IEnumerable<TransactionOutput>> Execute(object? input)
    {
        var transactions = await transactionRepository.GetAllAsync();

        return transactions.Select(transaction => new TransactionOutput(transaction));
    }
}
