using Denarius.Application.Domain.Queries.Accounts;
using Denarius.Application.Domain.Results.Accounts;

namespace Denarius.Application.Domain.Commands.Accounts;

public interface ICreateAccountCommand : ICommand<CreateAccountQuery, AccountResult>
{
}
