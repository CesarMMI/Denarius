using Denarius.Application.Domain.Commands.Accounts;
using Denarius.Application.Domain.Queries.Accounts;
using Denarius.Application.Domain.Results.Accounts;
using Denarius.Application.Extensions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Commands.Accounts;

internal class GetAllAccountsCommand(IAccountRepository accountRepository) : IGetAllAccountsCommand
{
    public async Task<IEnumerable<AccountResult>> Execute(GetAllAccountsQuery query)
    {
        query.Validate();
        var accounts = await accountRepository.FindManyAsync(acc => acc.UserId == query.UserId);
        return accounts.Select(acc => acc.ToResult());
    }
}
