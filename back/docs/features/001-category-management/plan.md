# Plano de implementação: Gestão de categorias

**Branch**: `001-category-management` | **Data**: 2026-09-21 | **Spec**: [spec.md](./spec.md)

**Entrada**: Especificação da feature em `/specs/001-category-management/spec.md`

**Observação**: Este é um plano **retroativo** — documenta o design já entregue desta feature,
conferido com o código em `src/` e as suítes de testes em `tests/`, em vez de propor trabalho
novo. Um `/speckit-tasks` rodado sobre este plano não deve encontrar nada pendente além da lacuna
de documentação apontada na pesquisa.

## Resumo

A gestão de categorias oferece CRUD completo das categorias de gastos (nome + cor) e uma visão de
uso — quantidade de transações e saldo por categoria, opcionalmente restritos a um mês do
calendário, filtráveis por nome/uso e ordenáveis por nome/quantidade/saldo. Ela é implementada
como uma única fatia vertical pela arquitetura de quatro camadas existente: a entidade de domínio
`Category` e o value object `Color`, que se autovalidam, impõem as regras de negócio; quatro casos
de uso da camada Application, cada um com um único propósito (Create/Update/Delete/List),
orquestram a persistência e a agregação; o EF Core persiste `Category` no PostgreSQL, com uma
chave estrangeira restrict-on-delete vinda de `Transaction`; um único `CategoriesController` expõe
os casos de uso via REST; e o `GlobalExceptionHandler` converte as exceções de domínio/não
encontrado em respostas `ProblemDetails`.

## Contexto técnico

**Linguagem/versão**: C# 13 / .NET 10 (`net10.0`)

**Dependências principais**: ASP.NET Core (`Microsoft.AspNetCore.OpenApi`) para o host web; Entity
Framework Core 10 + `Npgsql.EntityFrameworkCore.PostgreSQL` para a persistência;
`Microsoft.Extensions.DependencyInjection.Abstractions` para os contratos de DI da camada
Application

**Armazenamento**: PostgreSQL, acessado via migrations code-first do EF Core
(`Denarius.Infrastructure/Migrations`); uma tabela `Categories` com uma chave estrangeira
restrict-on-delete vinda de `Transactions.CategoryId`

**Testes**: xUnit nos três projetos de teste; `Denarius.Domain.Tests` e
`Denarius.Application.Tests` usam NSubstitute para os dublês de repositório/unit of work;
`Denarius.WebAPI.Tests` usa `Microsoft.AspNetCore.TestHost` para testes de integração in-process

**Plataforma-alvo**: ASP.NET Core Web API (lado do servidor), consumida pelo front-end Angular do
Denarius (projeto separado, `front/`) e acessível em `api/categories`

**Tipo de projeto**: web — a metade backend de uma aplicação web de dois projetos; este plano
cobre só o backend

**Metas de desempenho**: Nenhuma especificada formalmente. Comportamento atual:
`GET /api/categories` carrega todas as categorias e todas as transações em memória uma vez por
requisição e agrega com LINQ (veja Pesquisa: Estratégia de agregação do uso) — aceitável na escala
atual de dados de finanças pessoais, sem benchmark além disso.

**Restrições**: Regidas por `.specify/memory/constitution.md` — as respostas da API precisam
continuar retrocompatíveis (Princípio I), o código precisa ficar na sua camada da Clean
Architecture (Princípio II), qualquer migration futura precisa de um rollback verificado
(Princípio III) e o `dotnet test` precisa passar nas três suítes (Princípio IV).

**Escala/escopo**: Uso single-tenant de finanças pessoais; dezenas de categorias e, por usuário, de
centenas a poucos milhares de transações — a lista sem paginação e a agregação completa em memória
foram dimensionadas para essa faixa, e não para grandes volumes multi-tenant.

## Constitution Check

*PORTÃO: precisa passar antes da pesquisa da Fase 0. Conferir de novo após o design da Fase 1.*

| Princípio | Status | Evidência |
|---|---|---|
| I. Compatibilidade da API pública | PASSA | Este plano documenta a superfície `api/categories` existente como está; nenhuma mudança é proposta. |
| II. Respeito às fronteiras de serviço | PASSA | A validação fica em `Denarius.Domain` (`Category`, `Color`); a orquestração, nos casos de uso de `Denarius.Application`, que dependem só de `Domain`; os detalhes do EF Core ficam confinados a `Denarius.Infrastructure`; o `CategoriesController` de `Denarius.WebAPI` só chama interfaces de casos de uso. Nenhuma camada é pulada. |
| III. Disciplina de rollback de migrations | PASSA | A migration que cria a tabela `Categories` (`20260813225006_InitialCreate`) tem um `Down()` limpo e não destrutivo (`DropTable`). |
| IV. Verificação da suíte de testes | PASSA | A entidade `Category` e os quatro casos de uso têm cobertura própria em xUnit; o próprio `CategoriesController` é coberto por `tests/Denarius.WebAPI.Tests/Categories/CategoriesControllerTests.cs` (adicionado pelo `/speckit-implement` em 2026-09-21, fechando a lacuna que a pesquisa tinha apontado). |

Nenhuma violação — o Acompanhamento de complexidade não é necessário.

*Conferido de novo após o design da Fase 1: sem mudança — data-model.md e contracts/ descrevem o
design entregue e não introduzem nada novo que precise passar pelo portão de novo.*

## Estrutura do projeto

### Documentação (desta feature)

```text
specs/001-category-management/
├── plan.md              # Este arquivo (saída do comando /speckit-plan)
├── research.md          # Saída da Fase 0 (comando /speckit-plan)
├── data-model.md         # Saída da Fase 1 (comando /speckit-plan)
├── quickstart.md         # Saída da Fase 1 (comando /speckit-plan)
├── contracts/            # Saída da Fase 1 (comando /speckit-plan)
└── tasks.md              # Saída da Fase 2 (comando /speckit-tasks - NÃO é criado pelo /speckit-plan)
```

### Código-fonte (raiz do repositório)

```text
src/
├── Denarius.Domain/
│   ├── Entities/Category.cs
│   ├── ValueObjects/Color.cs
│   └── Repositories/ICategoryRepository.cs
├── Denarius.Application/
│   ├── IO/Categories/                 # CreateCategoryInput, UpdateCategoryInput,
│   │                                   # ListCategoriesInput, CategoryOutput, CategoryOrderField
│   └── UseCases/Categories/           # Create, Update, Delete, List
├── Denarius.Infrastructure/
│   ├── Persistence/Configurations/CategoryConfiguration.cs
│   ├── Repositories/CategoryRepository.cs
│   └── Migrations/20260813225006_InitialCreate.cs
└── Denarius.WebAPI/
    └── Controllers/CategoriesController.cs

tests/
├── Denarius.Domain.Tests/Entities/CategoryTests.cs
├── Denarius.Application.Tests/UseCases/Categories/   # uma pasta por caso de uso
└── Denarius.WebAPI.Tests/                            # hoje sem testes específicos de Category (veja a pesquisa)
```

**Decisão de estrutura**: É a Opção 2 do template (aplicação web: backend/frontend separados)
restrita ao backend, com o layout real de Clean Architecture deste repositório
(`Denarius.Domain` / `Denarius.Application` / `Denarius.Infrastructure` / `Denarius.WebAPI` em
`src/`, espelhados 1:1 em `tests/`) em vez da divisão genérica `models/services/api` do template.

## Acompanhamento de complexidade

Não se aplica — o Constitution Check não apontou violações.
