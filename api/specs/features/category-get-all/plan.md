# category-get-all — Plan

Esta feature já está implementada. Este documento reflete o código real.

## Abordagem técnica

Fluxo Clean Architecture padrão do projeto (ver `specs/ARCHITECTURE.md`):

```
GET /api/categories?type={Income|Expense}
  → CategoryEndpoints.MapCategoryEndpoints (Denarius.Api/Endpoints/CategoryEndpoints.cs)
  → `type` vinculado do query string (CategoryType?), UserId extraído do ClaimsPrincipal (ClaimsPrincipalExtensions.GetUserId())
    → ListCategoriesInput(UserId, Type)
  → IListCategoriesUseCase → ListCategoriesUseCase (Denarius.Application/UseCases/Categories/ListCategoriesUseCase.cs)
      1. ICategoryRepository.ListByUserAsync(userId, type) — busca já filtrada por dono e, se informado, por tipo
      2. categories.Select(CategoryOutput.FromEntity)
  → Results.Ok(result) — 200, IEnumerable<CategoryOutput>
```

## Decisões locais

- `ICategoryRepository.ListByUserAsync(Guid userId, CategoryType? type)` (`Denarius.Infrastructure/Persistence/Repositories/CategoryRepository.cs`)
  filtra por `UserId` diretamente na query (`Where(c => c.UserId == userId)`), aplica o filtro por `Type`
  apenas quando informado (`type.HasValue`), e ordena por `Name` (`OrderBy(c => c.Name)`) — a ordenação
  alfabética é uma decisão do repositório, não exposta como opção configurável pelo use case ou pelo endpoint.
- O filtro por `Type` é resolvido inteiramente na query do banco (via `CategoryType?` no repositório), não em
  memória — evita carregar categorias fora do filtro antes de descartá-las.
- Sem paginação: a query retorna todas as categorias do usuário de uma vez. Aceitável dado que o número
  de categorias por usuário é tipicamente pequeno (dezenas, não milhares).

## Edge cases e como são tratados

- Usuário sem nenhuma categoria (ou nenhuma do tipo filtrado) → `ListByUserAsync` retorna lista vazia →
  endpoint responde 200 com `[]` (não é um erro).
- `Type` ausente na query string → `CategoryType?` chega como `null` ao endpoint → nenhum filtro de tipo
  é aplicado, comportamento idêntico a "listar todas".
- Requisição sem token JWT → 401, antes mesmo do use case ser executado (`RequireAuthorization()`
  no grupo de rotas).

## Alternativas descartadas

Nenhuma decisão de projeto alternativa registrada — implementação segue diretamente o padrão de
use case de listagem já estabelecido para os demais aggregates (Account, Transaction), com a adição
do filtro opcional por `Type`.
