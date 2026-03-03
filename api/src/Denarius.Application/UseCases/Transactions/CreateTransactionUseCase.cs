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
        
        if (account is null)
            throw new NotFoundException("Account not found.");

        var transaction = new Transaction(
            Identifier.New(),
            new Name(command.Title, "transaction title"),
            DateOnly.FromDateTime(command.Date),
            new Money(command.Amount, account.InitialBalance.Code),
            command.Type,
            account,
            tag);

        await transactionRepository.AddAsync(transaction);
        
        return new TransactionResult(transaction);
    }

    private async Task<Account?> FindAccount(string accountId)
    {
        return await acountRepository.FindByIdAsync(new Identifier(accountId));
    }

    private async Task<Tag?> FindTag(string? tagId)
    {
        if (tagId is null)
            return null;

        return await tagRepository.FindByIdAsync(new Identifier(tagId));
    }
}