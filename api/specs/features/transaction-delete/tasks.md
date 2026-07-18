# transaction-delete — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Input/Interface do use case (`DeleteTransactionInput`, `IDeleteTransactionUseCase : IUseCase<DeleteTransactionInput, Task>`)
      Verificação: `dotnet build` compila sem erros.
- [x] `DeleteTransactionUseCase` busca a transação via `ITransactionRepository.GetByIdAsync(transactionId, userId)`,
      lança `TransactionNotFoundException` quando `null`
      Verificação: `tests/Denarius.Application.Tests/UseCases/Transactions/DeleteTransactionUseCaseTests.cs` —
      `Execute_WithNonExistentTransaction_ThrowsTransactionNotFoundException`,
      `Execute_WithTransactionBelongingToAnotherUser_ThrowsTransactionNotFoundException`,
      `Execute_WhenNotFound_DoesNotCallDeleteAsync`.
- [x] Reversão de saldo para transações simples (`Income`/`Expense`) via `account.ApplyDelta(RevertDelta(transaction))`
      Verificação: `Execute_Income_RevertsAccountBalance`, `Execute_Expense_RevertsAccountBalance`.
- [x] Exclusão de transação simples: `DeleteAsync(transaction)` + `UpdateAsync(account)` + `CommitAsync()`
      uma única vez, sem chamar `DeleteRangeAsync`
      Verificação: `Execute_NonTransfer_CallsDeleteAsyncAndUpdateAccountAndCommit`,
      `Execute_NonTransfer_DoesNotCallDeleteRangeAsync`.
- [x] Reversão de saldo de ambas as contas em uma transferência (origem via `ToDelta`, destino via `IsIncomingTransfer`)
      Verificação: `Execute_Transfer_RevertsSourceAccountBalance`, `Execute_Transfer_RevertsDestinationAccountBalance`.
- [x] Exclusão de transferência: localiza a perna par via `TransferPeerId`, exclui as duas transações via
      `DeleteRangeAsync`, atualiza as duas contas, `CommitAsync()` uma única vez, nunca chama `DeleteAsync` individual
      Verificação: `Execute_Transfer_DeletesBothTransactionsAndUpdatesAccountsAndCommits`,
      `Execute_Transfer_DoesNotCallDeleteAsync`.
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `IDeleteTransactionUseCase` sem erro em runtime.
- [x] Endpoint `DELETE /api/transactions/{id}` mapeado em `TransactionEndpoints.MapTransactionEndpoints`,
      `UserId` extraído do `ClaimsPrincipal`, requer autenticação, responde `204 No Content` sem body
      Verificação: `tests/Denarius.Api.Tests/Endpoints/TransactionEndpointsTests.cs` —
      `DeleteTransaction_Returns204NoContent`.
- [x] Mapeamento de exceção → HTTP via `ExceptionMiddleware`
      Verificação: `TransactionNotFoundException` (herda `NotFoundException`) retorna 404.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
