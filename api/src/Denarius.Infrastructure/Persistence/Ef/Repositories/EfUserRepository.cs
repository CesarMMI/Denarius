using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Infrastructure.Persistence.Ef.AppDbContext;

namespace Denarius.Infrastructure.Persistence.Ef.Repositories;

internal class EfUserRepository(EfAppDbContext context) : EfRepository<User>(context), IUserRepository
{
}
