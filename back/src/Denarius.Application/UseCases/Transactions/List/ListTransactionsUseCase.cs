using Denarius.Application.IO.Transactions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.UseCases.Transactions.List;

internal class ListTransactionsUseCase(ITransactionRepository transactionRepository) : IListTransactionsUseCase
{
    public async Task<IEnumerable<TransactionOutput>> Execute(ListTransactionsInput input)
    {
        var monthStart = input.DateRef is { } dateRef ? new DateTime(dateRef.Year, dateRef.Month, 1, 0, 0, 0, DateTimeKind.Utc) : (DateTime?)null;

        var query = new TransactionListQuery
        {
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            From = monthStart,
            To = monthStart?.AddMonths(1),
            Type = input.Type switch
            {
                TransactionType.In => TransactionKind.In,
                TransactionType.Out => TransactionKind.Out,
                _ => TransactionKind.All
            },
            CategoryId = input.CategoryId,
            SortBy = input.OrderBy switch
            {
                TransactionOrderField.Description => TransactionSortField.Description,
                TransactionOrderField.Value => TransactionSortField.Value,
                TransactionOrderField.CategoryName => TransactionSortField.CategoryName,
                _ => TransactionSortField.Date
            },
            Ascending = input.Ascending
        };

        var transactions = await transactionRepository.ListAsync(query);

        return transactions.Select(transaction => new TransactionOutput(transaction)).ToList();
    }
}
