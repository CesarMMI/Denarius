using Denarius.Domain.Models;

namespace Denarius.Domain.Repositories;

public interface IAccountRepository
{
    Task<Account> CreateAsync(Account account);
    Task<Account> DeleteAsync(Account account);
    Task<IList<Account>> GetAllAsync(int userId);
    Task<Account?> GetByIdAsync(int id, int userId);
    Task<Account> UpdateAsync(Account account);
}
