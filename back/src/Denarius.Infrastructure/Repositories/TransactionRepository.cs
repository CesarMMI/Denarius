using System.Linq.Expressions;
using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using Denarius.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Repositories;

public class TransactionRepository(DenariusDbContext context) : Repository<Transaction>(context), ITransactionRepository
{
    public async Task<IEnumerable<Transaction>> GetAllAsync()
    {
        return await Context.Set<Transaction>().AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> ListAsync(TransactionListQuery query)
    {
        var transactions = Context.Set<Transaction>().AsNoTracking();

        var description = query.Description;
        if (!string.IsNullOrWhiteSpace(description))
            transactions = transactions.Where(t => t.Description != null && EF.Functions.Unaccent(t.Description).ToLower().Contains(EF.Functions.Unaccent(description).ToLower()));

        if (query.From is { } from)
            transactions = transactions.Where(t => t.Date >= from);

        if (query.To is { } to)
            transactions = transactions.Where(t => t.Date < to);

        transactions = query.Type switch
        {
            TransactionKind.In => transactions.Where(t => t.Value > 0),
            TransactionKind.Out => transactions.Where(t => t.Value < 0),
            _ => transactions
        };

        if (query.CategoryId is { } categoryId)
            transactions = transactions.Where(t => t.CategoryId == categoryId);

        if (query.SortBy == TransactionSortField.CategoryName)
            return await ListOrderedByCategoryNameAsync(transactions, query.Ascending);

        var ordered = query.SortBy switch
        {
            TransactionSortField.Date => OrderBy(transactions, t => t.Date, query.Ascending),
            TransactionSortField.Value => OrderBy(transactions, t => t.Value, query.Ascending),
            _ => OrderByDescription(transactions, query.Ascending)
        };

        // Ties fall back to the most recent; ordering by date, they follow its direction.
        ordered = query.SortBy == TransactionSortField.Date
            ? (query.Ascending ? ordered.ThenBy(t => t.CreatedAt) : ordered.ThenByDescending(t => t.CreatedAt))
            : ordered.ThenByDescending(t => t.Date).ThenByDescending(t => t.CreatedAt);

        return await ordered.ToListAsync();
    }

    private static IOrderedQueryable<Transaction> OrderBy<TKey>(IQueryable<Transaction> transactions, Expression<Func<Transaction, TKey>> key, bool ascending) =>
        ascending ? transactions.OrderBy(key) : transactions.OrderByDescending(key);

    /// <summary>Transactions without a description come first when ascending and last when descending.</summary>
    private static IOrderedQueryable<Transaction> OrderByDescription(IQueryable<Transaction> transactions, bool ascending) =>
        ascending
            ? transactions.OrderBy(t => t.Description != null).ThenBy(t => EF.Functions.Collate(t.Description!, DenariusDbContext.PortugueseCollation))
            : transactions.OrderByDescending(t => t.Description != null).ThenByDescending(t => EF.Functions.Collate(t.Description!, DenariusDbContext.PortugueseCollation));

    private async Task<IEnumerable<Transaction>> ListOrderedByCategoryNameAsync(IQueryable<Transaction> transactions, bool ascending)
    {
        var withCategoryName = transactions.Join(Context.Set<Category>(), t => t.CategoryId, c => c.Id, (t, c) => new { Transaction = t, CategoryName = c.Name });

        var ordered = ascending
            ? withCategoryName.OrderBy(x => EF.Functions.Collate(x.CategoryName, DenariusDbContext.PortugueseCollation))
            : withCategoryName.OrderByDescending(x => EF.Functions.Collate(x.CategoryName, DenariusDbContext.PortugueseCollation));

        // Ties fall back to the most recent.
        return await ordered
            .ThenByDescending(x => x.Transaction.Date)
            .ThenByDescending(x => x.Transaction.CreatedAt)
            .Select(x => x.Transaction)
            .ToListAsync();
    }

    public async Task<bool> ExistsByCategoryIdAsync(Guid categoryId)
    {
        return await Context.Set<Transaction>().AnyAsync(t => t.CategoryId == categoryId);
    }

    public async Task<IEnumerable<(int Year, int Month, decimal Income, decimal Expense)>> SumByMonthAsync(DateOnly from, DateOnly to)
    {
        var totals = await DatedBetween(from, to)
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Income = g.Sum(t => t.Value > 0 ? t.Value : 0),
                Expense = -g.Sum(t => t.Value < 0 ? t.Value : 0)
            })
            .ToListAsync();

        return totals.Select(t => (t.Year, t.Month, t.Income, t.Expense)).ToList();
    }

    public async Task<IEnumerable<(Category Category, decimal Expense)>> SumExpensesByCategoryAsync(DateOnly from, DateOnly to)
    {
        var expenses = await DatedBetween(from, to)
            .Where(t => t.Value < 0)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Expense = -g.Sum(t => t.Value) })
            .Join(Context.Set<Category>(), e => e.CategoryId, c => c.Id, (e, c) => new { Category = c, e.Expense })
            .ToListAsync();

        return expenses.Select(e => (e.Category, e.Expense)).ToList();
    }

    public async Task<IEnumerable<(DateOnly Date, decimal Expense)>> SumExpensesByDayAsync(DateOnly from, DateOnly to)
    {
        var expenses = await DatedBetween(from, to)
            .Where(t => t.Value < 0)
            .GroupBy(t => new { t.Date.Year, t.Date.Month, t.Date.Day })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Day, Expense = -g.Sum(t => t.Value) })
            .ToListAsync();

        return expenses.Select(e => (new DateOnly(e.Year, e.Month, e.Day), e.Expense)).ToList();
    }

    public async Task<IEnumerable<(Transaction Transaction, Category Category)>> GetWithCategoryAsync(DateOnly from, DateOnly to)
    {
        var transactions = await DatedBetween(from, to)
            .Join(Context.Set<Category>(), t => t.CategoryId, c => c.Id, (t, c) => new { Transaction = t, Category = c })
            .ToListAsync();

        return transactions.Select(t => (t.Transaction, t.Category)).ToList();
    }

    /// <summary>Transaction dates are calendar days, stored as their UTC midnight.</summary>
    private IQueryable<Transaction> DatedBetween(DateOnly from, DateOnly to)
    {
        var start = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        return Context.Set<Transaction>().AsNoTracking().Where(t => t.Date >= start && t.Date < end);
    }
}
