# transaction-transfer — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
POST /api/transactions/transfer
  → TransactionEndpoints.MapTransactionEndpoints (Denarius.Api/Endpoints/TransactionEndpoints.cs)
  → CreateTransferRequest(SourceAccountId, DestinationAccountId, Amount, Description, Date)
    + UserId extraído do ClaimsPrincipal via ClaimsPrincipalExtensions.GetUserId()
    → CreateTransferInput
  → ICreateTransferUseCase → CreateTransferUseCase (Denarius.Application/UseCases/Transactions/CreateTransferUseCase.cs)
      1. IAccountRepository.GetByIdAsync(sourceAccountId, userId) — busca já filtrada por dono
         null → AccountNotFoundException(sourceAccountId)
         !IsActive → InactiveAccountException(sourceAccountId)
      2. IAccountRepository.GetByIdAsync(destinationAccountId, userId) — mesma filtragem
         null → AccountNotFoundException(destinationAccountId)
         !IsActive → InactiveAccountException(destinationAccountId)
      3. sourceAccountId == destinationAccountId → InvalidTransferException
      4. sourceAccount.CurrencyCode != destinationAccount.CurrencyCode → InvalidTransferException
      5. new Transaction(...) para a perna de entrada (incoming), com um TransferPeerId
         provisório (Guid.NewGuid()) só para satisfazer a validação do construtor
         (entidade valida Amount e Description aqui — InvalidAmountException /
         InvalidDescriptionException se inválidos)
      6. incoming.MarkAsIncoming()
      7. new Transaction(...) para a perna de saída (outgoing), com TransferPeerId = incoming.Id
      8. incoming.LinkTransferPeer(outgoing.Id) — corrige o TransferPeerId provisório do
         incoming para apontar de volta ao outgoing real
      9. sourceAccount.ApplyDelta(-Amount)
      10. destinationAccount.ApplyDelta(Amount)
      11. ITransactionRepository.AddRangeAsync([outgoing, incoming])
      12. IAccountRepository.UpdateAsync(sourceAccount)
      13. IAccountRepository.UpdateAsync(destinationAccount)
      14. IUnitOfWork.CommitAsync() — única chamada de commit do use case
  → CreateTransferOutput(TransactionOutput.FromEntity(outgoing), TransactionOutput.FromEntity(incoming))
  → Results.Created($"/api/transactions/{result.Outgoing.Id}", result) — 201
```

## Decisões locais

- `IAccountRepository.GetByIdAsync(Guid id, Guid userId)` já filtra a query por dono, mesmo
  padrão usado em `account-update`/`account-get-by-id`. "Conta não existe" e "conta pertence a
  outro usuário" chegam ao use case como o mesmo `null` — ambos os casos viram
  `AccountNotFoundException` (mesmo princípio de não-enumeração usado em `Login`, ver
  `specs/ARCHITECTURE.md`).
- Ordem de validação é fixa e sequencial: origem (existência → ativa) → destino (existência →
  ativa) → mesma conta → moeda diferente → `Amount`/`Description` (estes dois últimos só são
  checados quando a entidade `Transaction` é construída, dentro do construtor de
  `Denarius.Domain/Entities/Transaction.cs`).
- `CreateTransferOutput` é um output composto (record sem `FromEntity()`, conforme convenção do
  projeto para outputs que agrupam outros outputs) — carrega `Outgoing` e `Incoming`, cada um um
  `TransactionOutput.FromEntity(...)` completo.
- Problema de "ovo e galinha" na criação do par: o construtor de `Transaction` exige
  `transferPeerId` não-nulo quando `Type == Transfer`, mas os dois `Id`s só existem depois que
  cada `Transaction` é construída. Solução usada: constrói-se primeiro o `incoming` com um
  `TransferPeerId` provisório (`Guid.NewGuid()`, nunca persistido), depois o `outgoing` já com
  `TransferPeerId = incoming.Id` (correto desde a criação), e por fim `incoming.LinkTransferPeer(outgoing.Id)`
  substitui o valor provisório pelo `Id` real do `outgoing`. O `outgoing` nunca passa por um
  valor provisório.
- `IsIncomingTransfer` é setado via `incoming.MarkAsIncoming()` logo após a criação — `outgoing`
  nunca chama esse método e permanece com o valor padrão (`false`).
- Nenhuma das duas transações recebe `CategoryId` (`null`) — o construtor de `Transaction` já
  proíbe categoria quando `Type == Transfer` (`InvalidCategoryException`), então o use case nunca
  precisa validar isso explicitamente.
- Persistência das duas transações é feita em uma única chamada, `AddRangeAsync([outgoing, incoming])`,
  não duas chamadas separadas de `AddAsync` — reforça a atomicidade do par (ambas persistem
  juntas no mesmo `CommitAsync`).
- Atualização de saldo usa `Account.ApplyDelta(decimal)` diretamente no use case (delta negativo
  para origem, positivo para destino) — não usa `Transaction.ToDelta()`, que é usado pelos fluxos
  de `CreateTransaction`/`UpdateTransaction`/`DeleteTransaction` para uma única conta.
- Resposta HTTP usa `Results.Created` com `Location: /api/transactions/{outgoing.Id}` — aponta
  para a perna de saída, consistente com `GET /api/transactions/{id}` (`GetTransactionById`), que
  também funciona para o `Id` da perna de entrada.

## Edge cases e como são tratados

- `SourceAccountId` que nunca existiu, ou de outro usuário → `AccountNotFoundException` → 404
  (repositório já filtra por `UserId`, sem checagem de posse separada no use case).
- `DestinationAccountId` que nunca existiu, ou de outro usuário → mesmo tratamento, mesmo status.
- Conta de origem inativa → `InactiveAccountException` → 422, antes mesmo de verificar a conta de
  destino.
- Conta de destino inativa → `InactiveAccountException` → 422, verificado somente depois que a
  conta de origem já passou pelas checagens de existência/atividade.
- `SourceAccountId == DestinationAccountId` (mesma conta em ambos os campos) → `InvalidTransferException`
  → 400, verificado somente depois que ambas as contas foram encontradas e estão ativas.
- Moedas diferentes (`BRL` vs `USD`) → `InvalidTransferException` → 400, última checagem antes da
  criação das transações.
- `Amount` zero ou negativo → `InvalidAmountException` (lançada pelo construtor de `Transaction`,
  na criação do `incoming`) → 400.
- `Description` vazia, só espaços, ou nula → `InvalidDescriptionException` (mesmo ponto do
  `Amount`) → 400.
- Requisição sem token JWT → 401, antes mesmo do use case ser executado (`RequireAuthorization()`
  no grupo de rotas).
- `Date` futura ou passada → aceita sem restrição, mesmo comportamento de `CreateTransaction`
  (ver `specs/entities/transaction.md`).

## Alternativas descartadas

- Gerar os dois `Guid`s de transação antecipadamente (fora do construtor de `Transaction`, com
  `Guid.NewGuid()` explícito para `outgoing.Id` e `incoming.Id` antes de instanciar as entidades)
  para evitar o `TransferPeerId` provisório — descartado porque `Transaction.Id` é atribuído
  internamente no construtor (`Id = Guid.NewGuid()`), não é um parâmetro; mudar isso quebraria o
  padrão de todas as outras entidades do projeto, que não aceitam `Id` como parâmetro de
  construção.
