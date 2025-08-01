using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Infrastructure.Persistence.Ef.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Persistence.Ef.Repositories;

internal class EfUserRepository(EfAppDbContext context) : IUserRepository
{
    private readonly DbSet<User> dbSet = context.Set<User>();

    public async Task<User> CreateAsync(User user)
    {
        await dbSet.AddAsync(user);
        return user;
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> FindByIdAsync(int id)
    {
        return await dbSet.FirstOrDefaultAsync(u => u.Id == id);
    }
}
