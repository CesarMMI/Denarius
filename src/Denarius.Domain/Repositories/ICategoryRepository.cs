using Denarius.Domain.Models;

namespace Denarius.Domain.Repositories;

public interface ICategoryRepository
{
    Task<Category> CreateAsync(Category category);
    Task<Category> DeleteAsync(Category category);
    Task<IList<Category>> GetAllAsync(int userId);
    Task<Category?> GetByIdAsync(int id, int userId);
    Task<Category> UpdateAsync(Category category);
}
