using Denarius.Application.Exceptions.Categories;
using Denarius.Application.Inputs.Categories;
using Denarius.Application.Interfaces.UseCases.Categories;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Categories;

public class DeleteCategoryUseCase(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : IDeleteCategoryUseCase
{
    public async Task Execute(DeleteCategoryInput input)
    {
        var category = await categoryRepository.GetByIdAsync(input.CategoryId, input.UserId);

        if (category is null)
            throw new CategoryNotFoundException(input.CategoryId);

        await categoryRepository.NullifyTransactionCategoriesAsync(category.Id);
        await categoryRepository.DeleteAsync(category);
        await unitOfWork.CommitAsync();
    }
}
