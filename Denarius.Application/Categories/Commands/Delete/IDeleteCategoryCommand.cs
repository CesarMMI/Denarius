using Denarius.Application.Categories.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Categories.Commands.Delete;

public interface IDeleteCategoryCommand : ICommand<DeleteCategoryQuery, CategoryResult> { }