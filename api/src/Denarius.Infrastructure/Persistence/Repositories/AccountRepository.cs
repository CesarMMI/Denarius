using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Repositories;

public class AccountRepository(DenariusDbContext context) : IAccountRepository
{
    public async Task<Account?> GetByIdAsync(Guid id, Guid userId) =>
        await context.Accounts
                     .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

    public async Task<IEnumerable<Account>> ListByUserAsync(Guid userId) =>
        await context.Accounts
                     .Where(a => a.UserId == userId)
                     .OrderBy(a => a.Name)
                     .ToListAsync();

    public async Task AddAsync(Account account) =>
        await context.Accounts.AddAsync(account);

    public Task UpdateAsync(Account account)
    {
        context.Accounts.Update(account);
        return Task.CompletedTask;
    }
}
