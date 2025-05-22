using Denarius.Application.Categories.Results;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Categories.Commands.GetById;

internal class GetCategoryByIdCommand(ICategoryRepository categoryRepository) : IGetCategoryByIdCommand
{
    public async Task<CategoryResult> Execute(GetCategoryByIdQuery query)
    {
        query.Validate();

        var category = await categoryRepository.GetByIdAsync(query.Id, query.UserId);

        if(category is null)
        {
            throw new NotFoundException("Category not found");
        }

        return category.ToCategoryResult();
    }
}
