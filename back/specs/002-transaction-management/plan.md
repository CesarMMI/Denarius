# Plano de implementação: Gestão de transações

**Branch**: `002-transaction-management` | **Data**: 2026-09-22 | **Spec**: [spec.md](./spec.md)

**Entrada**: Especificação da feature em `/specs/002-transaction-management/spec.md`

**Observação**: Este é um plano **retroativo** — documenta o design já entregue desta feature,
conferido com o código em `src/` e as suítes de testes em `tests/`, em vez de propor trabalho
novo. Um `/speckit-tasks` rodado sobre este plano não deve encontrar nada pendente além da lacuna
de documentação apontada na pesquisa. Atualizado em 2026-09-24 para cobrir a História de usuário 3
(filtro e ordenação da lista), projetada e entregue na mesma mudança desta atualização.

## Resumo

A gestão de transações oferece CRUD de transações financeiras individuais — data, valor (positivo
ou negativo), referência obrigatória a uma categoria e descrição opcional. Ela é implementada como
uma única fatia vertical pela arquitetura de quatro camadas existente: a entidade de domínio
`Transaction`, que se autovalida, impõe as regras de negócio; quatro casos de uso da camada
Application, cada um com um único propósito (Create/Update/Delete/List), orquestram a
persistência, e Create/Update também verificam se a `Category` referenciada existe; o EF Core
persiste `Transaction` no PostgreSQL, com uma chave estrangeira restrict-on-delete para
`Category`; um único `TransactionsController` expõe os casos de uso via REST; e o
`GlobalExceptionHandler` converte as exceções de domínio/não encontrado em respostas
`ProblemDetails`. Como em [001-category-management](../001-category-management/plan.md), o caso de
uso `List` recebe um `ListTransactionsInput` (descrição, mês `DateRef`, tipo, categoria,
`orderBy`/`asc`) e aplica os filtros e a ordenação em memória sobre as transações carregadas; não
há paginação.

## Alteração de 2026-10-11: filtros e ordenação no banco

Pedido e decisões na nota de alteração da [spec](./spec.md) (FR-013, FR-018 a FR-021, história 4 e SC-007 a SC-010),
aprovada pelo usuário em 2026-10-11, com o plan aprovado na mesma data. Decisões e alternativas em
[research.md](./research.md#alteração-de-2026-10-11-filtros-e-ordenação-no-banco). Esta seção substitui, para a lista, o
que o Resumo acima diz sobre filtrar "em memória".

**Resumo**: A lista de transações (`GET /api/transactions`) hoje lê todas as transações e filtra e ordena em memória no
`ListTransactionsUseCase`. Passa a ser uma consulta SQL: um novo `ITransactionRepository.ListAsync(TransactionListQuery)`
aplica busca, mês, tipo, categoria e ordenação no PostgreSQL, e o caso de uso só traduz o input e monta os outputs. A
busca ignora acentos (`unaccent`, como a de categorias), a ordem alfabética usa uma collation ICU pt-BR declarada em
migration, e o mês vira o intervalo meio-aberto `[início, início do mês seguinte)`. Rotas, parâmetros e formato da
resposta não mudam; o contrato vai para a 1.3.0.

**Contexto técnico da alteração**: C# / .NET 10; EF Core 10.0.11 e Npgsql EF Core 10.0.3 (nenhuma dependência nova);
PostgreSQL 18 (extensão `unaccent` já instalada; collation `pt_br_icu` nova); xUnit, com Testcontainers
(`postgres:18.0`) nos testes de repositório e de migration. Meta: SC-008 (lista do mês e da categoria pelo menos 5
vezes mais rápida, 10.000 transações, 200 no filtro, mediana de 20 requisições). Volume: centenas a poucos milhares de
transações por ano, sem paginação.

### Constitution Check da alteração

| Princípio | Resultado | Como |
| --- | --- | --- |
| I. TDD | PASSA | Os testes do `tasks.md` vêm antes do código: repositório (Testcontainers), caso de uso (mocks) e migration. Os testes atuais de `ListTransactionsUseCaseTests` que simulam `GetAllAsync` passam a simular `ListAsync` e mantêm as asserções de saída; nenhum é apagado ou enfraquecido. Os que hoje verificam filtro e ordem feitos pelo caso de uso migram para o teste de repositório, onde isso passa a acontecer; o `tasks.md` traz a tabela "teste antigo → teste novo" que torna isso rastreável, e a tradução de input em consulta (trim, branco vira nulo, tipo, campo, direção, `Kind` UTC) fica coberta no caso de uso |
| II. Segurança | PASSA | Texto buscado como parâmetro (`Unaccent`/`Contains` do EF); o nome da collation é constante; nenhum parâmetro de entrada novo; erros seguem pelo `GlobalExceptionHandler`; nada de descrição ou busca em log (FR-021). Nenhuma dependência nova |
| III. Performance | PASSA | Filtros e ordenação na fonte (é o objetivo); `AsNoTracking()`; índices em `Date` e `CategoryId` já existem; busca e tipo não ganham índice sem medição. SC-008 medido antes e depois |
| IV. Contrato | PASSA | `contracts/transactions-api.yaml` 1.3.0, aditivo; o front não muda. A ordem de strings passa a ser independente do ambiente (FR-018) |
| V. Escopo e simplicidade | PASSA | Só a lista de transações. A listagem de categorias fica como está (dívida registrada na spec). Sem abstração nova além da consulta tipada |
| B1. Camadas | PASSA | Consulta no `Infrastructure` atrás de interface do `Domain`; os tipos da consulta ficam no `Domain`; o `Application` não vê EF |
| B2. Compatibilidade | PASSA, mudança aditiva | Rotas, parâmetros, status e `ProblemDetails` iguais; contrato 1.3.0 em `contracts/` |
| B3. Migrations | PASSA, a cumprir no implement | Uma migration cria a collation; o `Down()` gerado pelo Npgsql já traz o `DROP COLLATION pt_br_icu` (conferido em `dotnet ef migrations script`), então não há SQL explícito, e a migration roda numa transação; `Up()` e `Down()` aplicados, revertidos e reaplicados numa base descartável, conferindo `pg_collation` |
| Idioma | PASSA | Artefatos em português; código e comentários em inglês |

### Código da alteração

```text
back/
├── src/
│   ├── Denarius.Domain/Repositories/
│   │   ├── ITransactionRepository.cs      # + ListAsync(TransactionListQuery)
│   │   └── TransactionListQuery.cs        # novo, com TransactionKind e TransactionSortField
│   ├── Denarius.Application/UseCases/Transactions/List/
│   │   └── ListTransactionsUseCase.cs     # só traduz o input; sem ICategoryRepository
│   └── Denarius.Infrastructure/
│       ├── Persistence/DenariusDbContext.cs          # + HasCollation("pt_br_icu", ...)
│       ├── Repositories/TransactionRepository.cs     # + ListAsync
│       └── Migrations/<timestamp>_AddPortugueseCollation.cs   # nova
└── tests/
    ├── Denarius.Infrastructure.Tests/Repositories/TransactionRepositoryTests.cs   # novo; inclui o caso de uso real contra o repositório, com `dateRef`
    ├── Denarius.Infrastructure.Tests/Migrations/PortugueseCollationMigrationTests.cs   # novo; migra até o alvo anterior, aplica, reverte e reaplica com `IMigrator`
    ├── Denarius.Application.Tests/UseCases/Transactions/List/ListTransactionsUseCaseTests.cs   # adaptado
    └── Denarius.WebAPI.Tests/Transactions/TransactionsControllerTests.cs   # contrato da lista, sem mudança de formato
```

Sem projeto novo; segue o `CategoryRepository`/`CategoryRepositoryTests` da correção da busca de categorias.

### Testes previstos

- **Repositório (Testcontainers)**: busca (acentos, caixa, espaços já aparados pelo caso de uso, `%`, `_`, `\`, nulos),
  mês (00:00 do dia 1 e 23:59:59,999 do último dia), tipo, categoria, combinação, cada campo de ordenação nas duas
  direções com desempates, posição das sem descrição, ordem "agua, Agua, água, Água, Banana", nome da categoria atual.
- **Caso de uso (mocks)**: conversão de input em consulta (trim, mês em intervalo UTC, tipo, campo, direção) e mapeamento
  do output.
- **Migration**: aplicar, reverter e reaplicar a collation.
- **WebAPI**: formato e parâmetros da lista inalterados.

### Dependências e riscos da alteração

- **Docker desligado na máquina de desenvolvimento** (`docker version` falhou em 2026-10-11): só o
  `Denarius.Infrastructure.Tests` usa banco real (Testcontainers); o `Denarius.WebAPI.Tests` usa `TestServer` com casos
  de uso falsos. Subir o Docker Desktop antes do implement. Como a WebAPI não toca o banco, o caminho completo de
  `dateRef` (model binding, caso de uso e repositório) é coberto no `Infrastructure.Tests`.
- **Ordem de implantação**: a migration vai antes do código novo, porque `COLLATE "pt_br_icu"` com a collation
  inexistente responde `500`; o inverso também vale (reverter a migration com o código novo no ar quebra a lista). A
  migration, sozinha, é compatível com o código antigo. Se a migration falhar, o código novo ainda não está no ar e a
  lista continua como antes; reverter é rodar o `Down()` (que remove a collation) e só depois voltar o código antigo.
- **Collation ICU**: assume PostgreSQL com ICU (o `postgres:18.0` oficial tem). Em instalação sem ICU, a migration falha
  explicitamente, sem ordenar errado em silêncio.
- **Base de dev**: ainda falta aplicar a migration `AddUnaccentExtension` (da correção da busca de categorias); o
  `dotnet ef database update` aplica as duas.
- **SC-008**: pode não chegar a 5 vezes se a latência fixa de HTTP dominar; a medição de base decide, e o resultado sobe
  para o usuário se o alvo não for atingido.
- **Pós-alteração**: atualizar a exceção do `back/AGENTS.md` por `/speckit-constitution` (tarefa T053, só com o aval do
  usuário) e a spec 003 do front pelo fluxo do front, com a aprovação do usuário (fora do `tasks.md` do back).

## Contexto técnico

**Linguagem/versão**: C# 13 / .NET 10 (`net10.0`)

**Dependências principais**: ASP.NET Core (`Microsoft.AspNetCore.OpenApi`) para o host web; Entity
Framework Core 10 + `Npgsql.EntityFrameworkCore.PostgreSQL` para a persistência;
`Microsoft.Extensions.DependencyInjection.Abstractions` para os contratos de DI da camada
Application

**Armazenamento**: PostgreSQL, acessado via migrations code-first do EF Core
(`Denarius.Infrastructure/Migrations`); uma tabela `Transactions` com uma chave estrangeira
restrict-on-delete para `Categories.Id` e um índice não único em `CategoryId`

**Testes**: xUnit nos três projetos de teste; `Denarius.Domain.Tests` e
`Denarius.Application.Tests` usam NSubstitute para os dublês de repositório/unit of work;
`Denarius.WebAPI.Tests` usa `Microsoft.AspNetCore.TestHost` para testes de integração in-process

**Plataforma-alvo**: ASP.NET Core Web API (lado do servidor), consumida pelo front-end Angular do
Denarius (projeto separado, `front/`) e acessível em `api/transactions`

**Tipo de projeto**: web — a metade backend de uma aplicação web de dois projetos; este plano
cobre só o backend

**Metas de desempenho**: Nenhuma especificada formalmente. Comportamento atual:
`GET /api/transactions` carrega todas as transações em memória uma vez por requisição e depois
filtra e ordena no caso de uso (mais uma consulta extra para os nomes das categorias, só quando a
ordenação é por `CategoryName`) — a mesma postura de "aceitável na escala atual" do
`ListCategoriesUseCase`. Levar os filtros para a consulta do repositório é o próximo passo natural
se os volumes superarem isso (veja research.md → Filtro e ordenação da lista).

**Restrições**: Regidas por `.specify/memory/constitution.md` — as respostas da API precisam
continuar retrocompatíveis (Princípio I), o código precisa ficar na sua camada da Clean
Architecture (Princípio II), qualquer migration futura precisa de um rollback verificado
(Princípio III) e o `dotnet test` precisa passar nas três suítes (Princípio IV).

**Escala/escopo**: Uso single-tenant de finanças pessoais; por usuário, de centenas a poucos
milhares de transações — a lista sem paginação e filtrada em memória foi dimensionada para essa
faixa, e não para grandes volumes multi-tenant.

## Constitution Check

*PORTÃO: precisa passar antes da pesquisa da Fase 0. Conferir de novo após o design da Fase 1.*

| Princípio | Status | Evidência |
|---|---|---|
| I. Compatibilidade da API pública | PASSA | A História de usuário 3 é **aditiva**: `GET /api/transactions` ganha seis parâmetros de query opcionais (`description`, `dateRef`, `type`, `categoryId`, `orderBy`, `asc`), e uma chamada sem nenhum deles continua devolvendo todas as transações, com o formato de resposta inalterado. A única diferença observável para quem já chama a API é que a ordem, antes não especificada, agora é data decrescente. Nenhuma outra rota, DTO ou código de status mudou. |
| II. Respeito às fronteiras de serviço | PASSA | A validação fica em `Denarius.Domain` (`Transaction`); a orquestração, nos casos de uso de `Denarius.Application`, que dependem só de `Domain` (mais o `ICategoryRepository`, para a verificação de existência entre entidades e para os nomes das categorias na ordenação por `CategoryName`); os detalhes do EF Core ficam confinados a `Denarius.Infrastructure`; o `TransactionsController` de `Denarius.WebAPI` só faz o binding dos parâmetros de query e chama interfaces de casos de uso. Nenhuma camada é pulada. |
| III. Disciplina de rollback de migrations | PASSA | A migration que cria a tabela `Transactions` (`20260814132713_AddTransaction`) tem um `Down()` limpo e não destrutivo (`DropTable`). A História de usuário 3 não adiciona migration. |
| IV. Verificação da suíte de testes | PASSA | A entidade `Transaction` e os quatro casos de uso têm cobertura própria em xUnit; o próprio `TransactionsController` é coberto por `tests/Denarius.WebAPI.Tests/Transactions/TransactionsControllerTests.cs` (adicionado pelo `/speckit-implement` em 2026-09-22, fechando a lacuna que a pesquisa tinha apontado). Os filtros, as opções de ordenação, o binding da query e o comportamento de 400 para parâmetro inválido da História de usuário 3 são cobertos em `ListTransactionsUseCaseTests.cs` e `TransactionsControllerTests.cs`. |

Nenhuma violação — o Acompanhamento de complexidade não é necessário.

*Conferido de novo após o design da Fase 1: sem mudança — data-model.md e contracts/ descrevem o
design entregue e não introduzem nada novo que precise passar pelo portão de novo.*

## Estrutura do projeto

### Documentação (desta feature)

```text
specs/002-transaction-management/
├── plan.md               # Este arquivo (saída do comando /speckit-plan)
├── research.md           # Saída da Fase 0 (comando /speckit-plan)
├── data-model.md         # Saída da Fase 1 (comando /speckit-plan)
├── quickstart.md         # Saída da Fase 1 (comando /speckit-plan)
├── contracts/            # Saída da Fase 1 (comando /speckit-plan)
└── tasks.md              # Saída da Fase 2 (comando /speckit-tasks - NÃO é criado pelo /speckit-plan)
```

### Código-fonte (raiz do repositório)

```text
src/
├── Denarius.Domain/
│   ├── Entities/Transaction.cs
│   └── Repositories/ITransactionRepository.cs
├── Denarius.Application/
│   ├── IO/Transactions/               # CreateTransactionInput, UpdateTransactionInput,
│   │                                   # ListTransactionsInput, TransactionOutput,
│   │                                   # TransactionType, TransactionOrderField
│   └── UseCases/Transactions/         # Create, Update, Delete, List
├── Denarius.Infrastructure/
│   ├── Persistence/Configurations/TransactionConfiguration.cs
│   ├── Repositories/TransactionRepository.cs
│   └── Migrations/20260814132713_AddTransaction.cs
└── Denarius.WebAPI/
    └── Controllers/TransactionsController.cs

tests/
├── Denarius.Domain.Tests/Entities/TransactionTests.cs
├── Denarius.Application.Tests/UseCases/Transactions/   # uma pasta por caso de uso
└── Denarius.WebAPI.Tests/Transactions/TransactionsControllerTests.cs
```

**Decisão de estrutura**: A mesma de [001-category-management](../001-category-management/plan.md)
— a Opção 2 do template (aplicação web: backend/frontend separados) restrita ao backend, com o
layout real de Clean Architecture deste repositório (`Denarius.Domain` / `Denarius.Application` /
`Denarius.Infrastructure` / `Denarius.WebAPI` em `src/`, espelhados 1:1 em `tests/`) em vez da
divisão genérica `models/services/api` do template.

## Acompanhamento de complexidade

Não se aplica — o Constitution Check não apontou violações.
