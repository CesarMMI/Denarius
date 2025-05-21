using Denarius.Application.Accounts.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Accounts.Commands.Create;

public interface ICreateAccountCommand : ICommand<CreateAccountQuery, AccountResult>
{
}
