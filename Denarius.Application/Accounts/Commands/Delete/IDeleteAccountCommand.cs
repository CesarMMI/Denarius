using Denarius.Application.Accounts.Results;
using Denarius.Application.Shared.Command;

namespace Denarius.Application.Accounts.Commands.Delete;

public interface IDeleteAccountCommand : ICommand<DeleteAccountQuery, AccountResult>
{
}
