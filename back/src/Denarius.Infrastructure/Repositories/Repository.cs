using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using Denarius.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Repositories;

public class Repository<T>(DenariusDbContext context) : IRepository<T> where T : Entity
{
    protected readonly DenariusDbContext Context = context;

    public T? GetById(Guid id) => Context.Set<T>().Find(id);

    public async Task<T?> GetByIdAsync(Guid id) => await Context.Set<T>().FindAsync(id);

    public void Add(T entity) => Context.Set<T>().Add(entity);

    public async Task AddAsync(T entity) => await Context.Set<T>().AddAsync(entity);

    public void Update(T entity) => Context.Set<T>().Update(entity);

    public Task UpdateAsync(T entity)
    {
        Context.Set<T>().Update(entity);
        return Task.CompletedTask;
    }

    public void Delete(T entity) => Context.Set<T>().Remove(entity);

    public Task DeleteAsync(T entity)
    {
        Context.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }
}
