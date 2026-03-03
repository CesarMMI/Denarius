using Denarius.Application.Commands;
using Denarius.Application.Results;

namespace Denarius.Application.Interfaces.Accounts;

public interface IDeleteAccountUseCase : IUseCase<IdCommand, Task<AccountResult>>
{
}