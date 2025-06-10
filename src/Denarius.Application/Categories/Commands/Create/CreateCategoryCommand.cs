using Denarius.Application.Categories.Results;
using Denarius.Application.Shared.UnitOfWork;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Categories.Commands.Create;

public class CreateCategoryCommand(
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

        return category.ToCategoryResult();
    }
}
