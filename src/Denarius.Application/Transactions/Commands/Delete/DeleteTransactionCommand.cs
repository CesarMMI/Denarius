using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.UnitOfWork;
using Denarius.Application.Transactions.Results;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Transactions.Commands.Delete;

public class DeleteTransactionCommand(
    IUnitOfWork unitOfWork,
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository
) : IDeleteTransactionCommand
{
    public async Task<TransactionResult> Execute(DeleteTransactionQuery query)
    {
        query.Validate();

        var transaction = await transactionRepository.GetByIdAsync(query.Id, query.UserId);
        if (transaction is null) throw new NotFoundException("Transaction not found");

        await unitOfWork.BeginTransactionAsync();
        try
        {
            await UpdateAccountBalance(query, transaction);

            transaction = await transactionRepository.DeleteAsync(transaction);

            await unitOfWork.CommitAsync();
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }

        return transaction.ToTransactionResult();
    }

    private async Task UpdateAccountBalance(DeleteTransactionQuery query, Domain.Models.Transaction transaction)
    {
        var account = await accountRepository.GetByIdAsync(transaction.AccountId, query.UserId);
        if (account is null) throw new NotFoundException("Account not found");

        account.Balance -= transaction.Amount;

        await accountRepository.UpdateAsync(account);
    }
}
