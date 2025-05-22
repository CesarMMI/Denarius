using Denarius.Application.Categories.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Categories.Commands.GetAll;

public interface IGetAllCategoriesCommand : ICommand<GetAllCategoriesQuery, IList<CategoryResult>> { }
