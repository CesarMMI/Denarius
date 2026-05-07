using Denarius.Application.Exceptions.Accounts;
using Denarius.Application.Inputs.Accounts;
using Denarius.Application.Interfaces.UseCases.Accounts;
using Denarius.Application.Outputs.Accounts;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Accounts;

public class UpdateAccountUseCase(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    : IUpdateAccountUseCase
{
    public async Task<AccountOutput> Execute(UpdateAccountInput input)
    {
        var account = await accountRepository.GetByIdAsync(input.AccountId, input.UserId);

        if (account is null)
            throw new AccountNotFoundException(input.AccountId);

        account.Update(input.Name, input.Color);
        await accountRepository.UpdateAsync(account);
        await unitOfWork.CommitAsync();
        return AccountOutput.FromEntity(account);
    }
}
