using Denarius.Domain.Entities;

namespace Denarius.Domain.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetAllAsync();

    /// <summary>The transactions that meet every filter of the query, in the order it asks for.</summary>
    Task<IEnumerable<Transaction>> ListAsync(TransactionListQuery query);

    Task<bool> ExistsByCategoryIdAsync(Guid categoryId);

    // The report queries add up the transactions dated from `from` up to, but excluding, `to`, and give money out
    // (negative values) as a positive amount.

    /// <summary>The money in and out of each month that has transactions.</summary>
    Task<IEnumerable<(int Year, int Month, decimal Income, decimal Expense)>> SumByMonthAsync(DateOnly from, DateOnly to);

    /// <summary>The money out of each category that has expenses.</summary>
    Task<IEnumerable<(Category Category, decimal Expense)>> SumExpensesByCategoryAsync(DateOnly from, DateOnly to);

    /// <summary>The money out of each day that has expenses.</summary>
    Task<IEnumerable<(DateOnly Date, decimal Expense)>> SumExpensesByDayAsync(DateOnly from, DateOnly to);

    /// <summary>Each transaction, with its category.</summary>
    Task<IEnumerable<(Transaction Transaction, Category Category)>> GetWithCategoryAsync(DateOnly from, DateOnly to);
}
