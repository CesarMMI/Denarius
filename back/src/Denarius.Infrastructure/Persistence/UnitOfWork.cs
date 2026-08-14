using Denarius.Domain.Repositories;

namespace Denarius.Infrastructure.Persistence;

public class UnitOfWork(DenariusDbContext context) : IUnitOfWork
{
    public void SaveChanges() => context.SaveChanges();

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
