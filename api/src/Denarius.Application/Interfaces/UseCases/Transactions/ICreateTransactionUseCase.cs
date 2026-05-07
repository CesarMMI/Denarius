using Denarius.Application.Inputs.Transactions;
using Denarius.Application.Outputs.Transactions;

namespace Denarius.Application.Interfaces.UseCases.Transactions;

public interface ICreateTransactionUseCase : IUseCase<CreateTransactionInput, Task<TransactionOutput>>;
