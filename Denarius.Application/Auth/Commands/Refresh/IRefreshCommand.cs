using Denarius.Application.Auth.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Auth.Commands.Refresh;

public interface IRefreshCommand : ICommand<RefreshQuery, AuthResult>
{
}
