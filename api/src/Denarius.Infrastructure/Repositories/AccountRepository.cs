using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces;
using Denarius.Infrastructure.Contexts;

namespace Denarius.Infrastructure.Repositories;

internal class TagRepository(AppDbContext context) : Repository<Tag>(context), ITagRepository
{
}
