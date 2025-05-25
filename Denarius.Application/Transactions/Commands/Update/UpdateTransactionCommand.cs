using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.Services;
using Denarius.Application.Transactions.Results;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Transactions.Commands.Update;

internal class UpdateTransactionCommand(
    IDbTransactionService dbTransactionService,
    IAccountRepository accountRepository,
    ICategoryRepository categoryRepository,
    ITransactionRepository transactionRepository
) : IUpdateTransactionCommand
{
    public async Task<TransactionResult> Execute(UpdateTransactionQuery query)
    {
        query.Validate();

        var transaction = await transactionRepository.GetByIdAsync(query.Id, query.UserId);
        if (transaction is null) throw new NotFoundException("Transaction not found");        

        transaction = await dbTransactionService.ExecuteAsync(async () =>
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

            transaction = await transactionRepository.UpdateAsync(transaction);

            return transaction;
        });

        return transaction.ToTransactionResult();
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
        var account = await accountRepository.GetByIdAsync(accountId, userId);
        if (account is null) throw new NotFoundException(notFoundMessage);

        account.Balance += value;
        account = await accountRepository.UpdateAsync(account);

        return account;
    }

    private async Task<Transaction> UpdateTransactionCategory(int categoryId, int userId, Transaction transaction)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId, userId);
        if (category is null) throw new NotFoundException("Category not found"); 

        transaction.Category = category;
        transaction.CategoryId = category.Id;

        return transaction;
    }
}