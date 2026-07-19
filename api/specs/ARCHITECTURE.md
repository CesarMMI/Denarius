# Architecture

Decisões técnicas globais do projeto. Este documento é referência viva —
specs de feature devem *citar* este arquivo, não repetir seu conteúdo. Setup,
stack e comandos do dia a dia estão em `specs/DEVELOPMENT.md`, não aqui.

## Estilo arquitetural

- Clean Architecture, com testes cobrindo as quatro camadas
- API exposta via **Minimal API**

### Camadas e dependências

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

> **Nota:** `InvalidCredentialsException` (falha de login) herda `AppException` e
> retorna **400**, não 401. Isso é intencional — não expor se o e-mail existe no
> sistema.

## Autenticação

- Senhas nunca trafegam nem são persistidas em texto puro — hashing via
  `IPasswordHasher`; entidades de domínio (`User`) só recebem o hash já computado.
- Login retorna token JWT.
- Falhas de autenticação (email inexistente, senha incorreta) sempre retornam o
  mesmo erro genérico (`InvalidCredentialsException`, mesma mensagem) para evitar
  enumeração de usuários.

## Endpoints REST

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

Todos os endpoints de `/api/accounts`, `/api/categories` e `/api/transactions`
requerem `Authorization: Bearer <token>`.

## Configuração via variáveis de ambiente

Quem guarda qual arquivo/chave está em `specs/DEVELOPMENT.md` ("Configuração
— quem guarda o quê"). Decisão registrada aqui, por ser específica desta
camada de infra e não repetir em outras features:

- **Arrays viraram strings CSV**: `Cors:AllowedOrigins` (e Methods/Headers) migrou
  de array JSON indexado (`Cors:AllowedOrigins:0`, `:1`...) para uma única string
  separada por vírgula, lida via `configuration["Cors:AllowedOrigins"]` +
  `Split(',')` manual em `AddCorsPolicy` (`Extensions/ServiceCollectionExtensions.cs`).
  Motivo: variáveis de ambiente num `.env` plano não representam bem arrays
  indexados (`Cors__AllowedOrigins__0`, `__1`...) de forma prática.
- Convenção resultante: `Cors__AllowedOrigins=${CORS_ALLOWED_ORIGINS}` no
  docker-compose, valor CSV no `.env`.
- Vazio em qualquer uma das três chaves de CORS ativa `AllowAny*` (permissivo).
- `AllowedHosts` é exceção deliberada: fica só em `appsettings.json` (raramente
  varia por ambiente).

## Docker e infraestrutura

- **Sem compose na raiz do repositório.** Cada projeto do monorepo (`api/`,
  futuramente `front/`) tem seu próprio `Dockerfile` + `.dockerignore` +
  `docker-compose.yml` (base, prod-safe) + `docker-compose.override.yml` (dev,
  auto-mesclado) — tudo isolado dentro da própria pasta do projeto, executado
  separadamente. Já existiu uma versão com compose raiz usando `include:` para
  trazer `api/docker-compose.yml` (testada e funcional), mas foi abandonada:
  não se quer subir front e api juntos por padrão.
- **Dockerfile multi-stage**: `sdk:9.0` (build) → `aspnet:9.0` (runtime), usuário
  non-root `app`.
- **Banco: `mariadb:12.3-noble`**, não `mysql:8.4` (~58% menor). `ServerVersion.AutoDetect`
  do Pomelo.EntityFrameworkCore.MySql já reconhece MariaDB sem mudança de código
  ou pacote NuGet. Healthcheck usa `mariadb-admin` (não `mysqladmin`).
- **Migrations automáticas no startup**, via `DatabaseMigrationExtensions.ApplyPendingMigrations()`,
  guardadas pela env var `RUN_MIGRATIONS_ON_STARTUP` (default `false`) — migrar
  incondicionalmente quebraria `WebApplicationFactory<Program>` nos testes de
  integração, que usa connection string falsa.
- Container só expõe HTTP (`ASPNETCORE_HTTP_PORTS=8080`); `UseHttpsRedirection()`
  vira no-op sem porta HTTPS configurada.
- Segredos via `api/.env` (gitignorado pela regra `.env` já existente no
  `.gitignore` raiz, que casa em qualquer profundidade), a partir de `api/.env.example`.
- **Docker é obrigatório só para o MariaDB em dev** (`docker compose up
  mariadb`) — a API roda nativa via `dotnet watch run`. Em produção (ou para
  validar a imagem antes de deploy), API e banco sobem juntos via
  `docker compose -f docker-compose.yml up`. Detalhes completos do fluxo de
  dev/prod: `specs/DEVELOPMENT.md`.

## Referências

- Setup, stack, comandos: `specs/DEVELOPMENT.md`
- Entidades: `specs/entities/`
- Specs de feature por contexto: `specs/features/<contexto>/spec.md`
