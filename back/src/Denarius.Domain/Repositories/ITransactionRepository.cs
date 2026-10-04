using Denarius.Domain.Entities;

namespace Denarius.Domain.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetAllAsync();
    Task<bool> ExistsByCategoryIdAsync(Guid categoryId);

    // The report queries add up the transactions dated from `from` up to, but excluding, `to`, and give money out
    // (negative values) as a positive amount.

    /// <summary>The money in and out of each month that has transactions.</summary>
    Task<IEnumerable<(int Year, int Month, decimal Income, decimal Expense)>> SumByMonthAsync(DateOnly from, DateOnly to);

    /// <summary>The money out of each category that has expenses.</summary>
    Task<IEnumerable<(Category Category, decimal Expense)>> SumExpensesByCategoryAsync(DateOnly from, DateOnly to);

    /// <summary>The money out of each day that has expenses.</summary>
    Task<IEnumerable<(DateOnly Date, decimal Expense)>> SumExpensesByDayAsync(DateOnly from, DateOnly to);

    /// <summary>Each transaction, with the name of its category.</summary>
    Task<IEnumerable<(Transaction Transaction, string CategoryName)>> GetWithCategoryNameAsync(DateOnly from, DateOnly to);
}
