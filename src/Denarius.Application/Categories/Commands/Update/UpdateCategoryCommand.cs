using Denarius.Application.Categories.Results;
using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.UnitOfWork;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Categories.Commands.Update;

public class UpdateCategoryCommand(
    IUnitOfWork unitOfWork,
    ICategoryRepository categoryRepository
) : IUpdateCategoryCommand
{
    public async Task<CategoryResult> Execute(UpdateCategoryQuery query)
    {
        query.Validate();

        var category = await categoryRepository.GetByIdAsync(query.Id, query.UserId);
        if (category is null) throw new NotFoundException("Category not found");

        category.Name = query.Name;
        category.Color = query.Color;

        await unitOfWork.BeginTransactionAsync();
        try
        {
            category = await categoryRepository.UpdateAsync(category);
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
