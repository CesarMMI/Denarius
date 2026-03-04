using Denarius.Domain.Entities;

namespace Denarius.Domain.Interfaces;

public interface IRepository<T> : IDisposable where T : Entity
{
    T Add(T entity);
    Task<T> AddAsync(T entity);
    void AddBatch(IEnumerable<T> entities);
    Task AddBatchAsync(IEnumerable<T> entities);

    T Delete(T entity);
    void DeleteBatch(IEnumerable<T> entities);
    
    IQueryable<T> Find();
    T? FindById(Guid id);
    Task<T?> FindByIdAsync(Guid id);

    void Save();
    Task SaveAsync();

    T Update(T entity);
    void UpdateBatch(IEnumerable<T> entities);
}
