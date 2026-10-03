using Denarius.Application.IO.Transactions;
using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;

namespace Denarius.Application.UseCases.Transactions.List;

internal class ListTransactionsUseCase(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository) : IListTransactionsUseCase
{
    public async Task<IEnumerable<TransactionOutput>> Execute(ListTransactionsInput input)
    {
        var transactions = await transactionRepository.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(input.Description))
        {
            var description = input.Description.Trim();
            transactions = transactions.Where(t => t.Description != null && t.Description.Contains(description, StringComparison.OrdinalIgnoreCase));
        }

        if (input.DateRef.HasValue)
        {
            var rangeStart = new DateTime(input.DateRef.Value.Year, input.DateRef.Value.Month, 1);
            var rangeEnd = rangeStart.AddMonths(1).AddTicks(-1);
            transactions = transactions.Where(t => t.Date >= rangeStart && t.Date <= rangeEnd);
        }

        transactions = input.Type switch
        {
            TransactionType.In => transactions.Where(t => t.Value > 0),
            TransactionType.Out => transactions.Where(t => t.Value < 0),
            _ => transactions
        };

        if (input.CategoryId.HasValue)
            transactions = transactions.Where(t => t.CategoryId == input.CategoryId.Value);

        // The list is grouped by day, so the day always comes first and the chosen field orders the transactions within it.
        var dayAscending = input.OrderBy == TransactionOrderField.Date && input.Ascending;
        var ordered = OrderBy(transactions, t => t.Date.Date, dayAscending);

        ordered = input.OrderBy switch
        {
            TransactionOrderField.Description => ThenBy(ordered, t => t.Description, input.Ascending),
            TransactionOrderField.Value => ThenBy(ordered, t => t.Value, input.Ascending),
            TransactionOrderField.CategoryName => ThenBy(ordered, await CategoryNameAsync(), input.Ascending),
            _ => ordered
        };

        return ThenBy(ordered, t => t.CreatedAt, dayAscending).Select(transaction => new TransactionOutput(transaction)).ToList();
    }

    private async Task<Func<Transaction, string>> CategoryNameAsync()
    {
        var categoryNames = (await categoryRepository.GetAllAsync(null)).ToDictionary(c => c.Id, c => c.Name);

        return transaction => categoryNames.GetValueOrDefault(transaction.CategoryId, string.Empty);
    }

    private static IOrderedEnumerable<Transaction> OrderBy<TKey>(IEnumerable<Transaction> transactions, Func<Transaction, TKey> key, bool ascending) =>
        ascending ? transactions.OrderBy(key) : transactions.OrderByDescending(key);

    private static IOrderedEnumerable<Transaction> ThenBy<TKey>(IOrderedEnumerable<Transaction> transactions, Func<Transaction, TKey> key, bool ascending) =>
        ascending ? transactions.ThenBy(key) : transactions.ThenByDescending(key);
}
