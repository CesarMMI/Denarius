using Denarius.Application.Exceptions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.UseCases.Transactions.Delete;

internal class DeleteTransactionUseCase(ITransactionRepository transactionRepository, IUnitOfWork unitOfWork) : IDeleteTransactionUseCase
{
    public async Task Execute(Guid input)
    {
        var transaction = await transactionRepository.GetByIdAsync(input)
            ?? throw new NotFoundException("Transação não encontrada.");

        await transactionRepository.DeleteAsync(transaction);
        await unitOfWork.SaveChangesAsync();
    }
}
