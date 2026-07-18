# transaction-create — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Entidade `Transaction` valida `Amount` (positivo) e `Description` (não vazia) no construtor, e
      exige `CategoryId` para `Income`/`Expense`
      Verificação: `tests/Denarius.Domain.Tests/Entities/TransactionTests.cs` cobre amount
      zero/negativo e description vazia/nula/whitespace.
- [x] `Account.ApplyDelta(decimal delta)` soma o delta ao `Balance` e rejeita `delta == 0`
      Verificação: `tests/Denarius.Domain.Tests/Entities/AccountTests.cs` cobre `ApplyDelta`.
- [x] `Transaction.ToDelta()` retorna `+Amount` para `Income` e `-Amount` para `Expense`
      Verificação: `tests/Denarius.Domain.Tests/Entities/TransactionTests.cs` cobre `ToDelta`.
- [x] Input/Output/Interface do use case (`CreateTransactionInput`, `TransactionOutput`,
      `ICreateTransactionUseCase`)
      Verificação: `dotnet build` compila sem erros.
- [x] `CreateTransactionUseCase` busca conta e categoria via `GetByIdAsync(id, userId)`, valida
      existência/atividade da conta, existência/compatibilidade da categoria, cria a `Transaction`,
      aplica o delta na conta, persiste via `ITransactionRepository.AddAsync` +
      `IAccountRepository.UpdateAsync`, e confirma via `IUnitOfWork.CommitAsync` exatamente uma vez
      Verificação: `tests/Denarius.Application.Tests/UseCases/Transactions/CreateTransactionUseCaseTests.cs`
      — happy path Income/Expense, saldo creditado/debitado corretamente, `AddAsync`/`UpdateAsync`/
      `CommitAsync` chamados uma vez, conta inexistente, conta inativa, categoria inexistente,
      categoria incompatível, amount inválido (zero/negativo), description inválida
      (vazia/whitespace/nula), `AddAsync` não chamado quando a conta não é encontrada.
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `ICreateTransactionUseCase` sem erro em runtime.
- [x] Endpoint `POST /api/transactions` mapeado em `TransactionEndpoints.MapTransactionEndpoints`,
      `UserId` extraído do `ClaimsPrincipal`, requer autenticação
      Verificação: `tests/Denarius.Api.Tests/Endpoints/TransactionEndpointsTests.cs` — retorna 201
      com `Location: /api/transactions/{id}` no happy path.
- [x] Mapeamento de exceções → HTTP via `ExceptionMiddleware`
      Verificação: `AccountNotFoundException`/`CategoryNotFoundException` (herdam `NotFoundException`)
      retornam 404; `InactiveAccountException` (herda `AppException`, mapeamento dedicado) retorna
      422; `InvalidCategoryException`/`InvalidAmountException`/`InvalidDescriptionException` (herdam
      `DomainException`) retornam 400.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
