using Denarius.Application.Categories.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Categories.Commands.GetById;

public interface IGetCategoryByIdCommand : ICommand<GetCategoryByIdQuery, CategoryResult> { }
