using Denarius.Domain.Entities;
using Denarius.Domain.ValueObjects;

namespace Denarius.Domain.Interfaces;

public interface IRepository<T> where T : Entity
{
    T Add(T entity);
    Task<T> AddAsync(T entity);
    void AddBatch(IEnumerable<T> entities);
    Task AddBatchAsync(IEnumerable<T> entities);
    T Delete(T entity);
    void DeleteBatch(IEnumerable<T> entities);
    IQueryable<T> Find();
    T? FindById(Identifier id);
    Task<T?> FindByIdAsync(Identifier id);    
    T Update(T entity);
    void UpdateBatch(IEnumerable<T> entities);
}
