using Denarius.Application.Domain.Commands.Categories;
using Denarius.Application.Domain.Queries.Categories;
using Denarius.Application.Domain.Results.Categories;
using Denarius.Application.Exceptions;
using Denarius.Application.Extensions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Commands.Categories;

internal class DeleteCategoryCommand(ICategoryRepository categoryRepository) : Command<DeleteCategoryQuery, CategoryResult>, IDeleteCategoryCommand
{
    protected override async Task<CategoryResult> Handle(DeleteCategoryQuery query)
    {
        var category = await categoryRepository.FindOneAsync(cat => cat.Id == query.Id && cat.UserId == query.UserId);
        if (category is null) throw new NotFoundException("Category not found");

        category = categoryRepository.Delete(category);
        return category.ToResult();
    }

    protected override void Validate(DeleteCategoryQuery query)
    {
        if (!query.UserId.IsValidId()) throw new BadRequestException("User id is required");
        if (!query.Id.IsValidId()) throw new BadRequestException("Category id is required");
    }
}
