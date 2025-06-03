using Denarius.Application.Accounts.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Accounts.Commands.Update;

public interface IUpdateAccountCommand : ICommand<UpdateAccountQuery, AccountResult>
{
}
