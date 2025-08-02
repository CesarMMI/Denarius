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
    IAccountRepository accountRepository,
    ICategoryRepository categoryRepository,
    ITransactionRepository transactionRepository
) : IUpdateTransactionCommand
{
    public async Task<TransactionResult> Execute(UpdateTransactionQuery query)
    {
        query.Validate();

        var transaction = await transactionRepository.FindOneAsync(tra => tra.Id == query.Id && tra.Account.UserId == query.UserId);
        if (transaction is null) throw new NotFoundException("Transaction not found");

        await unitOfWork.BeginTransactionAsync();
        try
        {
            if (query.AccountId != transaction.AccountId)
            {
                transaction = await UpdateTransactionAccount(query, transaction);
            }
            else if (query.Amount != transaction.Amount)
            {
                await AddAccountBalance(query.AccountId, query.UserId, -(transaction.Amount - query.Amount));
            }

            transaction.Amount = query.Amount;
            transaction.Date = query.Date;
            transaction.Description = query.Description;

            if (query.CategoryId.HasValue)
            {
                transaction = await UpdateTransactionCategory(query.CategoryId.Value, query.UserId, transaction);
            }

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

    private async Task<Transaction> UpdateTransactionAccount(UpdateTransactionQuery query, Transaction transaction)
    {
        await AddAccountBalance(transaction.AccountId, query.UserId, -transaction.Amount, "Old account not found");
        var account = await AddAccountBalance(query.AccountId, query.UserId, query.Amount);

        transaction.Account = account;
        transaction.AccountId = account.Id;
        return transaction;
    }

    private async Task<Account> AddAccountBalance(int accountId, int userId, decimal value, string notFoundMessage = "Account not found")
    {
        var account = await accountRepository.FindOneAsync(acc => acc.Id == accountId && acc.UserId == userId);
        if (account is null) throw new NotFoundException(notFoundMessage);

        account.Balance += value;
        account = accountRepository.Update(account);
        return account;
    }

    private async Task<Transaction> UpdateTransactionCategory(int categoryId, int userId, Transaction transaction)
    {
        var category = await categoryRepository.FindOneAsync(cat => cat.Id == categoryId && cat.UserId == userId);
        if (category is null) throw new NotFoundException("Category not found");

        transaction.Category = category;
        transaction.CategoryId = category.Id;
        return transaction;
    }
}