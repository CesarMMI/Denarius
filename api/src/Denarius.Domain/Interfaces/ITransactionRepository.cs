using Denarius.Domain.Entities;
using Denarius.Domain.ValueObjects;

namespace Denarius.Domain.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    IEnumerable<Transaction> FindByAccount(Identifier accountId);
    Task<IEnumerable<Transaction>> FindByAccountAsync(Identifier accountId);
    
    IEnumerable<Transaction> FindByTag(Identifier tagId);
    Task<IEnumerable<Transaction>> FindByTagAsync(Identifier tagId);
}