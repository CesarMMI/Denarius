using Denarius.Application.Commands.Accounts;
using Denarius.Application.Exceptions;
using Denarius.Application.Interfaces.Accounts;
using Denarius.Application.Results;
using Denarius.Domain.Interfaces;
using Denarius.Domain.ValueObjects;

namespace Denarius.Application.UseCases.Accounts;

internal class UpdateAccountUseCase(IAccountRepository accountRepository) : IUpdateAccountUseCase
{
    public async Task<AccountResult> Execute(UpdateAccountCommand command)
    {
        var id = new Identifier(command.Id);
        var account = await accountRepository.FindByIdAsync(id);
        
        if (account is null)
            throw new NotFoundException("Account not found.");

        account.Rename(new Name(command.Name ?? string.Empty, "account name"));
        account.ChangeColor(new Color(command.Color ?? string.Empty));

        accountRepository.Update(account);

        return new AccountResult(account);
    }
}