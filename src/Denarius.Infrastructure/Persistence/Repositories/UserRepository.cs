using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Repositories;

internal class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User> CreateAsync(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> FindByIdAsync(int id)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }
}
