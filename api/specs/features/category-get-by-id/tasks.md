# category-get-by-id — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Input/Output/Interface do use case (`GetCategoryByIdInput`, `CategoryOutput`, `IGetCategoryByIdUseCase`)
      Verificação: `dotnet build` compila sem erros.
- [x] `ICategoryRepository.GetByIdAsync(Guid id, Guid userId)` filtra a query por dono da categoria
      Verificação: `tests/Denarius.Infrastructure.Tests/Repositories/CategoryRepositoryTests.cs` cobre
      busca de categoria existente do usuário e ausência de retorno para categoria de outro usuário.
- [x] `GetCategoryByIdUseCase` retorna a categoria encontrada ou lança `CategoryNotFoundException`
      quando o repositório retorna `null` (categoria inexistente ou de outro usuário)
      Verificação: `tests/Denarius.Application.Tests/UseCases/Categories/GetCategoryByIdUseCaseTests.cs`
      — happy path, categoria inexistente, categoria pertencente a outro usuário (mesmo comportamento).
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `IGetCategoryByIdUseCase` sem erro em runtime.
- [x] Endpoint `GET /api/categories/{id}` mapeado em `CategoryEndpoints.MapCategoryEndpoints`,
      `UserId` extraído do `ClaimsPrincipal`, requer autenticação
      Verificação: `tests/Denarius.Api.Tests/Endpoints/CategoryEndpointsTests.cs` — retorna 200 com
      os dados da categoria no happy path, chama o use case com `UserId`/`CategoryId` corretos.
- [x] Mapeamento de exceção → HTTP via `ExceptionMiddleware`
      Verificação: `CategoryNotFoundException` (herda `NotFoundException`) retorna 404.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
