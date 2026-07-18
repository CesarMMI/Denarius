# category-update — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
PUT /api/categories/{id}
  → CategoryEndpoints.MapCategoryEndpoints (Denarius.Api/Endpoints/CategoryEndpoints.cs)
  → { id } da rota + UpdateCategoryRequest(Name, Color) + UserId extraído do ClaimsPrincipal
    (ClaimsPrincipalExtensions.GetUserId())
    → UpdateCategoryInput(UserId, CategoryId, Name, Color)
  → IUpdateCategoryUseCase → UpdateCategoryUseCase (Denarius.Application/UseCases/Categories/UpdateCategoryUseCase.cs)
      1. ICategoryRepository.GetByIdAsync(categoryId, userId) — busca já filtrada por dono
      2. category is null → lança CategoryNotFoundException(categoryId)
      3. category.Update(name, color) — valida e muta a entidade (Denarius.Domain/Entities/Category.cs)
      4. ICategoryRepository.UpdateAsync(category)
      5. IUnitOfWork.CommitAsync() — única chamada de commit do use case
  → CategoryOutput.FromEntity(category)
  → Results.Ok(result) — 200
```

## Decisões locais

- `ICategoryRepository.GetByIdAsync(Guid id, Guid userId)` já filtra a query por dono
  (`FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId)`), mesmo padrão usado em
  `category-get-by-id` e `account-update`. "Categoria não existe" e "categoria pertence a outro
  usuário" chegam ao use case como o mesmo `null` — ambos os casos viram `CategoryNotFoundException`
  (mesmo princípio de não-enumeração usado em `Login`, ver `specs/ARCHITECTURE.md`).
- A validação de `Name`/`Color` vazios ou nulos vive inteiramente em `Category.Update()`
  (`InvalidNameException`, `InvalidColorException`) — o use case não duplica essa validação,
  apenas propaga o que a entidade lançar.
- `Category.Update(name, color)` não recebe `type` como parâmetro — a assinatura do método já
  impede a alteração do tipo por construção, não por uma checagem em runtime.
- `UpdateCategoryInput`/`UpdateCategoryRequest` só carregam `Name` e `Color` — não existe caminho
  para o chamador enviar `Type` nesta feature.

## Edge cases e como são tratados

- `CategoryId` que nunca existiu → `CategoryNotFoundException` → 404.
- `CategoryId` existente mas de outro usuário → mesmo resultado do caso acima,
  `CategoryNotFoundException` → 404 (repositório já filtra por `UserId` na query, não há checagem
  de posse separada no use case).
- `Name` vazio, só espaços ou nulo → `InvalidNameException` → 400.
- `Color` vazia, só espaços ou nula → `InvalidColorException` → 400.
- Requisição sem token JWT → 401, antes mesmo do use case ser executado (`RequireAuthorization()`
  no grupo de rotas).

## Alternativas descartadas

Nenhuma decisão de projeto alternativa registrada — implementação segue diretamente o padrão de
use case de escrita por id já estabelecido para os demais aggregates, o mesmo usado em
`specs/features/account-update/plan.md`.
