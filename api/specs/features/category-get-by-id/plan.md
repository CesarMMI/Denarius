# category-get-by-id — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
GET /api/categories/{id}
  → CategoryEndpoints.MapCategoryEndpoints (Denarius.Api/Endpoints/CategoryEndpoints.cs)
  → { id } da rota + UserId extraído do ClaimsPrincipal (ClaimsPrincipalExtensions.GetUserId())
    → GetCategoryByIdInput(UserId, CategoryId)
  → IGetCategoryByIdUseCase → GetCategoryByIdUseCase (Denarius.Application/UseCases/Categories/GetCategoryByIdUseCase.cs)
      1. ICategoryRepository.GetByIdAsync(categoryId, userId) — busca já filtrada por dono
      2. category is null → lança CategoryNotFoundException(categoryId)
  → CategoryOutput.FromEntity(category)
  → Results.Ok(result) — 200
```

## Decisões locais

- `ICategoryRepository.GetByIdAsync(Guid id, Guid userId)` (`Denarius.Infrastructure/Persistence/Repositories/CategoryRepository.cs`)
  já recebe `userId` como parâmetro e filtra no próprio `FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId)`.
  Isso significa que "categoria não existe" e "categoria pertence a outro usuário" chegam ao use case como o
  mesmo resultado (`null`) — o use case não distingue os dois casos, ambos viram `CategoryNotFoundException`.
  Efeito colateral desejado: evita vazar para o chamador se um `CategoryId` existe mas pertence a
  outro usuário (mesmo princípio de não-enumeração já usado em `Login` e `account-get-by-id`, ver `specs/ARCHITECTURE.md`).
- `CategoryNotFoundException` herda `NotFoundException` → mapeada para HTTP 404 pelo `ExceptionMiddleware`.

## Edge cases e como são tratados

- `CategoryId` que nunca existiu → `CategoryNotFoundException` → 404.
- `CategoryId` existente mas de outro usuário → mesmo resultado do caso acima, `CategoryNotFoundException` → 404
  (repositório já filtra por `UserId` na query, não há checagem de posse separada no use case).
- Requisição sem token JWT → 401, antes mesmo do use case ser executado (`RequireAuthorization()` no grupo de rotas).

## Alternativas descartadas

Nenhuma decisão de projeto alternativa registrada — implementação segue diretamente o padrão de
use case de leitura por id já estabelecido para os demais aggregates (Account, Transaction).
