using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Repositories;

internal class AccountRepository(DbContext context) : Repository<Account>(context.Set<Account>()), IAccountRepository
{
}
