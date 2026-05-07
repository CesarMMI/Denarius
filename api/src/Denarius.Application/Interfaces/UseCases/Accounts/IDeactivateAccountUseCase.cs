using Denarius.Application.Inputs.Accounts;

namespace Denarius.Application.Interfaces.UseCases.Accounts;

public interface IDeactivateAccountUseCase : IUseCase<DeactivateAccountInput, Task>;
