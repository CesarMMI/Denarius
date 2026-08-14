namespace Denarius.Domain.Repositories;

public interface IUnitOfWork
{
    void SaveChanges();
    Task SaveChangesAsync();
}
