using Denarius.Application.Categories.Results;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Categories.Commands.GetAll;

internal class GetAllCategoriesCommand(ICategoryRepository categoryRepository) : IGetAllCategoriesCommand
{
    public async Task<IList<CategoryResult>> Execute(GetAllCategoriesQuery query)
    {
        query.Validate();

        var categories = await categoryRepository.GetAllAsync(query.UserId);

        return [.. categories.Select(a => a.ToCategoryResult())];
    }
}
