using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces;
using Denarius.Infrastructure.Contexts;

namespace Denarius.Infrastructure.Repositories;

internal class AccountRepository(AppDbContext context) : Repository<Account>(context), IAccountRepository
{
}
