using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.Services;
using Denarius.Application.Transactions.Results;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Transactions.Commands.Create;

internal class CreateTransactionCommand(
    IDbTransactionService dbTransactionService,
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

        transaction = await dbTransactionService.ExecuteAsync(async () =>
        {
            transaction = await AddTransactionAccount(query.AccountId, query.UserId, transaction);

            if (query.CategoryId.HasValue) transaction = await AddTransactionCategory(query.CategoryId.Value, query.UserId, transaction);

            transaction = await transactionRepository.CreateAsync(transaction);

            return transaction;
        });

        return transaction.ToTransactionResult();
    }

    private async Task<Transaction> AddTransactionAccount(int accountId, int userId, Transaction transaction)
    {
        var account = await accountRepository.GetByIdAsync(accountId, userId);
        if (account is null) throw new NotFoundException("Account not found");

        account.Balance += transaction.Amount;
        await accountRepository.UpdateAsync(account);

        transaction.Account = account;
        transaction.AccountId = account.Id;

        return transaction;
    }

    private async Task<Transaction> AddTransactionCategory(int categoryId, int userId, Transaction transaction)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId, userId);
        if (category is null) throw new NotFoundException("Category not found");

        transaction.Category = category;
        transaction.CategoryId = category.Id;

        return transaction;
    }
}
