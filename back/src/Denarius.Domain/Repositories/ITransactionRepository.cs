using Denarius.Domain.Entities;

namespace Denarius.Domain.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetAllAsync();
    Task<bool> ExistsByCategoryIdAsync(Guid categoryId);
}
