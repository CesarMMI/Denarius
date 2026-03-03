using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces;
using Denarius.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Repositories;

internal class TransactionRepository(DbContext context) : Repository<Transaction>(context.Set<Transaction>()), ITransactionRepository
{
    public IEnumerable<Transaction> FindByAccount(Identifier accountId) => Find()
        .Where(t => t.Account.Id == accountId).ToList();

    public Task<IEnumerable<Transaction>> FindByAccountAsync(Identifier accountId) => Find()
        .Where(t => t.Account.Id == accountId).ToListAsync()
        .ContinueWith(t => (IEnumerable<Transaction>)t.Result);

    public IEnumerable<Transaction> FindByTag(Identifier tagId) => Find()
        .Where(t => t.Tag != null && t.Tag.Id == tagId).ToList();

    public Task<IEnumerable<Transaction>> FindByTagAsync(Identifier tagId) => Find()
        .Where(t => t.Tag != null && t.Tag.Id == tagId).ToListAsync()
        .ContinueWith(t => (IEnumerable<Transaction>)t.Result);
}
