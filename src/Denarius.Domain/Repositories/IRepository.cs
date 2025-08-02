using System.Linq.Expressions;

namespace Denarius.Domain.Repositories;

public interface IRepository<T> where T : class
{
    IQueryable<T> Query();
    T? FindOne(Expression<Func<T, bool>> predicate);
    IEnumerable<T> FindMany(Expression<Func<T, bool>> predicate);
    Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> FindManyAsync(Expression<Func<T, bool>> predicate);
    T Create(T category);
    Task<T> CreateAsync(T category);
    T Delete(T category);
    T Update(T category);
}
