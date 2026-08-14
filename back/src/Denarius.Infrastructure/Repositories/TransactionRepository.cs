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
}
