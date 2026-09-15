using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using Denarius.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Repositories;

public class CategoryRepository(DenariusDbContext context) : Repository<Category>(context), ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetAllAsync(string? name)
    {
        var query = Context.Set<Category>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(c => c.Name.Contains(name));

        return await query.ToListAsync();
    }
}
