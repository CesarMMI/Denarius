using Denarius.Application.Categories.Results;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Categories.Commands.Delete;

public class DeleteCategoryCommand(ICategoryRepository categoryRepository) : IDeleteCategoryCommand
{
    public async Task<CategoryResult> Execute(DeleteCategoryQuery query)
    {
        query.Validate();

        var category = await categoryRepository.GetByIdAsync(query.Id, query.UserId);

        if (category is null)
        {
            throw new NotFoundException("Category not found");
        }

        category = await categoryRepository.DeleteAsync(category);

        return category.ToCategoryResult();
    }
}
