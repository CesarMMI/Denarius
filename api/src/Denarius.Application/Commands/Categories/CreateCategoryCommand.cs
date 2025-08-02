using Denarius.Application.Domain.Commands.Categories;
using Denarius.Application.Domain.Queries.Categories;
using Denarius.Application.Domain.Results.Categories;
using Denarius.Application.Extensions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Domain.UnitOfWork;

namespace Denarius.Application.Commands.Categories;

internal class CreateCategoryCommand(
    IUnitOfWork unitOfWork, 
    ICategoryRepository categoryRepository
) : ICreateCategoryCommand
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

        await unitOfWork.BeginTransactionAsync();
        try
        {
            category = await categoryRepository.CreateAsync(category);
            await unitOfWork.CommitAsync();
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }

        return category.ToResult();
    }
}
