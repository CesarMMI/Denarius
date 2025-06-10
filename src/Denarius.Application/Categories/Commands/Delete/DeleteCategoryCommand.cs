using Denarius.Application.Categories.Results;
using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.UnitOfWork;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Categories.Commands.Delete;

public class DeleteCategoryCommand(
    IUnitOfWork unitOfWork,
    ICategoryRepository categoryRepository
) : IDeleteCategoryCommand
{
    public async Task<CategoryResult> Execute(DeleteCategoryQuery query)
    {
        query.Validate();

        var category = await categoryRepository.GetByIdAsync(query.Id, query.UserId);

        if (category is null) throw new NotFoundException("Category not found");

        await unitOfWork.BeginTransactionAsync();
        try
        {
            category = await categoryRepository.DeleteAsync(category);
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
