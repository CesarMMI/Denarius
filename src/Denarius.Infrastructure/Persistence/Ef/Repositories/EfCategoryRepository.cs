using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Infrastructure.Persistence.Ef.AppDbContext;

namespace Denarius.Infrastructure.Persistence.Ef.Repositories;

internal class EfCategoryRepository(EfAppDbContext context) : EfRepository<Category>(context), ICategoryRepository
{
}