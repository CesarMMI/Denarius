using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Infrastructure.Persistence.Ef.AppDbContext;

namespace Denarius.Infrastructure.Persistence.Ef.Repositories;

internal class EfAccountRepository(EfAppDbContext context) : EfRepository<Account>(context), IAccountRepository
{
}