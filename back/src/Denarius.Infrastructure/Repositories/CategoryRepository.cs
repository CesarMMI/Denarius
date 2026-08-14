using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using Denarius.Domain.ValueObjects;
using Denarius.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Repositories;

public class CategoryRepository(DenariusDbContext context) : Repository<Category>(context), ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetAllAsync(string? name, string? color)
    {
        var query = Context.Set<Category>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(c => c.Name.Contains(name));

        var categories = await query.ToListAsync();

        if (!string.IsNullOrWhiteSpace(color))
        {
            var normalizedColor = new Color(color).HexCode;
            categories = categories.Where(c => c.Color.HexCode == normalizedColor).ToList();
        }

        return categories;
    }
}
