using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Repositories;

public class TransactionRepository(DenariusDbContext context) : ITransactionRepository
{
    public async Task<Transaction?> GetByIdAsync(Guid id, Guid userId) =>
        await context.Transactions
                     .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    public async Task<IEnumerable<Transaction>> ListByUserAsync(
        Guid userId,
        Guid? accountId,
        Guid? categoryId,
        TransactionType? type,
        DateTime? startDate,
        DateTime? endDate)
    {
        var query = context.Transactions.Where(t => t.UserId == userId);

        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);

        return await query
                     .OrderByDescending(t => t.Date)
                     .ThenByDescending(t => t.CreatedAt)
                     .ToListAsync();
    }

    public async Task AddAsync(Transaction transaction) =>
        await context.Transactions.AddAsync(transaction);

    public async Task AddRangeAsync(IEnumerable<Transaction> transactions) =>
        await context.Transactions.AddRangeAsync(transactions);

    public Task UpdateAsync(Transaction transaction)
    {
        context.Transactions.Update(transaction);
        return Task.CompletedTask;
    }

    public Task UpdateRangeAsync(IEnumerable<Transaction> transactions)
    {
        context.Transactions.UpdateRange(transactions);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Transaction transaction)
    {
        context.Transactions.Remove(transaction);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<Transaction> transactions)
    {
        context.Transactions.RemoveRange(transactions);
        return Task.CompletedTask;
    }
}
