using Denarius.Application.Accounts.Results;
using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.UnitOfWork;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Accounts.Commands.Update;

public class UpdateAccountCommand(
    IUnitOfWork unitOfWork,
    IAccountRepository accountRepository
) : IUpdateAccountCommand
{
    public async Task<AccountResult> Execute(UpdateAccountQuery query)
    {
        query.Validate();

        var account = await accountRepository.GetByIdAsync(query.Id, query.UserId);

        if (account is null) throw new NotFoundException("Account not found");

        account.Name = query.Name;
        account.Color = query.Color;

        await unitOfWork.BeginTransactionAsync();
        try
        {
            account = await accountRepository.UpdateAsync(account);
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
