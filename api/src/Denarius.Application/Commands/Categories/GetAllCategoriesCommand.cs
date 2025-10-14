using Denarius.Application.Domain.Commands.Categories;
using Denarius.Application.Domain.Queries.Categories;
using Denarius.Application.Domain.Results.Categories;
using Denarius.Application.Exceptions;
using Denarius.Application.Extensions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Commands.Categories;

internal class GetAllCategoriesCommand(ICategoryRepository categoryRepository) : Command<GetAllCategoriesQuery, IEnumerable<CategoryResult>>, IGetAllCategoriesCommand
{
    protected override async Task<IEnumerable<CategoryResult>> Handle(GetAllCategoriesQuery query)
    {
        var categories = await categoryRepository.FindManyAsync(cat => cat.UserId == query.UserId);
        return categories.Select(a => a.ToResult());
    }

    protected override void Validate(GetAllCategoriesQuery query)
    {
        if (!query.UserId.IsValidId()) throw new BadRequestException("User id is required");
    }
}
