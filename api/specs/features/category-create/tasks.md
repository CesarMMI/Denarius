# category-create — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Entidade `Category` valida `Name` e `Color` no construtor, aceita `Type` sem validação
      própria (enum)
      Verificação: `tests/Denarius.Domain.Tests/Entities/CategoryTests.cs` cobre nome/cor
      vazios, nulos e só com espaços.
- [x] Input/Output/Interface do use case (`CreateCategoryInput`, `CategoryOutput`,
      `ICreateCategoryUseCase`)
      Verificação: `dotnet build` compila sem erros.
- [x] `CreateCategoryUseCase` cria a entidade, persiste via `ICategoryRepository.AddAsync` e
      confirma via `IUnitOfWork.CommitAsync` exatamente uma vez
      Verificação: `tests/Denarius.Application.Tests/UseCases/Categories/CreateCategoryUseCaseTests.cs`
      — happy path, nome/cor inválidos, `AddAsync` não chamado quando a validação falha.
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `ICreateCategoryUseCase` sem erro em runtime.
- [x] Endpoint `POST /api/categories` mapeado em `CategoryEndpoints.MapCategoryEndpoints`,
      `UserId` extraído do `ClaimsPrincipal`, requer autenticação
      Verificação: `tests/Denarius.Api.Tests/Endpoints/CategoryEndpointsTests.cs` — retorna 201
      com `Location: /api/categories/{id}` no happy path.
- [x] Mapeamento de exceções → HTTP via `ExceptionMiddleware`
      Verificação: `InvalidNameException`, `InvalidColorException` (ambas `DomainException`)
      retornam 400.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
