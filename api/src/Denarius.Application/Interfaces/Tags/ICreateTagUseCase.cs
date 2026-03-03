using Denarius.Application.Commands.Tags;
using Denarius.Application.Results;

namespace Denarius.Application.Interfaces.Tags;

public interface ICreateTagUseCase : IUseCase<CreateTagCommand, Task<TagResult>>
{
}