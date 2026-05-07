using Denarius.Application.Inputs.Accounts;
using Denarius.Application.Outputs.Accounts;

namespace Denarius.Application.Interfaces.UseCases.Accounts;

public interface ICreateAccountUseCase : IUseCase<CreateAccountInput, Task<AccountOutput>>;
