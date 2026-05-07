using Denarius.Domain.Entities;
using Denarius.Domain.Enums;

namespace Denarius.Domain.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<Category>> ListByUserAsync(Guid userId, CategoryType? type);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Category category);
    Task NullifyTransactionCategoriesAsync(Guid categoryId);
}
