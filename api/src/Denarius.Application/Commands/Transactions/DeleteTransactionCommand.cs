using Denarius.Application.Domain.Commands.Transactions;
using Denarius.Application.Domain.Queries.Transactions;
using Denarius.Application.Domain.Results.Transactions;
using Denarius.Application.Extensions;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Repositories;
using Denarius.Domain.UnitOfWork;

namespace Denarius.Application.Commands.Transactions;

internal class DeleteTransactionCommand(
    IUnitOfWork unitOfWork,
    ITransactionRepository transactionRepository
) : IDeleteTransactionCommand
{
    public async Task<TransactionResult> Execute(DeleteTransactionQuery query)
    {
        query.Validate();

        var transaction = await transactionRepository.FindOneAsync(t => t.Id == query.Id && t.UserId == query.UserId);
        if (transaction is null) throw new NotFoundException("Transaction not found");

        await unitOfWork.BeginTransactionAsync();
        try
        {
            transaction = transactionRepository.Delete(transaction);
            await unitOfWork.CommitAsync();
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }

        return transaction.ToResult();
    }
}
