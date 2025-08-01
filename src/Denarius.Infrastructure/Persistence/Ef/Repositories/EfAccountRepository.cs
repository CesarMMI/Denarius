using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Infrastructure.Persistence.Ef.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Ef.Repositories;

internal class EfAccountRepository(EfAppDbContext context) : IAccountRepository
{
    private readonly DbSet<Account> _dbSet = context.Set<Account>();

    public async Task<Account> CreateAsync(Account account)
    {
        await _dbSet.AddAsync(account);
        return account;
    }

    public Task<Account> DeleteAsync(Account account)
    {
        _dbSet.Attach(account);
        _dbSet.Remove(account);
        return Task.FromResult(account);
    }

    public async Task<IList<Account>> GetAllAsync(int userId)
    {
        return await _dbSet
          .Where(a => a.UserId == userId)
          .ToListAsync();
    }

    public async Task<Account?> GetByIdAsync(int id, int userId)
    {
        return await _dbSet
           .Where(a => a.Id == id && a.UserId == userId)
           .FirstOrDefaultAsync();
    }

    public Task<Account> UpdateAsync(Account account)
    {
        _dbSet.Attach(account);
        return Task.FromResult(account);
    }
}
