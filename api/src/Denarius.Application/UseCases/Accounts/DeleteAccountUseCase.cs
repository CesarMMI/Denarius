using Denarius.Application.Commands;
using Denarius.Application.Exceptions;
using Denarius.Application.Interfaces.Accounts;
using Denarius.Application.Results;
using Denarius.Domain.Interfaces;
using Denarius.Domain.ValueObjects;

namespace Denarius.Application.UseCases.Accounts;

public class DeleteAccountUseCase(IAccountRepository accountRepository, ITransactionRepository transactionRepository) : IDeleteAccountUseCase
{
    public async Task<AccountResult> Execute(IdCommand command)
    {
        var id = new Identifier(command.Id);
        var account = await accountRepository.FindByIdAsync(id);
        if (account is null) throw new NotFoundException("Account not found.");

        var transactionsIds = (await transactionRepository.FindByAccountAsync(account.Id))
            .Select(transaction => transaction.Id)
            .ToList();
        
        if (transactionsIds.Count > 0) await transactionRepository.DeleteBatchAsync(transactionsIds);
        await accountRepository.DeleteAsync(account.Id);

        return new AccountResult(account);
    }
}