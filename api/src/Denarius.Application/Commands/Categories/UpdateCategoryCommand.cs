using Denarius.Application.Domain.Commands.Categories;
using Denarius.Application.Domain.Queries.Categories;
using Denarius.Application.Domain.Results.Categories;
using Denarius.Application.Extensions;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Repositories;
using Denarius.Domain.UnitOfWork;

namespace Denarius.Application.Commands.Categories;

internal class UpdateCategoryCommand(
    IUnitOfWork unitOfWork,
    ICategoryRepository categoryRepository
) : IUpdateCategoryCommand
{
    public async Task<CategoryResult> Execute(UpdateCategoryQuery query)
    {
        query.Validate();

        var category = await categoryRepository.FindOneAsync(cat => cat.Id == query.Id && cat.UserId == query.UserId);
        if (category is null) throw new NotFoundException("Category not found");

        category.Name = query.Name;
        category.Color = query.Color;

        await unitOfWork.BeginTransactionAsync();
        try
        {
            category = categoryRepository.Update(category);
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
