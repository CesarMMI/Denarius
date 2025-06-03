using Denarius.Application.Shared.Command;

namespace Denarius.Application.Categories.Commands.GetTypes;

public interface IGetCategoryTypesCommand : ICommand<GetCategoryTypesQuery, IList<GetCategoryTypesResult>>
{
}
