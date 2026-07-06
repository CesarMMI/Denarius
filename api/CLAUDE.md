# CLAUDE.md — Denarius API

## 1. Stack & Ambiente

**Linguagem / Runtime**
- C# 13, .NET 9.0, `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`

**Principais bibliotecas**
| Biblioteca | Uso |
|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer 9.*` | Autenticação JWT |
| `Microsoft.AspNetCore.OpenApi 9.*` | Spec OpenAPI gerada em build |
| `Pomelo.EntityFrameworkCore.MySql 9.*` | ORM + driver MySQL |
| `BCrypt.Net-Next 4.2.0` | Hash de senha |
| `System.IdentityModel.Tokens.Jwt 8.*` | Geração de tokens JWT |
| `xunit 2.9.2` | Testes (todos os projetos) |
| `NSubstitute 5.3.0` | Mocking (testes de Api e Application) |
| `Microsoft.EntityFrameworkCore.Sqlite 9.*` | BD in-memory nos testes de Infrastructure |

**Build**
```
dotnet build
```

**Testes**
```
dotnet test
```

**Rodar localmente**
```
dotnet run --project src/Denarius.Api
```
A API sobe em HTTPS. O spec OpenAPI fica disponível em `GET /openapi/v1.json` quando `ASPNETCORE_ENVIRONMENT=Development`.

**Migrations (EF Core)**
```
dotnet ef migrations add <NomeDaMigration> \
  --project src/Denarius.Infrastructure \
  --startup-project src/Denarius.Api
```
```
dotnet ef database update \
  --project src/Denarius.Infrastructure \
  --startup-project src/Denarius.Api
```

**Variáveis de configuração necessárias** (`appsettings.Development.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=denarius;User=root;Password=root;"
  },
  "Jwt": {
    "Secret": "<mínimo 32 caracteres>"
  }
}
```
`Jwt:Issuer`, `Jwt:Audience` e `Jwt:ExpiryMinutes` estão em `appsettings.json`.

---

## 2. Arquitetura

O projeto segue **Clean Architecture** com quatro camadas separadas em projetos distintos:

```
src/
  Denarius.Domain/          # Entidades, enums, exceções de domínio, interfaces de repositório
  Denarius.Application/     # Use cases, inputs, outputs, exceções de aplicação, interfaces de serviço
  Denarius.Infrastructure/  # EF Core DbContext, repositórios, auth (JWT + BCrypt), DI
  Denarius.Api/             # Minimal API endpoints, request models, middleware, extensões de DI

tests/
  Denarius.Domain.Tests/          # Testes unitários de entidades (xUnit puro)
  Denarius.Application.Tests/     # Testes unitários de use cases (NSubstitute)
  Denarius.Infrastructure.Tests/  # Testes de repositórios com SQLite in-memory
  Denarius.Api.Tests/             # Testes de integração de endpoints (WebApplicationFactory + TestAuthHandler)
```

**Dependências entre projetos**
```
Api → Application + Infrastructure
Application → Domain
Infrastructure → Domain + Application
```

### Onde ficam as peças

| Artefato | Localização |
|---|---|
| Entidades de domínio | `src/Denarius.Domain/Entities/` |
| Enums | `src/Denarius.Domain/Enums/` |
| Exceções de domínio | `src/Denarius.Domain/Exceptions/` e subpastas por aggregate (`Accounts/`, `Transactions/`, `Users/`) |
| Interfaces de repositório | `src/Denarius.Domain/Interfaces/Repositories/` |
| Interfaces de use case | `src/Denarius.Application/Interfaces/UseCases/<Aggregate>/` |
| Interface base de use case (`IUseCase<T, U>`) | `src/Denarius.Application/Interfaces/` |
| Interfaces de serviço (`IPasswordHasher`, `ITokenService`) | `src/Denarius.Application/Interfaces/` |
| Inputs de use case | `src/Denarius.Application/Inputs/<Aggregate>/` |
| Outputs de use case | `src/Denarius.Application/Outputs/<Aggregate>/` |
| Implementações de use case | `src/Denarius.Application/UseCases/<Aggregate>/` |
| Exceções de aplicação | `src/Denarius.Application/Exceptions/<Aggregate>/` |
| Registro de DI da Application | `src/Denarius.Application/DependencyInjection.cs` |
| DbContext e configurações EF | `src/Denarius.Infrastructure/Persistence/` |
| Repositórios | `src/Denarius.Infrastructure/Persistence/Repositories/` |
| Serviços de auth | `src/Denarius.Infrastructure/Auth/` |
| Registro de DI da Infrastructure | `src/Denarius.Infrastructure/DependencyInjection.cs` |
| Endpoints Minimal API | `src/Denarius.Api/Endpoints/` |
| Request models (API) | `src/Denarius.Api/Requests/<Aggregate>/` |
| Middleware de exceções | `src/Denarius.Api/Middleware/ExceptionMiddleware.cs` |
| Extensões de DI e helpers | `src/Denarius.Api/Extensions/` |

### Convenções de nomenclatura

| Tipo | Padrão | Exemplo |
|---|---|---|
| Interface base de use case | `IUseCase<TInput, Task<TOutput>>` | `IUseCase<CreateAccountInput, Task<AccountOutput>>` |
| Interface de use case | `I<Verbo><Agregado>UseCase` herda `IUseCase<…>` | `ICreateAccountUseCase` |
| Implementação de use case | `<Verbo><Agregado>UseCase` | `CreateAccountUseCase` |
| Input | `<Verbo><Agregado>Input` (record) | `CreateAccountInput` |
| Output | `<Agregado>Output` (record com `FromEntity()`) | `AccountOutput` |
| Output composto | `<Verbo><Agregado>Output` (record sem `FromEntity()`, agrupa outputs) | `CreateTransferOutput` |
| Request (API) | `<Verbo><Agregado>Request` (record) | `CreateAccountRequest` |
| Endpoint mapper | `Map<Agregado>Endpoints` (extensão de `IEndpointRouteBuilder`) | `MapAccountEndpoints` |
| Repositório (interface) | `I<Agregado>Repository` | `IAccountRepository` |
| Repositório (impl.) | `<Agregado>Repository` | `AccountRepository` |
| Exceção de domínio | `Invalid<Campo>Exception`, `<Motivo>Exception` | `InvalidCurrencyCodeException` |
| Exceção de aplicação | herda `NotFoundException` ou `AppException` | `AccountNotFoundException` |

### Mapeamento de exceções → HTTP (`ExceptionMiddleware`)

| Exceção | Status |
|---|---|
| `NotFoundException` (e subclasses) | 404 |
| `InactiveAccountException` | 422 |
| `InvalidDateRangeException` | 400 |
| `DomainException` (e subclasses) | 400 |
| `AppException` (e subclasses) | 400 |
| Qualquer outra | 500 |

> **Nota:** `InvalidCredentialsException` (falha de login) herda `AppException` e retorna **400**, não 401. Isso é intencional — não expor se o e-mail existe no sistema.

---

## 3. Domínio

Documentação detalhada de cada entidade e aggregate em `.claude/entities/` e `.claude/use-cases/`.

Lista completa de features com status de implementação: [`harness/feature_list.json`](harness/feature_list.json)

### Entidades

| Entidade | Documentação |
|---|---|
| `User` | [`.claude/entities/user.md`](.claude/entities/user.md) |
| `Account` | [`.claude/entities/account.md`](.claude/entities/account.md) |
| `Category` | [`.claude/entities/category.md`](.claude/entities/category.md) |
| `Transaction` | [`.claude/entities/transaction.md`](.claude/entities/transaction.md) |

**Hierarquia de propriedade:** `User` → `Account`, `Category`, `Transaction`. Toda query a repositório filtra por `UserId` — nunca retornar dados de outro usuário.

**Enums**
- `CategoryType`: `Income`, `Expense`
- `TransactionType`: `Income`, `Expense`, `Transfer`

### Use Cases existentes

| Aggregate | Use Cases | Documentação |
|---|---|---|
| Auth | `RegisterUser`, `Login` | [`.claude/use-cases/use-cases-auth.md`](.claude/use-cases/use-cases-auth.md) |
| Account | `CreateAccount`, `GetAccountById`, `ListAccounts`, `UpdateAccount`, `DeactivateAccount` | [`.claude/use-cases/use-cases-account.md`](.claude/use-cases/use-cases-account.md) |
| Category | `CreateCategory`, `GetCategoryById`, `ListCategories`, `UpdateCategory`, `DeleteCategory` | [`.claude/use-cases/use-cases-category.md`](.claude/use-cases/use-cases-category.md) |
| Transaction | `CreateTransaction`, `CreateTransfer`, `GetTransactionById`, `ListTransactions`, `UpdateTransaction`, `DeleteTransaction` | [`.claude/use-cases/use-cases-transaction.md`](.claude/use-cases/use-cases-transaction.md) |

### Endpoints REST

```
POST   /api/auth/register
POST   /api/auth/login

GET    /api/accounts
GET    /api/accounts/{id}
POST   /api/accounts
PUT    /api/accounts/{id}
DELETE /api/accounts/{id}       → desativa (204)

GET    /api/categories
GET    /api/categories/{id}
POST   /api/categories
PUT    /api/categories/{id}
DELETE /api/categories/{id}

GET    /api/transactions
GET    /api/transactions/{id}
POST   /api/transactions
POST   /api/transactions/transfer
PUT    /api/transactions/{id}
DELETE /api/transactions/{id}
```

Todos os endpoints de `/api/accounts`, `/api/categories` e `/api/transactions` requerem `Authorization: Bearer <token>`.

---

## 4. Regras para o Agente

### Antes de qualquer tarefa
1. Executar `dotnet build` — se falhar, **parar e reportar o erro** antes de qualquer alteração.
2. Executar `dotnet test` — se falhar, **parar e reportar** quais testes estão quebrando.

### Durante a tarefa
- Trabalhar em **uma tarefa por vez**.
- Nunca declarar "done" sem `dotnet build` e `dotnet test` passando sem erros.
- Ao adicionar um novo use case:
  1. Criar a interface em `Denarius.Application/Interfaces/UseCases/<Aggregate>/`
  2. Criar o input em `Denarius.Application/Inputs/<Aggregate>/`
  3. Criar o output em `Denarius.Application/Outputs/<Aggregate>/` (se necessário)
  4. Implementar o use case em `Denarius.Application/UseCases/<Aggregate>/`
  5. Registrar o par interface→implementação em `Denarius.Application/DependencyInjection.cs`
  6. Mapear o endpoint em `Denarius.Api/Endpoints/<Aggregate>Endpoints.cs`
- Ao adicionar uma entidade de domínio:
  1. Criar a entidade em `Denarius.Domain/Entities/`
  2. Criar a interface de repositório em `Denarius.Domain/Interfaces/Repositories/`
  3. Implementar o repositório em `Denarius.Infrastructure/Persistence/Repositories/`
  4. Criar a configuração EF em `Denarius.Infrastructure/Persistence/Configurations/`
  5. Registrar DbSet e repositório em `DenariusDbContext` e `DependencyInjection` da Infrastructure
  6. Criar migration

### Documentação faltante

Se durante a implementação você perceber que um contexto não possui documentação markdown correspondente (entidade sem `*.md` em `.claude/entities/`, use case sem `*.md` em `.claude/use-cases/`, etc.):

1. Crie o arquivo markdown **antes** de implementar
2. Documente o que o contexto deve conter com base no que inferiu do código existente
3. Sinalize com `<!-- inferido do código — confirmar com o desenvolvedor -->` em cada bloco que não estava explícito
4. Referencie o novo arquivo nas tabelas da seção 3 deste `CLAUDE.md`
5. Só então prossiga com a implementação

**Nunca implemente algo sem que exista documentação correspondente.**

### Testes
- Testes de **domínio**: xUnit puro, sem mocking, em `Denarius.Domain.Tests/Entities/`
- Testes de **use case**: NSubstitute para repositórios e serviços, em `Denarius.Application.Tests/UseCases/<Aggregate>/`
- Testes de **repositório**: herdar `RepositoryTestBase` (SQLite in-memory), em `Denarius.Infrastructure.Tests/Repositories/`
- Testes de **endpoints**: usar `IClassFixture<WebApplicationFactory<Program>>` com a extensão `WebApplicationFactoryExtensions.CreateTestClient()` (definida em `Denarius.Api.Tests/Shared/CustomWebApplicationFactory.cs`), NSubstitute para use cases injetados, em `Denarius.Api.Tests/Endpoints/`

### Ao terminar
Descrever:
- O que foi feito (arquivos criados/modificados)
- O que ainda está pendente ou requer atenção do desenvolvedor

---

## 5. O que NÃO fazer

- **Não modificar** `appsettings.json` ou `appsettings.Development.json` sem avisar explicitamente ao usuário.
- **Não instalar pacotes NuGet** (`<PackageReference>`) sem confirmar com o usuário.
- **Não alterar contratos públicos** — interfaces de use case (`IXxxUseCase`), records de Input/Output e request models da API — sem aprovação explícita. Quebrar um contrato implica reescrever todos os consumidores e potencialmente quebrar clientes da API.
- **Não usar Controllers** — o projeto usa exclusivamente Minimal APIs com grupos e extensões de `IEndpointRouteBuilder`.
- **Não expor propriedades de entidade de domínio como `public set`** — todas as mutações passam por métodos do domínio (ex: `Update()`, `ApplyDelta()`, `Deactivate()`).
- **Não chamar `unitOfWork.CommitAsync()` mais de uma vez por use case** — cada use case abre uma única unidade de trabalho.
- **Não retornar entidades de domínio diretamente** nos outputs — sempre mapear para um record `XxxOutput` com `FromEntity()`.
- **Não criar arquivos fora das camadas corretas** — respeitar a estrutura de projetos descrita na seção 2.
