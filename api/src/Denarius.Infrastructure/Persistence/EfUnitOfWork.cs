using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Infrastructure.Persistence;

public class EfUnitOfWork(DenariusDbContext context) : IUnitOfWork
{
    public async Task CommitAsync() =>
        await context.SaveChangesAsync();
}
