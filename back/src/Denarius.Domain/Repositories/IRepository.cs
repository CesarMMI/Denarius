using Denarius.Domain.Entities;

namespace Denarius.Domain.Repositories;

public interface IRepository<T> where T : Entity
{
    T? GetById(Guid id);
    Task<T?> GetByIdAsync(Guid id);

    void Add(T entity);
    Task AddAsync(T entity);

    void Update(T entity);
    Task UpdateAsync(T entity);

    void Delete(T entity);
    Task DeleteAsync(T entity);
}
