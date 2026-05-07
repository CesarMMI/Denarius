using Denarius.Application.Inputs.Categories;
using Denarius.Application.Outputs.Categories;

namespace Denarius.Application.Interfaces.UseCases.Categories;

public interface ICreateCategoryUseCase : IUseCase<CreateCategoryInput, Task<CategoryOutput>>;
