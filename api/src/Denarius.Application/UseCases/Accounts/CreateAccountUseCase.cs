using Denarius.Application.Commands.Accounts;
using Denarius.Application.Interfaces.Accounts;
using Denarius.Application.Results;
using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces;
using Denarius.Domain.ValueObjects;

namespace Denarius.Application.UseCases.Accounts;

internal class CreateAccountUseCase(IAccountRepository accountRepository) : ICreateAccountUseCase
{
    public async Task<AccountResult> Execute(CreateAccountCommand command)
    {
        var account = Account.New(
            Name.New(command.Name, "account name"),
            Color.New(command.Color),
            Money.New(command.Balance ?? 0, command.CurrencyCode));

        await accountRepository.AddAsync(account);
        await accountRepository.SaveAsync();

        return new AccountResult(account);
    }
}
