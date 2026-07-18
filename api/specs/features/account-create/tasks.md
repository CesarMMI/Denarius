# account-create — Tasks

Feature já implementada — tarefas abaixo documentam o estado atual e servem como checklist de regressão.

- [x] Entidade `Account` valida `Name`, `CurrencyCode` e `Color` no construtor, e inicializa
      `Balance = 0` / `IsActive = true`
      Verificação: `tests/Denarius.Domain.Tests/Entities/AccountTests.cs` cobre nome/cor
      vazios e código de moeda inválido.
- [x] Input/Output/Interface do use case (`CreateAccountInput`, `AccountOutput`, `ICreateAccountUseCase`)
      Verificação: `dotnet build` compila sem erros.
- [x] `CreateAccountUseCase` cria a entidade, persiste via `IAccountRepository.AddAsync` e
      confirma via `IUnitOfWork.CommitAsync` exatamente uma vez
      Verificação: `tests/Denarius.Application.Tests/UseCases/Accounts/CreateAccountUseCaseTests.cs`
      — happy path (saldo zero, ativo), nome/cor inválidos, código de moeda inválido,
      `AddAsync` não chamado quando a validação falha.
- [x] Registro de DI do use case em `Denarius.Application/DependencyInjection.cs`
      Verificação: `dotnet build` resolve `ICreateAccountUseCase` sem erro em runtime.
- [x] Endpoint `POST /api/accounts` mapeado em `AccountEndpoints.MapAccountEndpoints`,
      `UserId` extraído do `ClaimsPrincipal`, requer autenticação
      Verificação: `tests/Denarius.Api.Tests/Endpoints/AccountEndpointsTests.cs` — retorna 201
      com `Location: /api/accounts/{id}` no happy path, 400 nos cenários de erro, 401 sem token.
- [x] Mapeamento de exceções → HTTP via `ExceptionMiddleware`
      Verificação: `InvalidNameException`, `InvalidCurrencyCodeException`, `InvalidColorException`
      (todas `DomainException`) retornam 400.
- [ ] Nenhuma pendência conhecida — suíte completa (`dotnet test`) deve passar sem falhas antes
      de qualquer nova alteração neste fluxo.
      Verificação: `dotnet test` na raiz de `api/` sem falhas.
