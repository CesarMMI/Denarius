# category-get-all — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Input/Output/Interface do use case (`ListCategoriesInput`, `CategoryOutput`, `IListCategoriesUseCase`)
      Verificação: `dotnet build` compila sem erros.
- [x] `ICategoryRepository.ListByUserAsync(Guid userId, CategoryType? type)` filtra a query por dono da
      categoria, aplica o filtro por tipo apenas quando informado, e ordena por `Name`
      Verificação: `tests/Denarius.Infrastructure.Tests/Repositories/CategoryRepositoryTests.cs` cobre
      listagem de categorias do usuário, filtro por tipo e ausência de categorias de outros usuários no
      resultado.
- [x] `ListCategoriesUseCase` retorna todas as categorias do usuário (ou apenas as do tipo filtrado), ou
      lista vazia quando não há categorias correspondentes
      Verificação: `tests/Denarius.Application.Tests/UseCases/Categories/ListCategoriesUseCaseTests.cs`
      — múltiplas categorias, filtro por `Type`, lista vazia.
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `IListCategoriesUseCase` sem erro em runtime.
- [x] Endpoint `GET /api/categories` mapeado em `CategoryEndpoints.MapCategoryEndpoints`, aceita `type`
      como query string opcional, `UserId` extraído do `ClaimsPrincipal`, requer autenticação
      Verificação: `tests/Denarius.Api.Tests/Endpoints/CategoryEndpointsTests.cs` — retorna 200 com a
      lista de categorias, chama o use case com `UserId` e `Type` corretos.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
