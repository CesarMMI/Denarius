using Denarius.Application.Domain.Commands.Transactions;
using Denarius.Application.Domain.Queries.Transactions;
using Denarius.Application.Domain.Results.Transactions;
using Denarius.Application.Exceptions;
using Denarius.Application.Extensions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Commands.Transactions;

internal class CreateTransactionCommand(ICategoryRepository categoryRepository, ITransactionRepository transactionRepository) : Command<CreateTransactionQuery, TransactionResult>, ICreateTransactionCommand
{
    protected override async Task<TransactionResult> Handle(CreateTransactionQuery query)
    {
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

        transaction = await transactionRepository.CreateAsync(transaction);
        
        return transaction.ToResult();
    }

    protected override void Validate(CreateTransactionQuery query)
    {
        if (!query.UserId.IsValidId()) throw new BadRequestException("User id is required");

        if (query.Amount == decimal.Zero) throw new BadRequestException("Amount can't be equal to 0");

        if (!query.Description.IsValidString()) throw new BadRequestException("Description is required");
        if (query.Description.Length < 3) throw new BadRequestException("Description length can't be lower than 3");
        if (query.Description.Length > 50) throw new BadRequestException("Description length can't be greater than 50");

        if (!query.CategoryId.IsValidId()) throw new BadRequestException("Invalid category id");
    }
}
