using Denarius.Application.Accounts.Results;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Accounts.Commands.Update;

internal class UpdateAccountCommand(IAccountRepository accountRepository) : IUpdateAccountCommand
{
    public async Task<AccountResult> Execute(UpdateAccountQuery query)
    {
        query.Validate();

        var account = await accountRepository.GetByIdAsync(query.Id, query.UserId);

        if(account is null)
        {
            throw new NotFoundException("Account not found");
        }

        account.Name = query.Name;
        account.Color = query.Color;

        account = await accountRepository.UpdateAsync(account);

        return account.ToAccountResult();
    }
}
