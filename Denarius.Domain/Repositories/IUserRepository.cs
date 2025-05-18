using Denarius.Domain.Models;

namespace Denarius.Domain.Repositories;

public interface IUserRepository
{
    Task<User> CreateAsync(User user);
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(int id);
}
