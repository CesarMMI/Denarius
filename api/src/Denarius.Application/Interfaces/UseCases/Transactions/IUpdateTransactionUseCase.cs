using Denarius.Application.Inputs.Transactions;
using Denarius.Application.Outputs.Transactions;

namespace Denarius.Application.Interfaces.UseCases.Transactions;

public interface IUpdateTransactionUseCase : IUseCase<UpdateTransactionInput, Task<TransactionOutput>>;
