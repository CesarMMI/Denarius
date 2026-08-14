using Denarius.Domain.Entities;

namespace Denarius.Domain.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetAllAsync(string? name, string? color);
}
