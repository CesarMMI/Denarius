using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Repositories;

public class CategoryRepository(DenariusDbContext context) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(Guid id, Guid userId) =>
        await context.Categories
                     .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

    public async Task<IEnumerable<Category>> ListByUserAsync(Guid userId, CategoryType? type)
    {
        var query = context.Categories.Where(c => c.UserId == userId);

        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task AddAsync(Category category) =>
        await context.Categories.AddAsync(category);

    public Task UpdateAsync(Category category)
    {
        context.Categories.Update(category);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Category category)
    {
        context.Categories.Remove(category);
        return Task.CompletedTask;
    }

    public async Task NullifyTransactionCategoriesAsync(Guid categoryId) =>
        await context.Transactions
                     .Where(t => t.CategoryId == categoryId)
                     .ExecuteUpdateAsync(s => s.SetProperty(t => t.CategoryId, (Guid?)null));
}
