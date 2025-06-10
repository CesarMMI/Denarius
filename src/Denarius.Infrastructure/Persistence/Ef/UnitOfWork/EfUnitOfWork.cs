using Denarius.Application.Shared.UnitOfWork;
using Microsoft.EntityFrameworkCore.Storage;

namespace Denarius.Infrastructure.Persistence.Ef.UnitOfWork;

internal class EfUnitOfWork(EfDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? transaction;

    public void BeginTransaction()
    {
        transaction = context.Database.BeginTransaction();
    }
    public async Task BeginTransactionAsync()
    {
        transaction = await context.Database.BeginTransactionAsync();
    }

    public void Commit()
    {
        transaction?.Commit();
    }
    public async Task CommitAsync()
    {
        if (transaction is null) return;
        await transaction.CommitAsync();
    }

    public void Rollback()
    {
        transaction?.Rollback();
    }
    public async Task RollbackAsync()
    {
        if (transaction is null) return;
        await transaction.RollbackAsync();
    }

    public void Dispose()
    {
        transaction?.Dispose();
        context.Dispose();
        GC.SuppressFinalize(this); // Fix for CA1816  
    }
}
