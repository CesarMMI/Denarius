# category-update — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Input/Output/Interface do use case (`UpdateCategoryInput`, `CategoryOutput`, `IUpdateCategoryUseCase`)
      Verificação: `dotnet build` compila sem erros.
- [x] `Category.Update(name, color)` valida `Name`/`Color` e não aceita `type` como parâmetro
      Verificação: `tests/Denarius.Domain.Tests/Entities/CategoryTests.cs` cobre update válido e
      lançamento de `InvalidNameException`/`InvalidColorException` para valores vazios/nulos.
- [x] `UpdateCategoryUseCase` busca a categoria via `ICategoryRepository.GetByIdAsync(categoryId, userId)`,
      lança `CategoryNotFoundException` quando `null`, chama `category.Update()`, `UpdateAsync()` e
      `CommitAsync()` uma única vez
      Verificação: `tests/Denarius.Application.Tests/UseCases/Categories/UpdateCategoryUseCaseTests.cs`
      — happy path, tipo não alterado, `UpdateAsync`/`CommitAsync` chamados uma vez, categoria
      inexistente, categoria de outro usuário, nome/cor inválidos.
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `IUpdateCategoryUseCase` sem erro em runtime.
- [x] Endpoint `PUT /api/categories/{id}` mapeado em `CategoryEndpoints.MapCategoryEndpoints`, recebe
      `UpdateCategoryRequest(Name, Color)`, `UserId` extraído do `ClaimsPrincipal`, requer autenticação
      Verificação: `tests/Denarius.Api.Tests/Endpoints/CategoryEndpointsTests.cs` — retorna 200 com
      os dados atualizados da categoria no happy path, chama o use case com
      `UserId`/`CategoryId`/`Name`/`Color` corretos.
- [x] Mapeamento de exceção → HTTP via `ExceptionMiddleware`
      Verificação: `CategoryNotFoundException` (herda `NotFoundException`) retorna 404;
      `InvalidNameException`/`InvalidColorException` (herdam `DomainException`) retornam 400.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
