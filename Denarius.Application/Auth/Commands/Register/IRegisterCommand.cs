using Denarius.Application.Auth.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Auth.Commands.Register;

public interface IRegisterCommand : ICommand<RegisterQuery, AuthResult>
{
}
