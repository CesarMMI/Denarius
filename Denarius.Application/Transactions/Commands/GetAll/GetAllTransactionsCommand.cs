using Denarius.Application.Transactions.Results;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Transactions.Commands.GetAll;

internal class GetAllTransactionsCommand(ITransactionRepository transactionRepository) : IGetAllTransactionsCommand
{
    public async Task<IList<TransactionResult>> Execute(GetAllTransactionsQuery query)
    {
        query.Validate();

        var transactions = await transactionRepository.GetAllAsync(query.UserId);

        return [.. transactions.Select(a => a.ToTransactionResult())];
    }
}
