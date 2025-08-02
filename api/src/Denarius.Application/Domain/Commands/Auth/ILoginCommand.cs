using Denarius.Application.Domain.Queries.Auth;
using Denarius.Application.Domain.Results.Auth;

namespace Denarius.Application.Domain.Commands.Auth;

public interface ILoginCommand : ICommand<LoginQuery, AuthResult>
{
}
