using Denarius.Application.Domain.Commands.Categories;
using Denarius.Application.Domain.Queries.Categories;
using Denarius.Application.Domain.Results.Categories;
using Denarius.Application.Extensions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Commands.Categories;

internal class GetAllCategoriesCommand(ICategoryRepository categoryRepository) : IGetAllCategoriesCommand
{
    public async Task<IEnumerable<CategoryResult>> Execute(GetAllCategoriesQuery query)
    {
        query.Validate();
        var categories = await categoryRepository.FindManyAsync(cat => cat.UserId == query.UserId);
        return categories.Select(a => a.ToResult());
    }
}
