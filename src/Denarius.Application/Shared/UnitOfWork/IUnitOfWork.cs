namespace Denarius.Application.Shared.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    void BeginTransaction();
    Task BeginTransactionAsync();
    void Commit();
    Task CommitAsync();
    void Rollback();
    Task RollbackAsync();
}
