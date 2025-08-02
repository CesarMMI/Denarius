using Denarius.Domain.Repositories;
using Denarius.Infrastructure.Persistence.Ef.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Denarius.Infrastructure.Persistence.Ef.Repositories;

internal abstract class EfRepository<T> : IRepository<T> where T : class
{
    protected EfAppDbContext DbContext { get; private set; }
    protected DbSet<T> DbSet => DbContext.Set<T>();

    protected EfRepository(EfAppDbContext context)
    {
        DbContext = context;
    }

    public IQueryable<T> Query()
    {
        return DbSet.AsQueryable<T>();
    }

    public T? FindOne(Expression<Func<T, bool>> predicate)
    {
        return DbSet.Where(predicate).FirstOrDefault<T>();
    }

    public IEnumerable<T> FindMany(Expression<Func<T, bool>> predicate)
    {
        return DbSet.Where(predicate).ToList<T>();
    }

    public async Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.Where(predicate).FirstOrDefaultAsync<T>();
    }

    public async Task<IEnumerable<T>> FindManyAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.Where(predicate).ToListAsync();
    }

    public T Create(T entity)
    {
        DbSet.Add(entity);
        return entity;
    }

    public async Task<T> CreateAsync(T entity)
    {
        await DbSet.AddAsync(entity);
        return entity;
    }

    public T Update(T entity)
    {
        DbSet.Attach(entity);
        return entity;
    }

    public T Delete(T entity)
    {
        DbSet.Remove(entity);
        return entity;
    }
}
