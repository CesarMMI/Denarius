# transaction-delete — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
DELETE /api/transactions/{id}
  → TransactionEndpoints.MapTransactionEndpoints (Denarius.Api/Endpoints/TransactionEndpoints.cs)
  → { id } da rota + UserId extraído do ClaimsPrincipal (ClaimsPrincipalExtensions.GetUserId())
    → DeleteTransactionInput(UserId, TransactionId)
  → IDeleteTransactionUseCase → DeleteTransactionUseCase (Denarius.Application/UseCases/Transactions/DeleteTransactionUseCase.cs)
      1. ITransactionRepository.GetByIdAsync(transactionId, userId) — busca já filtrada por dono
      2. transaction is null → lança TransactionNotFoundException(transactionId)
      3. IAccountRepository.GetByIdAsync(transaction.AccountId, userId) — busca a conta vinculada
      4. account.ApplyDelta(RevertDelta(transaction)) — reverte o efeito da transação no saldo
      5. Se transaction.IsTransfer:
         a. ITransactionRepository.GetByIdAsync(transaction.TransferPeerId, userId) — busca a perna par
         b. IAccountRepository.GetByIdAsync(peer.AccountId, userId) — busca a conta da perna par
         c. peerAccount.ApplyDelta(RevertDelta(peer)) — reverte o efeito da perna par
         d. ITransactionRepository.DeleteRangeAsync([transaction, peer]) — exclui as duas transações
         e. IAccountRepository.UpdateAsync(account) e UpdateAsync(peerAccount)
      6. Senão: ITransactionRepository.DeleteAsync(transaction) e IAccountRepository.UpdateAsync(account)
      7. IUnitOfWork.CommitAsync() — única chamada de commit do use case
  → Results.NoContent() — 204
```

`IDeleteTransactionUseCase` implementa `IUseCase<DeleteTransactionInput, Task>` — não há output,
mesmo padrão de `IDeactivateAccountUseCase`.

## Decisões locais

- `ITransactionRepository.GetByIdAsync(Guid id, Guid userId)` já filtra a query por dono, mesmo
  padrão usado nos demais use cases de Transaction. "Transação não existe" e "transação pertence
  a outro usuário" chegam ao use case como o mesmo `null` — ambos os casos viram
  `TransactionNotFoundException` (mesmo princípio de não-enumeração usado em `Login`, ver
  `specs/ARCHITECTURE.md`).
- A reversão de saldo é calculada por `RevertDelta`, método privado do use case
  (`DeleteTransactionUseCase.cs:46-47`) que nega o efeito originalmente aplicado na criação:
  - `IsIncomingTransfer == true` (perna de entrada de uma transferência) → `-Amount`
  - Qualquer outro caso (`Income`, `Expense`, perna de saída de `Transfer`) → `-transaction.ToDelta()`
    (`Income` → `-Amount`; `Expense` → `+Amount`; `Transfer` de saída → `+Amount`)
- Para transações do tipo `Transfer`, o use case busca a perna par (via `TransferPeerId`) e a conta
  correspondente antes de excluir qualquer coisa, reverte o saldo de **ambas** as contas, e só então
  exclui as duas transações juntas via `DeleteRangeAsync` — nunca via `DeleteAsync` individual, mesmo
  padrão de "par indivisível" já usado na criação de transferências (`CreateTransferUseCase`).
- `IAccountRepository.UpdateAsync` é chamado uma vez por conta afetada (uma para transação simples,
  duas para transferência), mas `IUnitOfWork.CommitAsync()` é chamado uma única vez ao final,
  cobrindo deletes e updates numa única transação de banco.
- `DeleteAsync`/`DeleteRangeAsync` (`TransactionRepository.cs`) removem a entidade via
  `context.Transactions.Remove(...)`/`RemoveRange(...)` — exclusão física (hard delete), não
  soft-delete.
- O use case não valida se a conta está ativa antes de reverter o saldo — a reversão ocorre
  independentemente do `IsActive` da conta.

## Edge cases e como são tratados

- `TransactionId` que nunca existiu → `TransactionNotFoundException` → 404.
- `TransactionId` existente mas de outro usuário → mesmo resultado do caso acima,
  `TransactionNotFoundException` → 404 (repositório já filtra por `UserId` na query, não há
  checagem de posse separada no use case).
- Transação do tipo `Transfer` → a perna par é sempre localizada via `TransferPeerId` e excluída
  junto; não há caminho de código que exclua apenas uma perna.
- Requisição sem token JWT → 401, antes mesmo do use case ser executado (`RequireAuthorization()`
  no grupo de rotas).
- `Account.ApplyDelta` lança `InvalidDeltaException` se o delta calculado for zero — na prática
  nunca ocorre neste fluxo, pois `Amount` de uma transação é sempre positivo (invariante garantida
  na criação), então `RevertDelta` nunca retorna zero.

## Alternativas descartadas

- Soft-delete de transações (marcar como excluída sem remover fisicamente): descartado a favor de
  exclusão física simples, consistente com o padrão de `category-delete`.
- Bloquear a exclusão de uma perna de transferência isoladamente (exigir que o usuário exclua as
  duas manualmente): descartado — o use case sempre trata o par como uma unidade indivisível,
  localizando e excluindo a perna par automaticamente.
