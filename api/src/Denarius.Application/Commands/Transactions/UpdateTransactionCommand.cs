using Denarius.Application.Domain.Commands.Transactions;
using Denarius.Application.Domain.Queries.Transactions;
using Denarius.Application.Domain.Results.Transactions;
using Denarius.Application.Exceptions;
using Denarius.Application.Extensions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Commands.Transactions;

internal class UpdateTransactionCommand(ICategoryRepository categoryRepository, ITransactionRepository transactionRepository) : Command<UpdateTransactionQuery, TransactionResult>, IUpdateTransactionCommand
{
    protected override async Task<TransactionResult> Handle(UpdateTransactionQuery query)
    {
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

        transaction = transactionRepository.Update(transaction);

        return transaction.ToResult();
    }

    protected override void Validate(UpdateTransactionQuery query)
    {
        if (!query.UserId.IsValidId()) throw new BadRequestException("User id is required");
        if (!query.Id.IsValidId()) throw new BadRequestException("Transaction id is required");

        if (query.Amount == decimal.Zero) throw new BadRequestException("Amount can't be equal to 0");

        if (!query.Description.IsValidString()) throw new BadRequestException("Description is required");
        if (query.Description.Length < 3) throw new BadRequestException("Description length can't be lower than 3");
        if (query.Description.Length > 50) throw new BadRequestException("Description length can't be greater than 50");

        if (!query.CategoryId.IsValidId()) throw new BadRequestException("Invalid category id");
    }
}