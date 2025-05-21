using Denarius.Application.Accounts.Results;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Accounts.Commands.Create;

internal class CreateAccountCommand(IAccountRepository accountRepository) : ICreateAccountCommand
{
    public async Task<AccountResult> Execute(CreateAccountQuery query)
    {
        query.Validate();

        var account = new Account
        {
            Name = query.Name,
            Color = query.Color,
            Balance = query.Balance,
            UserId = query.UserId
        };

        account = await accountRepository.CreateAsync(account);

        return account.ToAccountResult();
    }
}
