using Denarius.Application.Inputs.Accounts;
using Denarius.Application.Outputs.Accounts;

namespace Denarius.Application.Interfaces.UseCases.Accounts;

public interface IUpdateAccountUseCase : IUseCase<UpdateAccountInput, Task<AccountOutput>>;
