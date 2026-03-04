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
        var account = await accountRepository.FindByIdAsync(command.Id);

        if (account is null) throw new NotFoundException("Account not found.");

        account.Rename(Name.New(command.Name, "account name"));
        account.ChangeColor(Color.New(command.Color));

        accountRepository.Update(account);
        await accountRepository.SaveAsync();

        return new AccountResult(account);
    }
}