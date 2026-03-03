using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Infrastructure.Repositories;

internal class TagRepository(DbContext context) : Repository<Tag>(context.Set<Tag>()), ITagRepository
{
}
