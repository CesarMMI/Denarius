using Denarius.Application.Transactions.Results;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Transactions.Commands.GetById;

internal class GetTransactionByIdCommand(ITransactionRepository transactionRepository) : IGetTransactionByIdCommand
{
    public async Task<TransactionResult> Execute(GetTransactionByIdQuery query)
    {
        query.Validate();

        var transaction = await transactionRepository.GetByIdAsync(query.Id, query.UserId);
        if(transaction is null)
        {
            throw new NotFoundException("Transaction not found");
        }

        return transaction.ToTransactionResult();
    }
}
