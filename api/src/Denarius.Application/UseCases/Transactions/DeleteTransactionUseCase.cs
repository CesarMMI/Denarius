using Denarius.Application.Commands;
using Denarius.Application.Exceptions;
using Denarius.Application.Interfaces.Transactions;
using Denarius.Application.Results;
using Denarius.Domain.Interfaces;

namespace Denarius.Application.UseCases.Transactions;

internal class DeleteTransactionUseCase(ITransactionRepository transactionRepository) : IDeleteTransactionUseCase
{
    public async Task<TransactionResult> Execute(IdCommand command)
    {
        var transaction = await transactionRepository.FindByIdAsync(command.Id);

        if (transaction is null) throw new NotFoundException("Transaction not found.");

        transactionRepository.Delete(transaction);
        await transactionRepository.SaveAsync();

        return new TransactionResult(transaction);
    }
}