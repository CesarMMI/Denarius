using Denarius.Domain.Entities;
using Denarius.Domain.Enums;

namespace Denarius.Domain.Interfaces.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<Transaction>> ListByUserAsync(Guid userId, Guid? accountId, Guid? categoryId, TransactionType? type, DateTime? startDate, DateTime? endDate);
    Task AddAsync(Transaction transaction);
    Task AddRangeAsync(IEnumerable<Transaction> transactions);
    Task UpdateAsync(Transaction transaction);
    Task UpdateRangeAsync(IEnumerable<Transaction> transactions);
    Task DeleteAsync(Transaction transaction);
    Task DeleteRangeAsync(IEnumerable<Transaction> transactions);
}
