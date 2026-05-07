using Denarius.Application.Exceptions.Accounts;
using Denarius.Application.Inputs.Accounts;
using Denarius.Application.Interfaces.UseCases.Accounts;
using Denarius.Application.Outputs.Accounts;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Accounts;

public class GetAccountByIdUseCase(IAccountRepository accountRepository)
    : IGetAccountByIdUseCase
{
    public async Task<AccountOutput> Execute(GetAccountByIdInput input)
    {
        var account = await accountRepository.GetByIdAsync(input.AccountId, input.UserId);

        if (account is null)
            throw new AccountNotFoundException(input.AccountId);

        return AccountOutput.FromEntity(account);
    }
}
