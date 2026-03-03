using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces;
using Denarius.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Repositories;

internal abstract class Repository<T>(DbSet<T> context) : IRepository<T> where T : Entity
{
    public T Add(T entity) => context.Add(entity).Entity;

    public Task<T> AddAsync(T entity) => context.AddAsync(entity).AsTask().ContinueWith(x => x.Result.Entity);

    public void AddBatch(IEnumerable<T> entities) => context.AddRange(entities);

    public Task AddBatchAsync(IEnumerable<T> entities) => context.AddRangeAsync(entities);

    public T Delete(T entity) => context.Remove(entity).Entity;

    public void DeleteBatch(IEnumerable<T> entities) => context.RemoveRange(entities);

    public IQueryable<T> Find() => context.AsNoTracking();

    public T? FindById(Identifier id) => context.AsNoTracking().Where(x => x.Id == id).FirstOrDefault();

    public Task<T?> FindByIdAsync(Identifier id) => context.AsNoTracking().Where(x => x.Id == id).FirstOrDefaultAsync();

    public T Update(T entity) => context.Update(entity).Entity;

    public void UpdateBatch(IEnumerable<T> entities) => context.UpdateRange(entities);
}
