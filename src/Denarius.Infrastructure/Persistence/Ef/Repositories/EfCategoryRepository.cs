using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Infrastructure.Persistence.Ef.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Ef.Repositories;

internal class EfCategoryRepository(EfAppDbContext context) : ICategoryRepository
{
    private readonly DbSet<Category> dbSet = context.Set<Category>();

    public async Task<Category> CreateAsync(Category category)
    {
        await dbSet.AddAsync(category);
        return category;
    }

    public Task<Category> DeleteAsync(Category category)
    {
        dbSet.Attach(category);
        dbSet.Remove(category);
        return Task.FromResult(category);
    }

    public async Task<IList<Category>> GetAllAsync(int userId)
    {
        return await dbSet
          .Where(a => a.UserId == userId)
          .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id, int userId)
    {
        return await dbSet
           .Where(a => a.Id == id && a.UserId == userId)
           .FirstOrDefaultAsync();
    }

    public Task<Category> UpdateAsync(Category category)
    {
        dbSet.Attach(category);
        return Task.FromResult(category);
    }
}

