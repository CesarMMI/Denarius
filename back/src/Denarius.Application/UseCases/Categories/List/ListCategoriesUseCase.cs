using Denarius.Application.IO.Categories;
using Denarius.Domain.Repositories;

namespace Denarius.Application.UseCases.Categories.List;

internal class ListCategoriesUseCase(ICategoryRepository categoryRepository, ITransactionRepository transactionRepository) : IListCategoriesUseCase
{
    public async Task<IEnumerable<CategoryOutput>> Execute(ListCategoriesInput input)
    {
        var categories = await categoryRepository.GetAllAsync(input.Name);
        var transactions = await transactionRepository.GetAllAsync();

        if (input.DateRef.HasValue)
        {
            var rangeStart = new DateTime(input.DateRef.Value.Year, input.DateRef.Value.Month, 1);
            var rangeEnd = rangeStart.AddMonths(1).AddTicks(-1);
            transactions = transactions.Where(t => t.Date >= rangeStart && t.Date <= rangeEnd);
        }

        var transactionsByCategory = transactions
            .GroupBy(t => t.CategoryId)
            .ToDictionary(g => g.Key, g => (Count: g.Count(), Balance: g.Sum(t => t.Value)));

        var output = categories.Select(category =>
        {
            transactionsByCategory.TryGetValue(category.Id, out var aggregate);
            return new CategoryOutput(category, aggregate.Count, aggregate.Balance);
        });

        if (input.WithTransaction.HasValue)
            output = input.WithTransaction.Value
                ? output.Where(o => o.TransactionCount > 0)
                : output.Where(o => o.TransactionCount == 0);

        output = input.OrderBy switch
        {
            CategoryOrderField.TransactionCount => input.Ascending ? output.OrderBy(o => o.TransactionCount) : output.OrderByDescending(o => o.TransactionCount),
            CategoryOrderField.Balance => input.Ascending ? output.OrderBy(o => o.Balance) : output.OrderByDescending(o => o.Balance),
            _ => input.Ascending ? output.OrderBy(o => o.Name) : output.OrderByDescending(o => o.Name)
        };

        return output.ToList();
    }
}
