using Denarius.Application.Exceptions.Transactions;
using Denarius.Application.Inputs.Transactions;
using Denarius.Application.Interfaces.UseCases.Transactions;
using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Transactions;

public class DeleteTransactionUseCase(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork)
    : IDeleteTransactionUseCase
{
    public async Task Execute(DeleteTransactionInput input)
    {
        var transaction = await transactionRepository.GetByIdAsync(input.TransactionId, input.UserId);
        if (transaction is null)
            throw new TransactionNotFoundException(input.TransactionId);

        var account = await accountRepository.GetByIdAsync(transaction.AccountId, input.UserId);
        account!.ApplyDelta(RevertDelta(transaction));

        if (transaction.IsTransfer)
        {
            var peer = await transactionRepository.GetByIdAsync(transaction.TransferPeerId!.Value, input.UserId);
            var peerAccount = await accountRepository.GetByIdAsync(peer!.AccountId, input.UserId);
            peerAccount!.ApplyDelta(RevertDelta(peer));

            await transactionRepository.DeleteRangeAsync([transaction, peer]);
            await accountRepository.UpdateAsync(account);
            await accountRepository.UpdateAsync(peerAccount);
        }
        else
        {
            await transactionRepository.DeleteAsync(transaction);
            await accountRepository.UpdateAsync(account);
        }

        await unitOfWork.CommitAsync();
    }

    // Negates what was applied to the account when the transaction was created.
    // Incoming transfers were credited (+amount), so revert = -amount.
    // Everything else uses -ToDelta(): Income → -amount, Expense → +amount, outgoing Transfer → +amount.
    private static decimal RevertDelta(Transaction tx) =>
        tx.IsIncomingTransfer ? -tx.Amount : -tx.ToDelta();
}
