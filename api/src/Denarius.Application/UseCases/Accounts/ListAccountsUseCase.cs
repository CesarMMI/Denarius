using Denarius.Application.Inputs.Accounts;
using Denarius.Application.Interfaces.UseCases.Accounts;
using Denarius.Application.Outputs.Accounts;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Accounts;

public class ListAccountsUseCase(IAccountRepository accountRepository)
    : IListAccountsUseCase
{
    public async Task<IEnumerable<AccountOutput>> Execute(ListAccountsInput input)
    {
        var accounts = await accountRepository.ListByUserAsync(input.UserId);
        return accounts.Select(AccountOutput.FromEntity);
    }
}
