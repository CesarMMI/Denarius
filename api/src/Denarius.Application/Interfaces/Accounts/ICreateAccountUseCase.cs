using Denarius.Application.Commands.Accounts;
using Denarius.Application.Results;

namespace Denarius.Application.Interfaces.Accounts;

public interface ICreateAccountUseCase : IUseCase<CreateAccountCommand, Task<AccountResult>>
{
}