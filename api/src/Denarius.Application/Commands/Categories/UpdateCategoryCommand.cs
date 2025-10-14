using Denarius.Application.Domain.Commands.Categories;
using Denarius.Application.Domain.Queries.Categories;
using Denarius.Application.Domain.Results.Categories;
using Denarius.Application.Exceptions;
using Denarius.Application.Extensions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Commands.Categories;

internal class UpdateCategoryCommand(ICategoryRepository categoryRepository) : Command<UpdateCategoryQuery, CategoryResult>, IUpdateCategoryCommand
{
    protected override async Task<CategoryResult> Handle(UpdateCategoryQuery query)
    {
        var category = await categoryRepository.FindOneAsync(cat => cat.Id == query.Id && cat.UserId == query.UserId);
        if (category is null) throw new NotFoundException("Category not found");

        category.Name = query.Name;
        category.Color = query.Color;
        category = categoryRepository.Update(category);

        return category.ToResult();
    }

    protected override void Validate(UpdateCategoryQuery query)
    {
        if (!query.UserId.IsValidId()) throw new BadRequestException("User id is required");
        if (!query.Id.IsValidId()) throw new BadRequestException("Category id is required");

        if (!query.Name.IsValidString()) throw new BadRequestException("Name is required");
        if (query.Name.Length < 3) throw new BadRequestException("Name length can't be lower than 3");
        if (query.Name.Length > 50) throw new BadRequestException("Name length can't be greater than 50");

        if (query.Color is not null && !query.Color.IsValidColor()) throw new BadRequestException("Invalid color");
    }
}
