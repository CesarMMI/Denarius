using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.UnitOfWork;
using Denarius.Application.Transactions.Results;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Transactions.Commands.Create;

public class CreateTransactionCommand(
    IUnitOfWork unitOfWork,
    IAccountRepository accountRepository,
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

        var account = await accountRepository.GetByIdAsync(query.AccountId, query.UserId);
        if (account is null) throw new NotFoundException("Account not found");

        transaction.Account = account;
        transaction.AccountId = account.Id;

        await unitOfWork.BeginTransactionAsync();
        try
        {
            account.Balance += transaction.Amount;
            await accountRepository.UpdateAsync(account);

            if (query.CategoryId.HasValue)
            {
                var category = await categoryRepository.GetByIdAsync(query.CategoryId.Value, query.UserId);
                if (category is null) throw new NotFoundException("Category not found");

                transaction.Category = category;
                transaction.CategoryId = category.Id;
            }

            transaction = await transactionRepository.CreateAsync(transaction);

            await unitOfWork.CommitAsync();
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }

        return transaction.ToTransactionResult();
    }
}
