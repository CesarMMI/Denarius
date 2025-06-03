using Denarius.Application.Auth.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Auth.Commands.Login;

public interface ILoginCommand : ICommand<LoginQuery, AuthResult>
{
}
