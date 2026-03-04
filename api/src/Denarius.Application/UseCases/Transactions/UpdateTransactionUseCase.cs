using Denarius.Application.Commands.Transactions;
using Denarius.Application.Exceptions;
using Denarius.Application.Interfaces.Transactions;
using Denarius.Application.Results;
using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces;
using Denarius.Domain.ValueObjects;

namespace Denarius.Application.UseCases.Transactions;

internal class UpdateTransactionUseCase(
    IAccountRepository acountRepository,
    ITagRepository tagRepository,
    ITransactionRepository transactionRepository) : IUpdateTransactionUseCase
{
    public async Task<TransactionResult> Execute(UpdateTransactionCommand command)
    {
        var transaction = await transactionRepository.FindByIdAsync(command.Id);

        if (transaction is null) throw new NotFoundException("Transaction not found.");

        var tag = await FindTag(command.TagId);
        var account = await FindAccount(command.AccountId);

        if (account is null) throw new NotFoundException("Account not found.");

        transaction.Retitle(Name.New(command.Title));
        transaction.ChangeDate(DateOnly.FromDateTime(command.Date));
        transaction.ChangeAmount(Money.New(command.Amount, account.InitialBalance.Code));
        transaction.ChangeType(command.Type);
        transaction.SetAccount(account);
        transaction.SetTag(tag);

        transactionRepository.Update(transaction);
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