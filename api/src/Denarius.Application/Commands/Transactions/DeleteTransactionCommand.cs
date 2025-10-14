using Denarius.Application.Domain.Commands.Transactions;
using Denarius.Application.Domain.Queries.Transactions;
using Denarius.Application.Domain.Results.Transactions;
using Denarius.Application.Exceptions;
using Denarius.Application.Extensions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Commands.Transactions;

internal class DeleteTransactionCommand(ITransactionRepository transactionRepository) : Command<DeleteTransactionQuery, TransactionResult>, IDeleteTransactionCommand
{
    protected override async Task<TransactionResult> Handle(DeleteTransactionQuery query)
    {
        var transaction = await transactionRepository.FindOneAsync(t => t.Id == query.Id && t.UserId == query.UserId);
        if (transaction is null) throw new NotFoundException("Transaction not found");

        transaction = transactionRepository.Delete(transaction);

        return transaction.ToResult();
    }

    protected override void Validate(DeleteTransactionQuery query)
    {
        if (!query.UserId.IsValidId()) throw new BadRequestException("User id is required");
        if (!query.Id.IsValidId()) throw new BadRequestException("Transaction id is required");
    }
}
