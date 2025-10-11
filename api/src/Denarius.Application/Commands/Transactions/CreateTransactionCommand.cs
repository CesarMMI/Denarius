using Denarius.Application.Domain.Commands.Transactions;
using Denarius.Application.Domain.Queries.Transactions;
using Denarius.Application.Domain.Results.Transactions;
using Denarius.Application.Extensions;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Domain.UnitOfWork;

namespace Denarius.Application.Commands.Transactions;

internal class CreateTransactionCommand(
    IUnitOfWork unitOfWork,
    ICategoryRepository categoryRepository,
    ITransactionRepository transactionRepository
) : ICreateTransactionCommand
{
    public async Task<TransactionResult> Execute(CreateTransactionQuery query)
    {
        query.Validate();

        var transaction = new Transaction
        {
            Amount = query.Amount,
            Date = query.Date,
            Description = query.Description,
        };

        if (query.CategoryId.HasValue)
        {
            var category = await categoryRepository.FindOneAsync(c => c.Id == query.CategoryId.Value && c.UserId == query.UserId);
            if (category is null) throw new NotFoundException("Category not found");
            transaction.Category = category;
            transaction.CategoryId = category.Id;
        }

        await unitOfWork.BeginTransactionAsync();
        try
        {
            transaction = await transactionRepository.CreateAsync(transaction);
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
