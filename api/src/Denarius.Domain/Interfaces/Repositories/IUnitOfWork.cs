namespace Denarius.Domain.Interfaces.Repositories;

public interface IUnitOfWork
{
    Task CommitAsync();
}
