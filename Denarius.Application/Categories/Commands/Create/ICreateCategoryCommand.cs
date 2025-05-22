using Denarius.Application.Categories.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Categories.Commands.Create;

public interface ICreateCategoryCommand : ICommand<CreateCategoryQuery, CategoryResult> { }
