using Denarius.Application.Exceptions.Categories;
using Denarius.Application.Exceptions.Transactions;
using Denarius.Application.Inputs.Transactions;
using Denarius.Application.Interfaces.UseCases.Transactions;
using Denarius.Application.Outputs.Transactions;
using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Exceptions.Transactions;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Transactions;

public class UpdateTransactionUseCase(
    IAccountRepository accountRepository,
    ICategoryRepository categoryRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork)
    : IUpdateTransactionUseCase
{
    public async Task<TransactionOutput> Execute(UpdateTransactionInput input)
    {
        var transaction = await transactionRepository.GetByIdAsync(input.TransactionId, input.UserId);
        if (transaction is null)
            throw new TransactionNotFoundException(input.TransactionId);

        var account = await accountRepository.GetByIdAsync(transaction.AccountId, input.UserId);

        var oldAmount = transaction.Amount;
        transaction.UpdateAmount(input.Amount);

        if (input.Amount != oldAmount)
        {
            var correction = AccountDelta(transaction, input.Amount) - AccountDelta(transaction, oldAmount);
            account!.ApplyDelta(correction);

            if (transaction.IsTransfer)
            {
                var peer = await transactionRepository.GetByIdAsync(transaction.TransferPeerId!.Value, input.UserId);
                var peerAccount = await accountRepository.GetByIdAsync(peer!.AccountId, input.UserId);
                var peerCorrection = AccountDelta(peer, input.Amount) - AccountDelta(peer, oldAmount);
                peerAccount!.ApplyDelta(peerCorrection);
                peer.UpdateAmount(input.Amount);
                await transactionRepository.UpdateAsync(peer);
                await accountRepository.UpdateAsync(peerAccount);
            }
        }

        transaction.UpdateDescription(input.Description);

        if (!transaction.IsTransfer)
        {
            if (!input.CategoryId.HasValue)
                throw new InvalidCategoryException("Income and Expense transactions must have a category.");

            var category = await categoryRepository.GetByIdAsync(input.CategoryId.Value, input.UserId);
            if (category is null)
                throw new CategoryNotFoundException(input.CategoryId.Value);
            if (!category.AcceptsTransactionType(transaction.Type))
                throw new InvalidCategoryException($"Category is not compatible with transaction type '{transaction.Type}'.");

            transaction.UpdateCategory(input.CategoryId);
        }
        else if (input.CategoryId.HasValue)
        {
            transaction.UpdateCategory(input.CategoryId);
        }

        await transactionRepository.UpdateAsync(transaction);
        await accountRepository.UpdateAsync(account!);
        await unitOfWork.CommitAsync();

        return TransactionOutput.FromEntity(transaction);
    }

    // Returns the signed balance delta that this transaction applied to its account.
    // Incoming transfers and Income credit (+); outgoing transfers and Expense debit (-).
    private static decimal AccountDelta(Transaction tx, decimal amount) =>
        (tx.IsIncomingTransfer || tx.Type == TransactionType.Income) ? amount : -amount;
}
