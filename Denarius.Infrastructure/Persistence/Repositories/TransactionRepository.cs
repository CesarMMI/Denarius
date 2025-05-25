using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Repositories;

internal class TransactionRepository(AppDbContext context) : ITransactionRepository
{
    public async Task<Transaction> CreateAsync(Transaction account)
    {
        await context.Transactions.AddAsync(account);
        await context.SaveChangesAsync();
        return account;
    }

    public async Task<Transaction> DeleteAsync(Transaction account)
    {
        context.Transactions.Attach(account);
        context.Transactions.Remove(account);
        await context.SaveChangesAsync();
        return account;
    }

    public async Task<IList<Transaction>> GetAllAsync(int userId)
    {
        return await context.Transactions
          .Where(t => t.Account.UserId == userId)
          .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(int id, int userId)
    {
        return await context.Transactions
           .Where(a => a.Id == id && a.Account.UserId == userId)
           .FirstOrDefaultAsync();
    }

    public async Task<Transaction> UpdateAsync(Transaction account)
    {
        context.Transactions.Attach(account);
        await context.SaveChangesAsync();
        return account;
    }
}
