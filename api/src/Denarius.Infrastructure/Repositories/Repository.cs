using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Repositories;

internal abstract class Repository<T>(DbContext context) : IRepository<T> where T : Entity
{
    bool disposed = false;
    DbSet<T> DbSet => context.Set<T>();

    #region Add
    public T Add(T entity)
    {
        return DbSet.Add(entity).Entity;
    }

    public Task<T> AddAsync(T entity)
    {
        return DbSet.AddAsync(entity).AsTask().ContinueWith(x => x.Result.Entity);
    }

    public void AddBatch(IEnumerable<T> entities)
    {
        DbSet.AddRange(entities);
    }

    public Task AddBatchAsync(IEnumerable<T> entities)
    {
        return DbSet.AddRangeAsync(entities);
    }
    #endregion
    #region Delete
    public T Delete(T entity)
    {
        return DbSet.Remove(entity).Entity;
    }

    public void DeleteBatch(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
    }
    #endregion
    #region Find
    public IQueryable<T> Find()
    {
        return DbSet.AsNoTracking();
    }

    public T? FindById(Guid id)
    {
        return Find().Where(x => x.Id == id).FirstOrDefault();
    }

    public Task<T?> FindByIdAsync(Guid id)
    {
        return Find().Where(x => x.Id == id).FirstOrDefaultAsync();
    }
    #endregion
    #region Save
    public void Save()
    {
        context.SaveChanges();
    }

    public Task SaveAsync()
    {
        return context.SaveChangesAsync();
    }
    #endregion
    #region Update
    public T Update(T entity)
    {
        return DbSet.Update(entity).Entity;
    }

    public void UpdateBatch(IEnumerable<T> entities)
    {
        DbSet.UpdateRange(entities);
    }
    #endregion

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed && disposing) context.Dispose();
        disposed = true;
    }
}
