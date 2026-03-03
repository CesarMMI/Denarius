using Denarius.Application.Commands.Tags;
using Denarius.Application.Results;

namespace Denarius.Application.Interfaces.Tags;

public interface IUpdateTagUseCase : IUseCase<UpdateTagCommand, Task<TagResult>>
{
}