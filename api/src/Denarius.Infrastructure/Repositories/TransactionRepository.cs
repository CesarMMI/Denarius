using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces;
using Denarius.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Repositories;

internal class TransactionRepository(AppDbContext context) : Repository<Transaction>(context), ITransactionRepository
{
    public IEnumerable<Transaction> FindByAccount(Guid accountId) => Find()
        .Where(t => t.Account.Id == accountId).ToList();

    public Task<IEnumerable<Transaction>> FindByAccountAsync(Guid accountId) => Find()
        .Where(t => t.Account.Id == accountId).ToListAsync()
        .ContinueWith(t => (IEnumerable<Transaction>)t.Result);

    public IEnumerable<Transaction> FindByTag(Guid tagId) => Find()
        .Where(t => t.Tag != null && t.Tag.Id == tagId).ToList();

    public Task<IEnumerable<Transaction>> FindByTagAsync(Guid tagId) => Find()
        .Where(t => t.Tag != null && t.Tag.Id == tagId).ToListAsync()
        .ContinueWith(t => (IEnumerable<Transaction>)t.Result);
}
