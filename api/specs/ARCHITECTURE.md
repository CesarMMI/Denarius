# Architecture

Decisões técnicas globais do projeto. Este documento é referência viva —
specs de feature devem *citar* este arquivo, não repetir seu conteúdo.

## Estilo arquitetural

- Clean Architecture, com testes cobrindo as quatro camadas (368 testes, 0 falhas)
- API exposta via **Minimal API**

## Autenticação

- Senhas nunca trafegam nem são persistidas em texto puro — hashing via
  `IPasswordHasher`; entidades de domínio (`User`) só recebem o hash já computado.
- Login retorna token JWT.
- Falhas de autenticação (email inexistente, senha incorreta) sempre retornam o
  mesmo erro genérico (`InvalidCredentialsException`, mesma mensagem) para evitar
  enumeração de usuários.

## Configuração via variáveis de ambiente

- `Cors:AllowedOrigins/Methods/Headers` e `Logging:LogLevel:*` são configuráveis
  via env var, não apenas `appsettings.json`.
- **Arrays viraram strings CSV**: `Cors:AllowedOrigins` (e Methods/Headers) migrou
  de array JSON indexado (`Cors:AllowedOrigins:0`, `:1`...) para uma única string
  separada por vírgula, lida via `configuration["Cors:AllowedOrigins"]` +
  `Split(',')` manual em `AddCorsPolicy` (`Extensions/ServiceCollectionExtensions.cs`).
  Motivo: variáveis de ambiente num `.env` plano não representam bem arrays
  indexados (`Cors__AllowedOrigins__0`, `__1`...) de forma prática.
- Convenção resultante: `Cors__AllowedOrigins=${CORS_ALLOWED_ORIGINS}` no
  docker-compose, valor CSV no `.env`.
- Vazio em qualquer uma das três chaves de CORS ativa `AllowAny*` (permissivo) —
  mesmo comportamento de antes da migração.
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
- Comandos: dev = `docker compose up` de dentro de `api/` (mescla o override
  automaticamente); prod = `docker compose -f docker-compose.yml up` (só o base).
- Segredos via `api/.env` (gitignorado pela regra `.env` já existente no
  `.gitignore` raiz, que casa em qualquer profundidade), a partir de `api/.env.example`.
- Docker é o único jeito suportado de rodar a API — não há mais `appsettings.Development.json`
  nem script de run local fora de container.

## Referências

- Entidades: `specs/entities`
- Specs de feature por contexto: `specs/features/<contexto>/spec.md`
