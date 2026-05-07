using Denarius.Application.Exceptions.Accounts;
using Denarius.Application.Inputs.Transactions;
using Denarius.Application.Interfaces.UseCases.Transactions;
using Denarius.Application.Outputs.Transactions;
using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Exceptions.Transactions;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Transactions;

public class CreateTransferUseCase(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork)
    : ICreateTransferUseCase
{
    public async Task<CreateTransferOutput> Execute(CreateTransferInput input)
    {
        var sourceAccount = await accountRepository.GetByIdAsync(input.SourceAccountId, input.UserId);
        if (sourceAccount is null)
            throw new AccountNotFoundException(input.SourceAccountId);
        if (!sourceAccount.IsActive)
            throw new InactiveAccountException(input.SourceAccountId);

        var destinationAccount = await accountRepository.GetByIdAsync(input.DestinationAccountId, input.UserId);
        if (destinationAccount is null)
            throw new AccountNotFoundException(input.DestinationAccountId);
        if (!destinationAccount.IsActive)
            throw new InactiveAccountException(input.DestinationAccountId);

        if (input.SourceAccountId == input.DestinationAccountId)
            throw new InvalidTransferException("Source and destination accounts must be different.");

        if (sourceAccount.CurrencyCode != destinationAccount.CurrencyCode)
            throw new InvalidTransferException("Source and destination accounts must have the same currency code.");

        // Create incoming first with a placeholder peer so the constructor validation passes,
        // then create outgoing using incoming.Id, and finally update incoming to point back.
        var incoming = new Transaction(
            input.UserId,
            input.DestinationAccountId,
            null,
            Guid.NewGuid(),
            TransactionType.Transfer,
            input.Amount,
            input.Description,
            input.Date);
        incoming.MarkAsIncoming();

        var outgoing = new Transaction(
            input.UserId,
            input.SourceAccountId,
            null,
            incoming.Id,
            TransactionType.Transfer,
            input.Amount,
            input.Description,
            input.Date);

        incoming.LinkTransferPeer(outgoing.Id);

        sourceAccount.ApplyDelta(-input.Amount);
        destinationAccount.ApplyDelta(input.Amount);

        await transactionRepository.AddRangeAsync([outgoing, incoming]);
        await accountRepository.UpdateAsync(sourceAccount);
        await accountRepository.UpdateAsync(destinationAccount);
        await unitOfWork.CommitAsync();

        return new CreateTransferOutput(
            TransactionOutput.FromEntity(outgoing),
            TransactionOutput.FromEntity(incoming));
    }
}
