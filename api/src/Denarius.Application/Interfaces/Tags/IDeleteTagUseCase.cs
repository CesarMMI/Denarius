using Denarius.Application.Commands;
using Denarius.Application.Results;

namespace Denarius.Application.Interfaces.Tags;

public interface IDeleteTagUseCase : IUseCase<IdCommand, Task<TagResult>>
{
}
