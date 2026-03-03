using Denarius.Application.Commands.Transactions;
using Denarius.Application.Exceptions;
using Denarius.Application.Interfaces.Transactions;
using Denarius.Application.Results;
using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces;
using Denarius.Domain.ValueObjects;

namespace Denarius.Application.UseCases.Transactions;

public class UpdateTransactionUseCase(
    IAccountRepository acountRepository,
    ITagRepository tagRepository,
    ITransactionRepository transactionRepository) : IUpdateTransactionUseCase
{
    public async Task<TransactionResult> Execute(UpdateTransactionCommand command)
    {
        var id = new Identifier(command.Id);
        var transaction = await transactionRepository.FindByIdAsync(id);
        if (transaction is null) throw new NotFoundException("Transaction not found.");

        var tag = await FindTag(command.TagId);
        var account = await FindAccount(command.AccountId);
        if (account is null) throw new NotFoundException("Account not found.");

        transaction.Retitle(new Name(command.Title));
        transaction.ChangeDate(DateOnly.FromDateTime(command.Date));
        transaction.ChangeAmount(new Money(command.Amount, account.InitialBalance.Code));
        transaction.ChangeType(command.Type);
        transaction.SetAccount(account);
        transaction.SetTag(tag);

        await transactionRepository.UpdateAsync(transaction);

        return new TransactionResult(transaction);
    }

    private async Task<Account> FindAccount(string accountId)
    {
        var id = new Identifier(accountId);
        return await acountRepository.FindByIdAsync(id);
    }

    private async Task<Tag?> FindTag(string? tagId)
    {
        if (tagId is null) return null;
        var id = new Identifier(tagId);
        return await tagRepository.FindByIdAsync(id);
    }
}