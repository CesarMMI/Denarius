using Denarius.Application.Exceptions;
using Denarius.Application.IO.Categories;
using Denarius.Domain.Repositories;

namespace Denarius.Application.UseCases.Categories.GetById;

internal class GetCategoryByIdUseCase(ICategoryRepository categoryRepository) : IGetCategoryByIdUseCase
{
    public async Task<CategoryOutput> Execute(Guid input)
    {
        var category = await categoryRepository.GetByIdAsync(input)
            ?? throw new NotFoundException("Categoria não encontrada.");

        return new CategoryOutput(category);
    }
}
