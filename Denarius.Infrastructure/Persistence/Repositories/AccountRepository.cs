using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Repositories;

internal class AccountRepository(AppDbContext context) : IAccountRepository
{
    public async Task<Account> CreateAsync(Account account)
    {
        await context.Accounts.AddAsync(account);
        await context.SaveChangesAsync();
        return account;
    }

    public async Task<Account> DeleteAsync(Account account)
    {
        context.Accounts.Attach(account);
        context.Accounts.Remove(account);
        await context.SaveChangesAsync();
        return account;
    }

    public async Task<IList<Account>> GetAllAsync(int userId)
    {
        return await context.Accounts
          .Where(a => a.UserId == userId)
          .ToListAsync();
    }

    public async Task<Account?> GetByIdAsync(int id, int userId)
    {
        return await context.Accounts
           .Where(a => a.Id == id && a.UserId == userId)
           .FirstOrDefaultAsync();
    }

    public async Task<Account> UpdateAsync(Account account)
    {
        context.Accounts.Attach(account);
        await context.SaveChangesAsync();
        return account;
    }
}
