using Denarius.Application.Exceptions;
using Denarius.Application.IO.Categories;
using Denarius.Domain.Repositories;
using Denarius.Domain.ValueObjects;

namespace Denarius.Application.UseCases.Categories.Update;

internal class UpdateCategoryUseCase(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork) : IUpdateCategoryUseCase
{
    public async Task<CategoryOutput> Execute((Guid Id, UpdateCategoryInput Input) input)
    {
        var category = await categoryRepository.GetByIdAsync(input.Id)
            ?? throw new NotFoundException("Categoria não encontrada.");

        category.Update(input.Input.Name, new Color(input.Input.Color));

        await categoryRepository.UpdateAsync(category);
        await unitOfWork.SaveChangesAsync();

        return new CategoryOutput(category);
    }
}
