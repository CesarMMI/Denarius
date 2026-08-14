using Denarius.Application.IO.Categories;
using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using Denarius.Domain.ValueObjects;

namespace Denarius.Application.UseCases.Categories.Create;

internal class CreateCategoryUseCase(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork) : ICreateCategoryUseCase
{
    public async Task<CategoryOutput> Execute(CreateCategoryInput input)
    {
        var category = new Category(input.Name, new Color(input.Color));

        await categoryRepository.AddAsync(category);
        await unitOfWork.SaveChangesAsync();

        return new CategoryOutput(category);
    }
}
