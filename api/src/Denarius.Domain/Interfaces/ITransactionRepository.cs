using Denarius.Domain.Entities;
using Denarius.Domain.ValueObjects;

namespace Denarius.Domain.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    IEnumerable<Transaction> FindByAccount(Guid accountId);
    Task<IEnumerable<Transaction>> FindByAccountAsync(Guid accountId);
    
    IEnumerable<Transaction> FindByTag(Guid tagId);
    Task<IEnumerable<Transaction>> FindByTagAsync(Guid tagId);
}