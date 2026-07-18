# account-get-by-id — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
GET /api/accounts/{id}
  → AccountEndpoints.MapAccountEndpoints (Denarius.Api/Endpoints/AccountEndpoints.cs)
  → { id } da rota + UserId extraído do ClaimsPrincipal (ClaimsPrincipalExtensions.GetUserId())
    → GetAccountByIdInput(UserId, AccountId)
  → IGetAccountByIdUseCase → GetAccountByIdUseCase (Denarius.Application/UseCases/Accounts/GetAccountByIdUseCase.cs)
      1. IAccountRepository.GetByIdAsync(accountId, userId) — busca já filtrada por dono
      2. account is null → lança AccountNotFoundException(accountId)
  → AccountOutput.FromEntity(account)
  → Results.Ok(result) — 200
```

## Decisões locais

- `IAccountRepository.GetByIdAsync(Guid id, Guid userId)` (`Denarius.Infrastructure/Persistence/Repositories/AccountRepository.cs`)
  já recebe `userId` como parâmetro e filtra no próprio `FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId)`.
  Isso significa que "conta não existe" e "conta pertence a outro usuário" chegam ao use case como o
  mesmo resultado (`null`) — o use case não distingue os dois casos, ambos viram `AccountNotFoundException`.
  Efeito colateral desejado: evita vazar para o chamador se um `AccountId` existe mas pertence a
  outro usuário (mesmo princípio de não-enumeração já usado em `Login`, ver `specs/ARCHITECTURE.md`).
- `AccountNotFoundException` herda `NotFoundException` → mapeada para HTTP 404 pelo `ExceptionMiddleware`.
- Contas inativas (`IsActive = false`) não são filtradas pela query — o endpoint retorna qualquer
  conta do usuário, ativa ou não.

## Edge cases e como são tratados

- `AccountId` que nunca existiu → `AccountNotFoundException` → 404.
- `AccountId` existente mas de outro usuário → mesmo resultado do caso acima, `AccountNotFoundException` → 404
  (repositório já filtra por `UserId` na query, não há checagem de posse separada no use case).
- Requisição sem token JWT → 401, antes mesmo do use case ser executado (`RequireAuthorization()` no grupo de rotas).

## Alternativas descartadas

Nenhuma decisão de projeto alternativa registrada — implementação segue diretamente o padrão de
use case de leitura por id já estabelecido para os demais aggregates (Category, Transaction).
