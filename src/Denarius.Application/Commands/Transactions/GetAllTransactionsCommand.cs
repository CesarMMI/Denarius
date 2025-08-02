using Denarius.Application.Domain.Commands.Transactions;
using Denarius.Application.Domain.Queries.Transactions;
using Denarius.Application.Domain.Results.Transactions;
using Denarius.Application.Extensions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Commands.Transactions;

internal class GetAllTransactionsCommand(ITransactionRepository transactionRepository) : IGetAllTransactionsCommand
{
    public async Task<IEnumerable<TransactionResult>> Execute(GetAllTransactionsQuery query)
    {
        query.Validate();
        var transactions = await transactionRepository.FindManyAsync(tra => tra.Account.UserId == query.UserId);
        return transactions.Select(a => a.ToResult());
    }
}
