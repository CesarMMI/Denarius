using Denarius.Application.Commands.Accounts;
using Denarius.Application.Results;

namespace Denarius.Application.Interfaces.Accounts;

public interface IUpdateAccountUseCase : IUseCase<UpdateAccountCommand, Task<AccountResult>>
{
}