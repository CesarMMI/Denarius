# category-delete — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
DELETE /api/categories/{id}
  → CategoryEndpoints.MapCategoryEndpoints (Denarius.Api/Endpoints/CategoryEndpoints.cs)
  → { id } da rota + UserId extraído do ClaimsPrincipal (ClaimsPrincipalExtensions.GetUserId())
    → DeleteCategoryInput(UserId, CategoryId)
  → IDeleteCategoryUseCase → DeleteCategoryUseCase (Denarius.Application/UseCases/Categories/DeleteCategoryUseCase.cs)
      1. ICategoryRepository.GetByIdAsync(categoryId, userId) — busca já filtrada por dono
      2. category is null → lança CategoryNotFoundException(categoryId)
      3. ICategoryRepository.NullifyTransactionCategoriesAsync(category.Id) — zera CategoryId
         de todas as transações vinculadas, antes de excluir a categoria
      4. ICategoryRepository.DeleteAsync(category)
      5. IUnitOfWork.CommitAsync() — única chamada de commit do use case
  → Results.NoContent() — 204
```

## Decisões locais

- `ICategoryRepository.GetByIdAsync(Guid id, Guid userId)` já filtra a query por dono, mesmo
  padrão usado em `category-get-by-id` e `category-update`. "Categoria não existe" e "categoria
  pertence a outro usuário" chegam ao use case como o mesmo `null` — ambos os casos viram
  `CategoryNotFoundException` (mesmo princípio de não-enumeração usado em `Login`, ver
  `specs/ARCHITECTURE.md`).
- `NullifyTransactionCategoriesAsync` roda como um `ExecuteUpdateAsync` direto no banco
  (`CategoryRepository.cs`), não carrega as transações em memória para atualizá-las uma a uma —
  evita N updates individuais para categorias com muitas transações vinculadas.
- A ordem importa: nullify roda **antes** do delete no use case (coberto por teste dedicado,
  `ExecuteAsync_WithExistingCategory_NullifiesBeforeDelete`) para não deixar uma janela em que
  transações referenciam uma categoria já removida.
- `DeleteAsync` remove a entidade via `context.Categories.Remove(category)` — exclusão física
  (hard delete), não soft-delete.
- O use case não precisa checar se existem transações vinculadas antes de decidir se pode
  excluir — ao contrário de um "bloquear exclusão se houver dependências", a estratégia adotada
  é sempre permitir a exclusão e desvincular as transações.

## Edge cases e como são tratados

- `CategoryId` que nunca existiu → `CategoryNotFoundException` → 404.
- `CategoryId` existente mas de outro usuário → mesmo resultado do caso acima,
  `CategoryNotFoundException` → 404 (repositório já filtra por `UserId` na query, não há
  checagem de posse separada no use case).
- Categoria sem nenhuma transação vinculada → `NullifyTransactionCategoriesAsync` executa um
  update que não afeta nenhuma linha; fluxo segue normalmente.
- Categoria com falha na busca (`null`) → `DeleteAsync` e `NullifyTransactionCategoriesAsync`
  nunca são chamados (coberto por `ExecuteAsync_WithNonExistentCategory_DoesNotCallDeleteAsync`).
- Requisição sem token JWT → 401, antes mesmo do use case ser executado (`RequireAuthorization()`
  no grupo de rotas).

## Alternativas descartadas

- Excluir em cascata as transações vinculadas à categoria: descartado porque destruiria o
  histórico financeiro do usuário só por causa da remoção de uma categoria; nullify preserva a
  transação e apenas remove a classificação.
- Bloquear a exclusão de categorias com transações vinculadas (exigir desvincular manualmente
  antes): descartado a favor do nullify automático, que resolve a dependência sem exigir um
  passo extra do usuário.
