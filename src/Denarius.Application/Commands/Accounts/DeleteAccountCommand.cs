using Denarius.Application.Domain.Commands.Accounts;
using Denarius.Application.Domain.Queries.Accounts;
using Denarius.Application.Domain.Results.Accounts;
using Denarius.Application.Extensions;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Repositories;
using Denarius.Domain.UnitOfWork;

namespace Denarius.Application.Commands.Accounts;

internal class DeleteAccountCommand(
    IUnitOfWork unitOfWork,
    IAccountRepository accountRepository
) : IDeleteAccountCommand
{
    public async Task<AccountResult> Execute(DeleteAccountQuery query)
    {
        query.Validate();

        var account = await accountRepository.FindOneAsync(acc => acc.Id == query.Id && acc.UserId == query.UserId);
        if (account is null) throw new NotFoundException("Account not found");

        await unitOfWork.BeginTransactionAsync();
        try
        {
            account = accountRepository.Delete(account);
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
