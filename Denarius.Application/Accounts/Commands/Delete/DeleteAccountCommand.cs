using Denarius.Application.Accounts.Results;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Accounts.Commands.Delete;

internal class DeleteAccountCommand(IAccountRepository accountRepository) : IDeleteAccountCommand
{
    public async Task<AccountResult> Execute(DeleteAccountQuery query)
    {
        query.Validate();

        var account = await accountRepository.GetByIdAsync(query.Id, query.UserId);

        if (account is null)
        {
            throw new NotFoundException("Account not found");
        }

        account = await accountRepository.DeleteAsync(account);

        return account.ToAccountResult();
    }
}
