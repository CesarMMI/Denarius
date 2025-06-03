using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Repositories;

internal class CategoryRepository(AppDbContext context) : ICategoryRepository
{

    public async Task<Category> CreateAsync(Category category)
    {
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task<Category> DeleteAsync(Category category)
    {
        context.Categories.Attach(category);
        context.Categories.Remove(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task<IList<Category>> GetAllAsync(int userId)
    {
        return await context.Categories
          .Where(a => a.UserId == userId)
          .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id, int userId)
    {
        return await context.Categories
           .Where(a => a.Id == id && a.UserId == userId)
           .FirstOrDefaultAsync();
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        context.Categories.Attach(category);
        await context.SaveChangesAsync();
        return category;
    }
}

