using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Repositories;

public class UserRepository(DenariusDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id) =>
        await context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<bool> ExistsByEmailAsync(string email) =>
        await context.Users.AnyAsync(u => u.Email == email);

    public async Task AddAsync(User user) =>
        await context.Users.AddAsync(user);
}
