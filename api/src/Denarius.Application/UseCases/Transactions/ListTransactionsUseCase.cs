using Denarius.Application.Exceptions.Transactions;
using Denarius.Application.Inputs.Transactions;
using Denarius.Application.Interfaces.UseCases.Transactions;
using Denarius.Application.Outputs.Transactions;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Transactions;

public class ListTransactionsUseCase(ITransactionRepository transactionRepository)
    : IListTransactionsUseCase
{
    public async Task<IEnumerable<TransactionOutput>> Execute(ListTransactionsInput input)
    {
        if (input.StartDate.HasValue && input.EndDate.HasValue && input.StartDate > input.EndDate)
            throw new InvalidDateRangeException();

        var transactions = await transactionRepository.ListByUserAsync(
            input.UserId,
            input.AccountId,
            input.CategoryId,
            input.Type,
            input.StartDate,
            input.EndDate);

        return transactions.Select(TransactionOutput.FromEntity);
    }
}
