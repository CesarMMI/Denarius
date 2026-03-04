using Denarius.Application.Commands;
using Denarius.Application.Exceptions;
using Denarius.Application.Interfaces.Accounts;
using Denarius.Application.Results;
using Denarius.Domain.Interfaces;

namespace Denarius.Application.UseCases.Accounts;

internal class DeleteAccountUseCase(IAccountRepository accountRepository, ITransactionRepository transactionRepository) : IDeleteAccountUseCase
{
    public async Task<AccountResult> Execute(IdCommand command)
    {
        var account = await accountRepository.FindByIdAsync(command.Id);

        if (account is null) throw new NotFoundException("Account not found.");

        var transactions = (await transactionRepository.FindByAccountAsync(account.Id)).ToList();

        if (transactions.Count > 0) transactionRepository.DeleteBatch(transactions);

        accountRepository.Delete(account);
        await accountRepository.SaveAsync();

        return new AccountResult(account);
    }
}