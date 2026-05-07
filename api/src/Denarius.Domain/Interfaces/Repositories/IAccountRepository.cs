using Denarius.Domain.Entities;

namespace Denarius.Domain.Interfaces.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<Account>> ListByUserAsync(Guid userId);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
}
