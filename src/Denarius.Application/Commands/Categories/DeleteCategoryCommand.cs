using Denarius.Application.Domain.Commands.Categories;
using Denarius.Application.Domain.Queries.Categories;
using Denarius.Application.Domain.Results.Categories;
using Denarius.Application.Extensions;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Repositories;
using Denarius.Domain.UnitOfWork;

namespace Denarius.Application.Commands.Categories;

internal class DeleteCategoryCommand(
    IUnitOfWork unitOfWork,
    ICategoryRepository categoryRepository
) : IDeleteCategoryCommand
{
    public async Task<CategoryResult> Execute(DeleteCategoryQuery query)
    {
        query.Validate();

        var category = await categoryRepository.FindOneAsync(cat => cat.Id == query.Id && cat.UserId == query.UserId);
        if (category is null) throw new NotFoundException("Category not found");

        await unitOfWork.BeginTransactionAsync();
        try
        {
            category = categoryRepository.Delete(category);
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
