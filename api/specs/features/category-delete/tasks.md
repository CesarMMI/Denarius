# category-delete — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Input/Interface do use case (`DeleteCategoryInput`, `IDeleteCategoryUseCase`)
      Verificação: `dotnet build` compila sem erros.
- [x] `ICategoryRepository.NullifyTransactionCategoriesAsync(categoryId)` implementado via
      `ExecuteUpdateAsync` direto no banco (`CategoryRepository.cs`), zerando `CategoryId` de
      todas as transações vinculadas sem carregar entidades em memória
      Verificação: `tests/Denarius.Infrastructure.Tests/Repositories/CategoryRepositoryTests.cs`
      cobre o nullify em massa.
- [x] `DeleteCategoryUseCase` busca a categoria via `ICategoryRepository.GetByIdAsync(categoryId, userId)`,
      lança `CategoryNotFoundException` quando `null`, chama `NullifyTransactionCategoriesAsync`
      antes de `DeleteAsync`, e `CommitAsync()` uma única vez
      Verificação: `tests/Denarius.Application.Tests/UseCases/Categories/DeleteCategoryUseCaseTests.cs`
      — happy path chama nullify/delete/commit uma vez cada, nullify roda antes do delete,
      categoria inexistente lança `CategoryNotFoundException`, categoria de outro usuário mesmo
      comportamento, categoria inexistente não chama `DeleteAsync`.
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `IDeleteCategoryUseCase` sem erro em runtime.
- [x] Endpoint `DELETE /api/categories/{id}` mapeado em `CategoryEndpoints.MapCategoryEndpoints`,
      `UserId` extraído do `ClaimsPrincipal`, requer autenticação, retorna 204 sem corpo
      Verificação: `tests/Denarius.Api.Tests/Endpoints/CategoryEndpointsTests.cs` —
      `DeleteCategory_Returns204NoContent`.
- [x] Mapeamento de exceção → HTTP via `ExceptionMiddleware`
      Verificação: `CategoryNotFoundException` (herda `NotFoundException`) retorna 404.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
