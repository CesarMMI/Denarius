using Denarius.Application.Categories.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Categories.Commands.Update;

public interface IUpdateCategoryCommand : ICommand<UpdateCategoryQuery, CategoryResult> { }
