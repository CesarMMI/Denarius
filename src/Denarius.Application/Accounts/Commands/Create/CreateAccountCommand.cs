using Denarius.Application.Accounts.Results;
using Denarius.Application.Shared.UnitOfWork;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Accounts.Commands.Create;

public class CreateAccountCommand(
    IUnitOfWork unitOfWork,
    IAccountRepository accountRepository
) : ICreateAccountCommand
{
    public async Task<AccountResult> Execute(CreateAccountQuery query)
    {
        query.Validate();

        var account = new Account
        {
            Name = query.Name,
            Color = query.Color,
            Balance = query.Balance,
            UserId = query.UserId
        };

        await unitOfWork.BeginTransactionAsync();
        try
        {
            account = await accountRepository.CreateAsync(account);
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
