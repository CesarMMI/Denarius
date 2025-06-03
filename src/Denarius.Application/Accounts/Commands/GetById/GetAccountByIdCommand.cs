using Denarius.Application.Accounts.Results;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Accounts.Commands.GetById;

public class GetAccountByIdCommand(IAccountRepository accountRepository) : IGetAccountByIdCommand
{
    public async Task<AccountResult> Execute(GetAccountByIdQuery query)
    {
        query.Validate();

        var account = await accountRepository.GetByIdAsync(query.Id, query.UserId);

        if(account is null)
        {
            throw new NotFoundException("Account not found");
        }

        return account.ToAccountResult();
    }
}
