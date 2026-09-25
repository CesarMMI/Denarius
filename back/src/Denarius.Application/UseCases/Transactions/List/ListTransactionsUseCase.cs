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

        transactions = input.OrderBy switch
        {
            TransactionOrderField.Description => input.Ascending ? transactions.OrderBy(t => t.Description) : transactions.OrderByDescending(t => t.Description),
            TransactionOrderField.Value => input.Ascending ? transactions.OrderBy(t => t.Value) : transactions.OrderByDescending(t => t.Value),
            TransactionOrderField.CategoryName => await OrderByCategoryNameAsync(transactions, input.Ascending),
            _ => input.Ascending ? transactions.OrderBy(t => t.Date) : transactions.OrderByDescending(t => t.Date)
        };

        return transactions.Select(transaction => new TransactionOutput(transaction)).ToList();
    }

    private async Task<IEnumerable<Transaction>> OrderByCategoryNameAsync(IEnumerable<Transaction> transactions, bool ascending)
    {
        var categoryNames = (await categoryRepository.GetAllAsync(null)).ToDictionary(c => c.Id, c => c.Name);

        string CategoryName(Transaction transaction) => categoryNames.GetValueOrDefault(transaction.CategoryId, string.Empty);

        return ascending ? transactions.OrderBy(CategoryName) : transactions.OrderByDescending(CategoryName);
    }
}
