using Denarius.Application.Commands.Transactions;
using Denarius.Application.Exceptions;
using Denarius.Application.Interfaces.Transactions;
using Denarius.Application.Results;
using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces;
using Denarius.Domain.ValueObjects;

namespace Denarius.Application.UseCases.Transactions;

internal class CreateTransactionUseCase(
    IAccountRepository acountRepository,
    ITagRepository tagRepository,
    ITransactionRepository transactionRepository) : ICreateTransactionUseCase
{
    public async Task<TransactionResult> Execute(CreateTransactionCommand command)
    {
        var tag = await FindTag(command.TagId);
        var account = await FindAccount(command.AccountId);

        if (account is null) throw new NotFoundException("Account not found.");

        var transaction = Transaction.New(
            Name.New(command.Title, "transaction title"),
            command.Date,
            Money.New(command.Amount, account.InitialBalance.Code),
            command.Type,
            account.Id,
            tag?.Id);

        await transactionRepository.AddAsync(transaction);
        await transactionRepository.SaveAsync();

        return new TransactionResult(transaction);
    }

    private async Task<Account?> FindAccount(Guid accountId)
    {
        return await acountRepository.FindByIdAsync(accountId);
    }

    private async Task<Tag?> FindTag(Guid? tagId)
    {
        if (!tagId.HasValue) return null;
        return await tagRepository.FindByIdAsync(tagId.Value);
    }
}