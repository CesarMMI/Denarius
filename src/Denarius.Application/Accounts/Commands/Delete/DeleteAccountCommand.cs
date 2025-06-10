using Denarius.Application.Accounts.Results;
using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.UnitOfWork;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Accounts.Commands.Delete;

public class DeleteAccountCommand(
    IUnitOfWork unitOfWork,
    IAccountRepository accountRepository
) : IDeleteAccountCommand
{
    public async Task<AccountResult> Execute(DeleteAccountQuery query)
    {
        query.Validate();

        var account = await accountRepository.GetByIdAsync(query.Id, query.UserId);

        if (account is null) throw new NotFoundException("Account not found");

        await unitOfWork.BeginTransactionAsync();
        try
        {
            account = await accountRepository.DeleteAsync(account);
            await unitOfWork.CommitAsync();
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }

        return account.ToAccountResult();
    }
}
