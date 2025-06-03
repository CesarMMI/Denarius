using Denarius.Application.Categories.Results;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Categories.Commands.Create;

public class CreateCategoryCommand(ICategoryRepository categoryRepository) : ICreateCategoryCommand
{
    public async Task<CategoryResult> Execute(CreateCategoryQuery query)
    {
        query.Validate();

        var category = new Category
        {
            Name = query.Name,
            Color = query.Color,
            Type = query.Type,
            UserId = query.UserId
        };

        category = await categoryRepository.CreateAsync(category);

        return category.ToCategoryResult();
    }
}
