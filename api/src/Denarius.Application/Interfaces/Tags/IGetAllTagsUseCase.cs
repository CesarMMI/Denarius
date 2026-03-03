using Denarius.Application.Commands;
using Denarius.Application.Results;

namespace Denarius.Application.Interfaces.Tags;

public interface IGetAllTagsUseCase : IUseCase<Command, IEnumerable<TagResult>>
{
}
