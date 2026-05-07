using Denarius.Application.Exceptions.Accounts;
using Denarius.Application.Inputs.Accounts;
using Denarius.Application.Interfaces.UseCases.Accounts;
using Denarius.Application.Outputs.Accounts;
using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Accounts;

public class CreateAccountUseCase(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    : ICreateAccountUseCase
{
    public async Task<AccountOutput> Execute(CreateAccountInput input)
    {
        var account = new Account(input.UserId, input.Name, input.CurrencyCode, input.Color);
        await accountRepository.AddAsync(account);
        await unitOfWork.CommitAsync();
        return AccountOutput.FromEntity(account);
    }
}
