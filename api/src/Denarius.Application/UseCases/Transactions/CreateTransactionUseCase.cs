using Denarius.Application.Exceptions.Accounts;
using Denarius.Application.Exceptions.Categories;
using Denarius.Application.Inputs.Transactions;
using Denarius.Application.Interfaces.UseCases.Transactions;
using Denarius.Application.Outputs.Transactions;
using Denarius.Domain.Entities;
using Denarius.Domain.Exceptions.Transactions;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Transactions;

public class CreateTransactionUseCase(
    IAccountRepository accountRepository,
    ICategoryRepository categoryRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork)
    : ICreateTransactionUseCase
{
    public async Task<TransactionOutput> Execute(CreateTransactionInput input)
    {
        var account = await accountRepository.GetByIdAsync(input.AccountId, input.UserId);
        if (account is null)
            throw new AccountNotFoundException(input.AccountId);
        if (!account.IsActive)
            throw new InactiveAccountException(input.AccountId);

        var category = await categoryRepository.GetByIdAsync(input.CategoryId, input.UserId);
        if (category is null)
            throw new CategoryNotFoundException(input.CategoryId);
        if (!category.AcceptsTransactionType(input.Type))
            throw new InvalidCategoryException($"Category is not compatible with transaction type '{input.Type}'.");

        var transaction = new Transaction(
            input.UserId,
            input.AccountId,
            input.CategoryId,
            null,
            input.Type,
            input.Amount,
            input.Description,
            input.Date);

        account.ApplyDelta(transaction.ToDelta());

        await transactionRepository.AddAsync(transaction);
        await accountRepository.UpdateAsync(account);
        await unitOfWork.CommitAsync();

        return TransactionOutput.FromEntity(transaction);
    }
}
