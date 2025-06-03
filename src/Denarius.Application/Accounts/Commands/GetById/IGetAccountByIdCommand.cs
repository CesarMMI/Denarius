using Denarius.Application.Accounts.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Accounts.Commands.GetById;

public interface IGetAccountByIdCommand : ICommand<GetAccountByIdQuery, AccountResult>
{
}
