using Denarius.Application.Accounts.Results;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Accounts.Commands.GetAll;

public class GetAllAccountsCommand(IAccountRepository accountRepository) : IGetAllAccountsCommand
{
    public async Task<IList<AccountResult>> Execute(GetAllAccountsQuery query)
    {
        query.Validate();

        var accounts = await accountRepository.GetAllAsync(query.UserId);

        return [.. accounts.Select(a => a.ToAccountResult())];
    }
}
