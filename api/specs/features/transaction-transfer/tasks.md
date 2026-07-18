# transaction-transfer — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Entidade `Transaction` valida `Amount`/`Description` no construtor, exige `TransferPeerId`
      e proíbe `CategoryId` quando `Type == Transfer`; expõe `MarkAsIncoming()` e
      `LinkTransferPeer(peerId)` para montar o par vinculado
      Verificação: `tests/Denarius.Domain.Tests/Entities/TransactionTests.cs` cobre amount/description
      inválidos, transferência sem `TransferPeerId`, transferência com categoria, e os dois métodos
      de par de transferência.
- [x] Input/Output/Interface do use case (`CreateTransferInput`, `CreateTransferOutput`,
      `ICreateTransferUseCase`) — `CreateTransferOutput` é output composto (`Outgoing`/`Incoming`,
      sem `FromEntity()`)
      Verificação: `dotnet build` compila sem erros.
- [x] `CreateTransferUseCase` busca origem e destino via `IAccountRepository.GetByIdAsync(id, userId)`,
      valida existência/atividade de ambas, valida contas diferentes e mesma moeda, monta o par
      `incoming`/`outgoing` vinculado via `TransferPeerId`, aplica `ApplyDelta` nos dois saldos,
      persiste via `AddRangeAsync` + dois `UpdateAsync` e confirma via `CommitAsync` exatamente uma vez
      Verificação: `tests/Denarius.Application.Tests/UseCases/Transactions/CreateTransferUseCaseTests.cs`
      — happy path (par vinculado, `IsIncomingTransfer`, débito/crédito de saldo, `AccountId` de
      cada perna), conta de origem/destino inexistente, conta de origem/destino inativa, mesma
      conta, moedas diferentes, `Amount` inválido, `Description` inválida.
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `ICreateTransferUseCase` sem erro em runtime.
- [x] Endpoint `POST /api/transactions/transfer` mapeado em `TransactionEndpoints.MapTransactionEndpoints`,
      recebe `CreateTransferRequest(SourceAccountId, DestinationAccountId, Amount, Description, Date)`,
      `UserId` extraído do `ClaimsPrincipal`, requer autenticação
      Verificação: `tests/Denarius.Api.Tests/Endpoints/TransactionEndpointsTests.cs` — retorna 201
      com `Location: /api/transactions/{outgoing.Id}` no happy path.
- [x] Mapeamento de exceções → HTTP via `ExceptionMiddleware`
      Verificação: `AccountNotFoundException` (herda `NotFoundException`) retorna 404;
      `InactiveAccountException` retorna 422; `InvalidTransferException`/`InvalidAmountException`/
      `InvalidDescriptionException` (herdam `DomainException`) retornam 400.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
