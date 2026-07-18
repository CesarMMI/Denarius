# account-deactivate — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
DELETE /api/accounts/{id}
  → AccountEndpoints.MapAccountEndpoints (Denarius.Api/Endpoints/AccountEndpoints.cs)
  → { id } da rota + UserId extraído do ClaimsPrincipal (ClaimsPrincipalExtensions.GetUserId())
    → DeactivateAccountInput(UserId, AccountId)
  → IDeactivateAccountUseCase → DeactivateAccountUseCase (Denarius.Application/UseCases/Accounts/DeactivateAccountUseCase.cs)
      1. IAccountRepository.GetByIdAsync(accountId, userId) — busca já filtrada por dono
      2. account is null → lança AccountNotFoundException(accountId)
      3. account.Deactivate() — muta a entidade (Denarius.Domain/Entities/Account.cs)
      4. IAccountRepository.UpdateAsync(account)
      5. IUnitOfWork.CommitAsync() — única chamada de commit do use case
  → Results.NoContent() — 204
```

`IDeactivateAccountUseCase` implementa `IUseCase<DeactivateAccountInput, Task>` — não há output,
diferente dos demais use cases de Account que retornam `AccountOutput`.

## Decisões locais

- `IAccountRepository.GetByIdAsync(Guid id, Guid userId)` já filtra a query por dono, mesmo padrão
  usado em `account-get-by-id` e `account-update`. "Conta não existe" e "conta pertence a outro
  usuário" chegam ao use case como o mesmo `null` — ambos os casos viram `AccountNotFoundException`
  (mesmo princípio de não-enumeração usado em `Login`, ver `specs/ARCHITECTURE.md`).
- Idempotência é responsabilidade da entidade, não do use case: `Account.Deactivate()`
  (`Denarius.Domain/Entities/Account.cs:62`) faz early-return (`if (!IsActive) return;`) quando a
  conta já está inativa. O use case não verifica `IsActive` antes de chamar `Deactivate()` — sempre
  chama `UpdateAsync()` e `CommitAsync()`, mesmo quando não houve mudança de estado.
- Endpoint mapeado com `MapDelete`, mas semanticamente é uma desativação (soft delete), não uma
  exclusão de registro — não existe caminho de código que remova a linha da conta do banco.
- Sem output/response body: o use case retorna `Task` (não `Task<T>`), e o endpoint sempre responde
  `204 No Content` no sucesso, nunca dados da conta.

## Edge cases e como são tratados

- `AccountId` que nunca existiu → `AccountNotFoundException` → 404.
- `AccountId` existente mas de outro usuário → mesmo resultado do caso acima, `AccountNotFoundException`
  → 404 (repositório já filtra por `UserId` na query, não há checagem de posse separada no use case).
- Conta já inativa recebendo nova desativação → sem exceção, `UpdateAsync`/`CommitAsync` ainda são
  chamados, resposta 204 igual ao caso de sucesso normal (idempotente).
- Requisição sem token JWT → 401, antes mesmo do use case ser executado (`RequireAuthorization()`
  no grupo de rotas).
- Conta com transações vinculadas → não há checagem especial neste fluxo; a restrição "não pode ser
  excluída, apenas desativada" é garantida pela ausência de qualquer operação de exclusão física,
  não por uma validação em tempo de execução.

## Alternativas descartadas

Nenhuma decisão de projeto alternativa registrada — implementação segue diretamente o padrão de
use case de escrita por id já estabelecido para os demais aggregates (Category, Transaction).
