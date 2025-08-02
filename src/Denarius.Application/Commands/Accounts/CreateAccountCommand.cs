using Denarius.Application.Domain.Commands.Accounts;
using Denarius.Application.Domain.Queries.Accounts;
using Denarius.Application.Domain.Results.Accounts;
using Denarius.Application.Extensions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Domain.UnitOfWork;

namespace Denarius.Application.Commands.Accounts;

internal class CreateAccountCommand(
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

        return account.ToResult();
    }
}
