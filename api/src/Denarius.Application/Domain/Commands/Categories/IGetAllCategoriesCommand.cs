using Denarius.Application.Domain.Queries.Categories;
using Denarius.Application.Domain.Results.Categories;

namespace Denarius.Application.Domain.Commands.Categories;

public interface IGetAllCategoriesCommand : ICommand<GetAllCategoriesQuery, IEnumerable<CategoryResult>> { }
