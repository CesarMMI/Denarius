# Development

Setup, stack e comandos do dia a dia. Este documento é referência viva —
CLAUDE.md e specs de feature devem *citar* este arquivo, não repetir seu
conteúdo.

## Stack

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

## Build & Testes

```
dotnet build
dotnet test
```

## Rodar localmente (dev)

Abordagem híbrida: só o MariaDB roda via Docker; a API roda nativa com
`dotnet watch run` (hot reload, debugger direto na IDE). O front (quando
dockerizado) roda em um compose próprio, separado — cada projeto sobe
independente, nunca via um compose compartilhado.

1. Suba só o banco:
   ```
   docker compose up mariadb
   ```
   `docker-compose.override.yml` é mesclado automaticamente, expõe a porta
   `3306` ao host e fixa `MARIADB_ROOT_PASSWORD=root` só para dev (valor
   hardcoded no override, propositalmente dissociado do `.env` — não é
   segredo real, evita ter que manter senha sincronizada entre arquivos). Não
   precisa de `.env` para este fluxo.
2. `src/Denarius.Api/appsettings.Development.json` já vem commitado (sem
   `.example` — nada ali é segredo real nem muda por desenvolvedor, então não
   há o que "preencher") com a connection string apontando pra esse MariaDB
   de dev, um `Jwt:Secret` de placeholder e `RUN_MIGRATIONS_ON_STARTUP=true`.
3. Rode a API:
   ```
   dotnet watch run --project src/Denarius.Api
   ```
   Migrations pendentes são aplicadas automaticamente no startup
   (`RUN_MIGRATIONS_ON_STARTUP=true` em `appsettings.Development.json`) via
   `ApplyPendingMigrations()` (`Extensions/DatabaseMigrationExtensions.cs`) —
   o mesmo mecanismo usado em produção, só que disparado pelo `dotnet run` em
   vez do container.

## Rodar em produção

1. Copie `api/.env.example` para `api/.env` (preencha
   `MARIADB_ROOT_PASSWORD`, `MARIADB_DATABASE`, `API_PORT` — só
   infraestrutura, não config da aplicação).
2. Copie `src/Denarius.Api/appsettings.json.example` para
   `src/Denarius.Api/appsettings.json` (gitignored — contém segredos reais) e
   preencha `Jwt:Secret` e `ConnectionStrings:DefaultConnection` (a senha
   aqui precisa bater com `MARIADB_ROOT_PASSWORD` do `.env`, já que são duas
   partes concordando na mesma credencial do banco).
3. Rode:
   ```
   docker compose -f docker-compose.yml up
   ```
   Sobe a API (container `api`, porta `${API_PORT:-8080}`, padrão `8080`) e
   o MariaDB (container `mariadb`) juntos. O `docker-compose.yml` monta
   `src/Denarius.Api/appsettings.json` dentro do container (bind mount,
   somente leitura) em vez de injetar env vars — o arquivo nunca é copiado
   para dentro da imagem (excluído via `.dockerignore`), só existe no host e
   é montado em runtime. Migrations pendentes são aplicadas automaticamente
   no startup. A API sobe só em HTTP dentro do container (TLS é
   responsabilidade de um proxy externo, se necessário).

Esse mesmo comando (com `-f docker-compose.yml`, sem o override) também serve
para validar localmente a imagem/Dockerfile antes de um deploy.

## Rodando sem Docker (bare metal)

A aplicação não tem nenhuma dependência de Docker — `Program.cs` só lê
`IConfiguration` (`appsettings.json` → `appsettings.{Environment}.json` →
env vars → args), nunca `.env`. Pra rodar `dotnet Denarius.Api.dll` (ou
publicar) direto num servidor sem Docker, o único arquivo que importa é
`src/Denarius.Api/appsettings.json` — preencha-o como no passo 2 de "Rodar
em produção" acima, apontando `ConnectionStrings:DefaultConnection` para
onde quer que o MariaDB esteja rodando (não necessariamente `mariadb`, esse
hostname só resolve dentro da rede do Compose).

## Migrations (EF Core)

Criar uma migration é feito localmente com a CLI do `dotnet ef`. Com
`appsettings.Development.json` (já commitado — ver passo 2 de "Rodar
localmente" acima), a connection string já é lida automaticamente — suba o
MariaDB primeiro (`docker compose up mariadb`) e rode:

```
dotnet ef migrations add <NomeDaMigration> \
  --project src/Denarius.Infrastructure \
  --startup-project src/Denarius.Api
```

Não é necessário rodar `dotnet ef database update` manualmente — as
migrations são aplicadas automaticamente no startup (guardado pela flag
`RUN_MIGRATIONS_ON_STARTUP`, ver `Extensions/DatabaseMigrationExtensions.cs`),
tanto pelo container `api` em produção quanto pelo `dotnet watch run` em dev.

## Configuração — quem guarda o quê

`appsettings.json` (dentro de `Denarius.Api`) é a fonte de verdade da
**aplicação**, em qualquer forma de execução (Docker ou não). `.env` guarda
só o que pertence à **infraestrutura** do Compose — coisas que a aplicação
em si não conhece:

| Arquivo | Escopo | Git |
|---|---|---|
| `api/.env` (a partir de `api/.env.example`) | `MARIADB_ROOT_PASSWORD`, `MARIADB_DATABASE`, `API_PORT` — só o container do MariaDB e o mapeamento de porta do host | gitignored (`.env`); `.env.example` commitado |
| `src/Denarius.Api/appsettings.json` (a partir de `appsettings.json.example`) | `ConnectionStrings:DefaultConnection`, `Jwt:Secret/Issuer/Audience/ExpiryMinutes`, `Cors:*`, `Logging:LogLevel:*`, `RUN_MIGRATIONS_ON_STARTUP` — toda a config da aplicação, em produção (Docker ou bare metal) | gitignored (contém segredo real); `appsettings.json.example` commitado |
| `src/Denarius.Api/appsettings.Development.json` | Mesmas chaves de app, mas com valores fixos de dev (sem segredo real — a senha bate com o `MARIADB_ROOT_PASSWORD` hardcoded em `docker-compose.override.yml`) | commitado direto, sem `.example` (real e exemplo seriam idênticos — nada ali diverge por dev ou por deploy) |

`AllowedHosts` (`"*"`) raramente precisa variar por ambiente e não está em
nenhum dos dois — fica só com o default do
`appsettings.json.example`/`appsettings.Development.json`.

A senha do MariaDB é o único valor que precisa concordar entre `.env` (que
configura a instância do banco) e `appsettings.json`/`appsettings.Development.json`
(que dizem à app como se conectar nela) — isso é inerente a dois sistemas
compartilhando uma credencial, não uma duplicação de config da aplicação.

`Cors:AllowedOrigins`/`AllowedMethods`/`AllowedHeaders` são listas separadas
por vírgula (não arrays JSON indexados) — `AddCorsPolicy`
(`Extensions/ServiceCollectionExtensions.cs`) faz o split manualmente via
`configuration["Cors:AllowedOrigins"]`. Vazio em qualquer uma delas ativa
`AllowAny*` (permissivo). Motivo dessa escolha (CSV em vez de array
indexado): ver `specs/ARCHITECTURE.md`.
