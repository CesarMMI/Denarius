using Denarius.Application.Exceptions;
using Denarius.Application.IO.Transactions;
using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;

namespace Denarius.Application.UseCases.Transactions.Create;

internal class CreateTransactionUseCase(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository, IUnitOfWork unitOfWork) : ICreateTransactionUseCase
{
    public async Task<TransactionOutput> Execute(CreateTransactionInput input)
    {
        _ = await categoryRepository.GetByIdAsync(input.CategoryId)
            ?? throw new NotFoundException("Categoria não encontrada.");

        var transaction = new Transaction(input.Description, input.Date, input.Value, input.CategoryId);

        await transactionRepository.AddAsync(transaction);
        await unitOfWork.SaveChangesAsync();

        return new TransactionOutput(transaction);
    }
}
