using Denarius.Domain.Entities;
using Denarius.Domain.ValueObjects;

namespace Denarius.Domain.Interfaces;

public interface IRepository<T> where T : Entity
{
    IQueryable<T> Find();
    T FindById(Identifier id);
    Task<T> FindByIdAsync(Identifier id);
    
    T Add(T entity);
    Task<T> AddAsync(T entity);
    
    IEnumerable<T> AddBatch(IEnumerable<T> entity);
    Task<IEnumerable<T>> AddBatchAsync(IEnumerable<T> entity);
    
    T Update(T entity);
    Task<T> UpdateAsync(T entity);
    
    IEnumerable<T> UpdateBatch(IEnumerable<T> entity);
    Task<IEnumerable<T>> UpdateBatchAsync(IEnumerable<T> entity);
    
    bool Delete(Identifier id);
    Task<bool> DeleteAsync(Identifier id);
    
    IEnumerable<bool> DeleteBatch(IEnumerable<Identifier> id);
    Task<IEnumerable<bool>> DeleteBatchAsync(IEnumerable<Identifier> id);
}
