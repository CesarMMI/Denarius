using Denarius.Application.Inputs.Categories;
using Denarius.Application.Interfaces.UseCases.Categories;
using Denarius.Application.Outputs.Categories;
using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Categories;

public class CreateCategoryUseCase(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : ICreateCategoryUseCase
{
    public async Task<CategoryOutput> Execute(CreateCategoryInput input)
    {
        var category = new Category(input.UserId, input.Name, input.Color, input.Type);
        await categoryRepository.AddAsync(category);
        await unitOfWork.CommitAsync();
        return CategoryOutput.FromEntity(category);
    }
}
