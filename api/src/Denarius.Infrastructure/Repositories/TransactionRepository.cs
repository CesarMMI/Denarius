using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces;
using Denarius.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Repositories;

internal class TransactionRepository(AppDbContext context) : Repository<Transaction>(context), ITransactionRepository
{
    public IEnumerable<Transaction> FindByAccount(Guid accountId) => Find()
        .Where(t => t.AccountId == accountId).ToList();

    public Task<IEnumerable<Transaction>> FindByAccountAsync(Guid accountId) => Find()
        .Where(t => t.AccountId == accountId).ToListAsync()
        .ContinueWith(t => (IEnumerable<Transaction>)t.Result);

    public IEnumerable<Transaction> FindByTag(Guid tagId) => Find()
        .Where(t => t.TagId != null && t.TagId == tagId).ToList();

    public Task<IEnumerable<Transaction>> FindByTagAsync(Guid tagId) => Find()
        .Where(t => t.TagId != null && t.TagId == tagId).ToListAsync()
        .ContinueWith(t => (IEnumerable<Transaction>)t.Result);
}
