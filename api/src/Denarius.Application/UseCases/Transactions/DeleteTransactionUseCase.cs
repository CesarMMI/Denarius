using Denarius.Application.Commands;
using Denarius.Application.Exceptions;
using Denarius.Application.Interfaces.Transactions;
using Denarius.Application.Results;
using Denarius.Domain.Interfaces;
using Denarius.Domain.ValueObjects;

namespace Denarius.Application.UseCases.Transactions;

internal class DeleteTransactionUseCase(ITransactionRepository transactionRepository) : IDeleteTransactionUseCase
{
    public async Task<TransactionResult> Execute(IdCommand command)
    {
        var id = new Identifier(command.Id);
        var transaction = await transactionRepository.FindByIdAsync(id);
        
        if (transaction is null)
            throw new NotFoundException("Transaction not found.");

        transactionRepository.Delete(transaction);

        return new TransactionResult(transaction);
    }
}