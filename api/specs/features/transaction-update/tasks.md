# transaction-update — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Input/Output/Interface do use case (`UpdateTransactionInput`, `TransactionOutput`, `IUpdateTransactionUseCase`)
      Verificação: `dotnet build` compila sem erros.
- [x] `Transaction.UpdateAmount(amount)`, `UpdateDescription(description)` e `UpdateCategory(categoryId)` validam
      seus respectivos campos e não aceitam alterar `Type`
      Verificação: `tests/Denarius.Domain.Tests/Entities/TransactionTests.cs` cobre `InvalidAmountException`,
      `InvalidDescriptionException` e `InvalidCategoryException` (categoria em `Transfer`, categoria nula em
      `Income`/`Expense`).
- [x] `UpdateTransactionUseCase` busca a transação via `ITransactionRepository.GetByIdAsync(transactionId, userId)`,
      lança `TransactionNotFoundException` quando `null`, aplica `UpdateAmount`/`UpdateDescription`/`UpdateCategory`,
      chama `UpdateAsync`/`CommitAsync` uma única vez
      Verificação: `tests/Denarius.Application.Tests/UseCases/Transactions/UpdateTransactionUseCaseTests.cs` —
      `Execute_WithValidInput_ReturnsUpdatedTransaction`, `Execute_WithValidInput_CallsUpdateAsyncAndCommit`,
      transação inexistente.
- [x] Recalculo de saldo da conta quando `Amount` muda, via `AccountDelta(novoAmount) - AccountDelta(amountAntigo)`
      aplicado com `account.ApplyDelta(correction)`, para `Income` e `Expense`
      Verificação: `Execute_IncomeAmountChange_CorrectlyAdjustsAccountBalance`,
      `Execute_ExpenseAmountChange_CorrectlyAdjustsAccountBalance`,
      `Execute_WhenAmountUnchanged_DoesNotAdjustAccountBalance` no arquivo de testes acima.
- [x] Sincronização da transação par (`TransferPeer`) quando `Amount` muda em uma transação `Transfer` — peer
      recebe o mesmo novo valor e sua conta tem o saldo recalculado
      Verificação: `Execute_TransferAmountChange_AdjustsBothAccountBalances`,
      `Execute_TransferAmountChange_UpdatesBothTransactions` no arquivo de testes acima.
- [x] Troca de categoria em `Income`/`Expense`: exige `CategoryId` não-nulo, categoria deve existir, pertencer
      ao usuário e ser compatível com o tipo da transação
      Verificação: `Execute_WithNullCategoryOnIncomeExpense_ThrowsInvalidCategoryException`,
      `Execute_WithNonExistentCategory_ThrowsCategoryNotFoundException`,
      `Execute_WithIncompatibleCategory_ThrowsInvalidCategoryException` no arquivo de testes acima.
- [x] Rejeição de `CategoryId` não-nulo em transação `Transfer`
      Verificação: `Execute_SettingCategoryOnTransfer_ThrowsInvalidCategoryException` no arquivo de testes acima.
- [x] Validação de `Amount` (zero/negativo) e `Description` (vazia/nula/só espaços)
      Verificação: `Execute_WithInvalidAmount_ThrowsInvalidAmountException`,
      `Execute_WithInvalidDescription_ThrowsInvalidDescriptionException` no arquivo de testes acima.
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      (`services.AddScoped<IUpdateTransactionUseCase, UpdateTransactionUseCase>()`)
      Verificação: `dotnet build` resolve `IUpdateTransactionUseCase` sem erro em runtime.
- [x] Endpoint `PUT /api/transactions/{id}` mapeado em `TransactionEndpoints.MapTransactionEndpoints`, recebe
      `UpdateTransactionRequest(Amount, Description, CategoryId)`, `UserId` extraído do `ClaimsPrincipal`,
      requer autenticação
      Verificação: `tests/Denarius.Api.Tests/Endpoints/TransactionEndpointsTests.cs` — retorna 200 com os dados
      atualizados da transação no happy path, chama o use case com
      `UserId`/`TransactionId`/`Amount`/`Description`/`CategoryId` corretos.
- [x] Mapeamento de exceção → HTTP via `ExceptionMiddleware`
      Verificação: `TransactionNotFoundException`/`CategoryNotFoundException` (herdam `NotFoundException`)
      retornam 404; `InvalidAmountException`/`InvalidDescriptionException`/`InvalidCategoryException`
      (herdam `DomainException`) retornam 400.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes de qualquer
      nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
