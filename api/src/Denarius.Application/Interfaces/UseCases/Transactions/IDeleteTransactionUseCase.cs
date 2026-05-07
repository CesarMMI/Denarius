using Denarius.Application.Inputs.Transactions;

namespace Denarius.Application.Interfaces.UseCases.Transactions;

public interface IDeleteTransactionUseCase : IUseCase<DeleteTransactionInput, Task>;
