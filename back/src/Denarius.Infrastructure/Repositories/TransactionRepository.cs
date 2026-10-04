using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using Denarius.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Repositories;

public class TransactionRepository(DenariusDbContext context) : Repository<Transaction>(context), ITransactionRepository
{
    public async Task<IEnumerable<Transaction>> GetAllAsync()
    {
        return await Context.Set<Transaction>().ToListAsync();
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

    public async Task<IEnumerable<(Transaction Transaction, string CategoryName)>> GetWithCategoryNameAsync(DateOnly from, DateOnly to)
    {
        var transactions = await DatedBetween(from, to)
            .Join(Context.Set<Category>(), t => t.CategoryId, c => c.Id, (t, c) => new { Transaction = t, CategoryName = c.Name })
            .ToListAsync();

        return transactions.Select(t => (t.Transaction, t.CategoryName)).ToList();
    }

    /// <summary>Transaction dates are calendar days, stored as their UTC midnight.</summary>
    private IQueryable<Transaction> DatedBetween(DateOnly from, DateOnly to)
    {
        var start = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        return Context.Set<Transaction>().AsNoTracking().Where(t => t.Date >= start && t.Date < end);
    }
}
