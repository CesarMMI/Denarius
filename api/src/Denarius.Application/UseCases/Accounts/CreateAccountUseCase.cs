using Denarius.Application.Commands.Accounts;
using Denarius.Application.Interfaces.Accounts;
using Denarius.Application.Results;
using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces;
using Denarius.Domain.ValueObjects;

namespace Denarius.Application.UseCases.Accounts;

public class CreateAccountUseCase(IAccountRepository accountRepository) : ICreateAccountUseCase
{
    public async Task<AccountResult> Execute(CreateAccountCommand command)
    {
        var account = new Account(
            Identifier.New(),
            new Name(command.Name ?? string.Empty, "account name"),
            new Color(command.Color ?? string.Empty),
            new Money(command.Balance ?? 0, command.CurrencyCode ?? string.Empty));

        await accountRepository.AddAsync(account);

        return new AccountResult(account);
    }
}
