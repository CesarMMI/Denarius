using Denarius.Application.Domain.Commands.Transactions;
using Denarius.Application.Domain.Queries.Transactions;
using Denarius.Application.Domain.Results.Transactions;
using Denarius.Application.Extensions;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Domain.UnitOfWork;

namespace Denarius.Application.Commands.Transactions;

internal class UpdateTransactionCommand(
    IUnitOfWork unitOfWork,
    ICategoryRepository categoryRepository,
    ITransactionRepository transactionRepository
) : IUpdateTransactionCommand
{
    public async Task<TransactionResult> Execute(UpdateTransactionQuery query)
    {
        query.Validate();

        var transaction = await transactionRepository.FindOneAsync(t => t.Id == query.Id && t.UserId == query.UserId);
        if (transaction is null) throw new NotFoundException("Transaction not found");

        transaction.Amount = query.Amount;
        transaction.Date = query.Date;
        transaction.Description = query.Description;

        if (query.CategoryId.HasValue)
        {
            var category = await categoryRepository.FindOneAsync(c => c.Id == query.CategoryId.Value && c.UserId == query.UserId);
            if (category is null) throw new NotFoundException("Category not found");
            transaction.Category = category;
            transaction.CategoryId = category.Id;
        }
        else
        {
            transaction.Category = null;
            transaction.CategoryId = null;
        }

        await unitOfWork.BeginTransactionAsync();
        try
        {
            transaction = transactionRepository.Update(transaction);
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