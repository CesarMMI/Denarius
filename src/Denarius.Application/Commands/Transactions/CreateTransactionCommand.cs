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

        var account = await AttachAccount(query, transaction);
        await AttachCategory(query, transaction);

        await unitOfWork.BeginTransactionAsync();
        try
        {
            accountRepository.Update(account);
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

    private async Task<Account> AttachAccount(CreateTransactionQuery query, Transaction transaction)
    {
        var account = await accountRepository.FindOneAsync(acc => acc.Id == query.AccountId && acc.UserId == query.UserId);
        if (account is null) throw new NotFoundException("Account not found");

        account.Balance += transaction.Amount;
        transaction.Account = account;
        transaction.AccountId = account.Id;

        return account;
    }

    private async Task AttachCategory(CreateTransactionQuery query, Transaction transaction)
    {
        if (!query.CategoryId.HasValue) return;

        var category = await categoryRepository.FindOneAsync(cat => cat.Id == query.CategoryId.Value && cat.UserId == query.UserId);
        if (category is null) throw new NotFoundException("Category not found");

        transaction.Category = category;
        transaction.CategoryId = category.Id;
    }
}
