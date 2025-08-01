using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Infrastructure.Persistence.Ef.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Ef.Repositories;

internal class EfTransactionRepository(EfAppDbContext context) : ITransactionRepository
{
    private readonly DbSet<Transaction> dbSet = context.Set<Transaction>();

    public async Task<Transaction> CreateAsync(Transaction account)
    {
        await dbSet.AddAsync(account);
        return account;
    }

    public Task<Transaction> DeleteAsync(Transaction account)
    {
        dbSet.Attach(account);
        dbSet.Remove(account);
        return Task.FromResult(account);
    }

    public async Task<IList<Transaction>> GetAllAsync(int userId)
    {
        return await dbSet
          .Where(t => t.Account.UserId == userId)
          .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(int id, int userId)
    {
        return await dbSet
           .Where(a => a.Id == id && a.Account.UserId == userId)
           .FirstOrDefaultAsync();
    }

    public Task<Transaction> UpdateAsync(Transaction account)
    {
        dbSet.Attach(account);
        return Task.FromResult(account);
    }
}
