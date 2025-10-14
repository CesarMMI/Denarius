using Denarius.Application.Domain.Commands.Categories;
using Denarius.Application.Domain.Queries.Categories;
using Denarius.Application.Domain.Results.Categories;
using Denarius.Application.Exceptions;
using Denarius.Application.Extensions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Commands.Categories;

internal class CreateCategoryCommand(ICategoryRepository categoryRepository) : Command<CreateCategoryQuery, CategoryResult>, ICreateCategoryCommand
{
    protected override async Task<CategoryResult> Handle(CreateCategoryQuery query)
    {
        var category = new Category
        {
            Name = query.Name,
            Color = query.Color,
            Type = query.Type,
            UserId = query.UserId
        };
        category = await categoryRepository.CreateAsync(category);

        return category.ToResult();
    }

    protected override void Validate(CreateCategoryQuery query)
    {
        if (!query.UserId.IsValidId()) throw new BadRequestException("User id is required");

        if (!query.Name.IsValidString()) throw new BadRequestException("Name is required");
        if (query.Name.Length < 3) throw new BadRequestException("Name length can't be lower than 3");
        if (query.Name.Length > 50) throw new BadRequestException("Name length can't be greater than 50");

        if (query.Color is not null && !query.Color.IsValidColor()) throw new BadRequestException("Invalid color");

        if (!query.Type.IsValidEnum()) throw new BadRequestException("Invalid type");
    }
}
