using Denarius.Application.Domain.Commands.Transactions;
using Denarius.Application.Domain.Queries.Transactions;
using Denarius.Application.Domain.Results.Transactions;
using Denarius.Application.Extensions;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Domain.UnitOfWork;

namespace Denarius.Application.Commands.Transactions;

internal class DeleteTransactionCommand(
    IUnitOfWork unitOfWork,
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository
) : IDeleteTransactionCommand
{
    public async Task<TransactionResult> Execute(DeleteTransactionQuery query)
    {
        query.Validate();

        var transaction = await transactionRepository.FindOneAsync(tra => tra.Id == query.Id && tra.Account.UserId == query.UserId);
        if (transaction is null) throw new NotFoundException("Transaction not found");

        await unitOfWork.BeginTransactionAsync();
        try
        {
            transaction = transactionRepository.Delete(transaction);
            await UpdateAccountBalance(query, transaction);
            await unitOfWork.CommitAsync();
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }

        return transaction.ToResult();
    }

    private async Task UpdateAccountBalance(DeleteTransactionQuery query, Transaction transaction)
    {
        var account = await accountRepository.FindOneAsync(acc => acc.Id == transaction.AccountId && acc.UserId == query.UserId);
        if (account is null) throw new NotFoundException("Account not found");

        account.Balance -= transaction.Amount;
        accountRepository.Update(account);
    }
}
