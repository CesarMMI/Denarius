using Denarius.Domain.Models;

namespace Denarius.Domain.Repositories;

public interface ITransactionRepository
{
    Task<Transaction> CreateAsync(Transaction transaction);
    Task<Transaction> DeleteAsync(Transaction transaction);
    Task<IList<Transaction>> GetAllAsync(int userId);
    Task<Transaction?> GetByIdAsync(int id, int userId);
    Task<Transaction> UpdateAsync(Transaction transaction);
}
