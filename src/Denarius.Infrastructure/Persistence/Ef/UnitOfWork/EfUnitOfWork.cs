using Denarius.Domain.UnitOfWork;
using Denarius.Infrastructure.Persistence.Ef.AppDbContext;
using Microsoft.EntityFrameworkCore.Storage;

namespace Denarius.Infrastructure.Persistence.Ef.UnitOfWork;

internal class EfUnitOfWork(EfAppDbContext context) : IUnitOfWork
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
        if (transaction is null) return;
        context.SaveChanges();
        transaction.Commit();
    }
    public async Task CommitAsync()
    {
        if (transaction is null) return;
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public void Rollback()
    {
        if (transaction is null) return;
        transaction.Rollback();
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
