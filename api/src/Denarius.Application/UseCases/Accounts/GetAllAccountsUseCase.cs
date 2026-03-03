using Denarius.Application.Commands;
using Denarius.Application.Interfaces.Accounts;
using Denarius.Application.Results;
using Denarius.Domain.Interfaces;

namespace Denarius.Application.UseCases.Accounts;

internal class GetAllAccountsUseCase(IAccountRepository accountRepository) : IGetAllAccountsUseCase
{
    public IEnumerable<AccountResult> Execute(Command command)
    {
        return accountRepository.Find()
            .ToList()
            .Select(tag => new AccountResult(tag));
    }
}
