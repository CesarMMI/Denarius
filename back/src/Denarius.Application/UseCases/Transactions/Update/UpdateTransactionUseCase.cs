using Denarius.Application.Exceptions;
using Denarius.Application.IO.Transactions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.UseCases.Transactions.Update;

internal class UpdateTransactionUseCase(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository, IUnitOfWork unitOfWork) : IUpdateTransactionUseCase
{
    public async Task<TransactionOutput> Execute((Guid Id, UpdateTransactionInput Input) input)
    {
        var transaction = await transactionRepository.GetByIdAsync(input.Id)
            ?? throw new NotFoundException("Transação não encontrada.");

        _ = await categoryRepository.GetByIdAsync(input.Input.CategoryId)
            ?? throw new NotFoundException("Categoria não encontrada.");

        transaction.Update(input.Input.Description, input.Input.Date, input.Input.Value, input.Input.CategoryId);

        await transactionRepository.UpdateAsync(transaction);
        await unitOfWork.SaveChangesAsync();

        return new TransactionOutput(transaction);
    }
}
