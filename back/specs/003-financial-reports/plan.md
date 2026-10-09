# Plano de implementação: Relatórios financeiros

**Branch**: `003-financial-reports` | **Data**: 2026-10-03 | **Spec**: [spec.md](./spec.md)

**Entrada**: Especificação da feature em `/specs/003-financial-reports/spec.md`

## Resumo

Cinco relatórios somente leitura para o painel de relatórios do front, cada um uma fatia vertical
pelas camadas existentes: uma action `GET` num novo `ReportsController` que só faz o binding e a
validação da sua query e delega a um caso de uso da camada Application com um único propósito
(`GetMonthlySummary`, `GetExpensesByCategory`, `GetIncomeVsExpense`,
`GetCumulativeExpenseComparison`, `ListMonthlyTransactions`), cada um com os seus próprios records
de Input e Output em `IO/Reports`. Os casos de uso guardam as regras dos relatórios (mês padrão,
taxa de poupança, projeção, variações percentuais, "Outras", série contínua, totais acumulados) e
obtêm os seus números de quatro novas consultas do `ITransactionRepository`, que fazem
`GROUP BY`/`SUM` no PostgreSQL só sobre os meses pedidos. O mês trafega como um `YearMonth`
`YYYY-MM`, com binding feito pelo MVC por meio do seu `TryParse`, então um mês inválido é um `400`
antes de qualquer caso de uso rodar, exatamente como os valores inválidos de `type`/`orderBy` que já
existem. "Hoje" — e, portanto, o mês atual — é o dia em America/Sao_Paulo, lido de um
`TimeProvider` injetado; as datas das transações continuam sendo dias do calendário. Uma nova
migration adiciona o índice que faltava em `Transactions.Date` (`CategoryId` já é indexado).

## Alteração de 2026-10-09: lançamentos futuros no mês atual

Pedido e decisões na nota de alteração da [spec](./spec.md) (FR-006, FR-012, FR-013, SC-006 e SC-007), aprovada
pelo usuário em 2026-10-09. A mudança fica em dois casos de uso da camada Application, sem consulta, migration, DTO
nem rota nova:

- **`GetCumulativeExpenseComparisonUseCase.Accumulate`** (`GetCumulativeExpenseComparisonUseCase.cs:25-30`): no mês
  atual, a quantidade de dias passa de `today.Day` para o maior entre `today.Day` e o último dia com despesa do mês. O
  dicionário de despesas por dia já cobre o mês inteiro (a consulta vai de `previousMonth.FirstDay` a
  `month.AddMonths(1).FirstDay`), então a data da última despesa sai dele, sem consulta nova. O máximo considera só
  as chaves do mês que está sendo acumulado (mesmo ano e mês): o dicionário cobre os dois meses, e uma despesa do mês
  seguinte não pode estender a série do mês atual. A mesma função serve às duas séries, então a regra vale também para
  a do mês anterior quando ela é o mês atual.
- **`GetMonthlySummaryUseCase.ProjectAsync`** (`GetMonthlySummaryUseCase.cs:53-59`): despesa projetada =
  `round(expenseToDate × DayCount ÷ today.Day, 2) + (TotalExpense − expenseToDate)`. A despesa depois de hoje é a
  diferença entre a despesa total do mês, que o caso de uso já soma, e a despesa até hoje, que ele já consulta: as duas
  são exatas em centavos, então a soma continua em centavos. Decisão em [research.md](./research.md#projeção). As
  duas somas vêm de consultas separadas, como já acontecia; uma gravação entre elas poderia deixar a diferença
  momentaneamente errada numa resposta. O risco é aceito sem tratamento extra: o uso é de um único usuário, e a
  próxima consulta corrige o valor.
- **Contrato**: só as descrições de `projectedExpense` e da série acumulada em
  [contracts/reports-api.yaml](./contracts/reports-api.yaml). O formato não muda (B2: compatível).

**Testes (TDD)**: primeiro os testes novos e os atualizados em `Denarius.Application.Tests`, vendo-os falhar, e depois
o código. Dois testes existentes codificam a regra antiga e mudam de expectativa porque o requisito mudou; nenhum fica
mais fraco:

- `GetCumulativeExpenseComparisonUseCaseTests.Execute_CurrentMonth_StopsAtTodayAndKeepsThePreviousMonthWhole`: a
  despesa do dia 10 (hoje é dia 3) passa a estender a série até o dia 10. O caso "para em hoje" continua coberto por
  um teste sem despesa depois de hoje.
- `GetMonthlySummaryUseCaseTests.Execute_CurrentMonth_ProjectsTheExpenseToDateOverTheDaysElapsed`: 600,00 lançados
  depois de hoje passam a entrar na projeção (2.700,00 → 3.300,00). O caso só com ritmo continua coberto por um
  teste sem despesa depois de hoje.

Os testes novos cobrem os cenários novos da spec: série até a última despesa, receita futura que não estende a série,
despesa no último dia do mês, mês seguinte ao atual (série do mês anterior até a última despesa dele, sem que uma
despesa do mês seguinte a estenda), projeção com despesa futura, dia 1º sem despesa até hoje e último dia do mês
(projeção igual à despesa total, SC-007).

**Front**: nenhuma mudança de código; o gráfico desenha a série devolvida (`cumulative-comparison-chart.ts:49-79`).
Ficam desatualizados o cenário 1 da história 5 de `front/specs/001-reports-dashboard/spec.md` ("a linha do mês ...
para em hoje") e o comentário de `front/src/app/reports/types/report.ts:48`. Corrigi-los é uma mudança trivial no
front (texto e comentário), feita depois deste ciclo e só com a aprovação do usuário (princípio V).

### Constitution Check da alteração (constituição 3.0.0)

| Princípio | Status | Evidência |
|---|---|---|
| I. TDD | PASSA | Testes antes do código; os dois testes que mudam de expectativa estão listados acima, com o motivo. |
| II. Segurança | PASSA | Nenhuma entrada, dado, log ou dependência nova. |
| III. Performance | PASSA | Nenhuma consulta nova: o acumulado usa o dicionário que já carrega, e a projeção reaproveita as duas somas que já faz. SC-002 mantido. |
| IV. Contrato | PASSA | Mudança só de significado, documentada no contrato; formato igual. O front não precisa mudar código. |
| V. Escopo | PASSA | Só os dois casos de uso, os testes e os artefatos da 003; o front fica para depois, com aprovação. |
| B1. Camadas | PASSA | A regra fica no Application, onde já está. |
| B2. Compatibilidade da API | PASSA | Rotas, DTOs e status codes iguais. |
| B3. Migrations | Não se aplica | Nenhuma migration. |

## Contexto técnico

**Linguagem/versão**: C# 14 / .NET 10 (`net10.0`)

**Dependências principais**: ASP.NET Core MVC (`Microsoft.AspNetCore.OpenApi`); Entity Framework
Core 10 + `Npgsql.EntityFrameworkCore.PostgreSQL` 10; `System.TimeProvider` (BCL) para o "hoje";
nenhum pacote novo

**Armazenamento**: PostgreSQL via migrations code-first do EF Core. `Transactions.Date` é
`timestamp with time zone` e guarda cada dia do calendário à meia-noite UTC; `Value` é
`numeric(18,2)`, positivo para entradas e negativo para saídas. Novo índice `IX_Transactions_Date`.

**Testes**: xUnit; `Denarius.Application.Tests` com NSubstitute para `ITransactionRepository` e
`TimeProvider`; `Denarius.WebAPI.Tests` com `TestServer` e fakes dos casos de uso escritos à mão.
Não há projeto de testes de Infrastructure, então a tradução das novas consultas para SQL é validada
no PostgreSQL pelo quickstart.

**Plataforma-alvo**: ASP.NET Core Web API em `api/reports`, consumida pelo front Angular do Denarius

**Tipo de projeto**: web — a metade backend de uma aplicação web de dois projetos; este plano cobre
o backend

**Metas de desempenho**: Cada relatório em menos de um segundo com um histórico de 10.000 transações
(SC-002): cada requisição lê só os meses que cobre, com as somas feitas pelo PostgreSQL sobre o
índice de data.

**Restrições**: Regidas por `.specify/memory/constitution.md` (veja o Constitution Check). Do pedido:
dinheiro como `decimal`, nunca `float`/`double`; datas em America/Sao_Paulo; `month` como `YYYY-MM`,
mês atual quando omitido, inválido → `400`; um endpoint e um caso de uso por relatório; controllers
finos.

**Escala/escopo**: Finanças pessoais single-tenant — de centenas a poucos milhares de transações por
ano, algumas dezenas de categorias. Cinco endpoints, cinco casos de uso, quatro consultas de
repositório, uma migration.

## Constitution Check

*PORTÃO: precisa passar antes da pesquisa da Fase 0. Conferir de novo após o design da Fase 1.*

| Princípio | Status | Evidência |
|---|---|---|
| I. Compatibilidade da API pública | PASSA | Só acréscimos: um novo `ReportsController` com cinco novas rotas `GET` e novos formatos de resposta. O `TransactionType` ganha atributos de JSON para que o novo campo `type` seja `in`/`out`; nenhum corpo de requisição ou de resposta existente usa esse enum, e o parâmetro de query `type` existente continua com binding sem diferenciar maiúsculas de minúsculas pelo seu type converter, então nenhum contrato existente muda. |
| II. Respeito às fronteiras de serviço | PASSA | `Denarius.Domain` ganha quatro assinaturas de consulta no `ITransactionRepository` e nenhuma dependência nova. `Denarius.Application` guarda todas as regras dos relatórios e depende só de `Domain` e do `TimeProvider` da BCL. `Denarius.Infrastructure` implementa as consultas com EF Core. O `ReportsController` faz o binding, valida (`YearMonth`, `[Range]`) e delega — nenhuma regra em `WebAPI`, nenhuma camada pulada. |
| III. Disciplina de rollback de migrations | PASSA | Uma migration, `AddTransactionDateIndex`: o `Up()` cria `IX_Transactions_Date`, o `Down()` o remove — nenhum dado é tocado. O quickstart a aplica, faz o rollback para `AddTransaction` e a reaplica num banco descartável antes da revisão. |
| IV. Verificação da suíte de testes | PASSA | Cada caso de uso ganha cobertura em `Denarius.Application.Tests` (mês vazio, só receita, só despesa, virada de mês/ano, mês atual vs. passado/futuro, divisão por zero), o `YearMonth` ganha testes próprios e o `ReportsController`, cobertura em `Denarius.WebAPI.Tests` (binding, padrões, `400`s, formato do JSON). O `dotnet test` precisa passar nas três suítes. |

Nenhuma violação — o Acompanhamento de complexidade não é necessário.

*Conferido de novo após o design da Fase 1: sem mudança. O design não acrescenta projeto, pacote nem
dependência entre camadas; data-model.md e contracts/ só descrevem os acréscimos acima.*

## Estrutura do projeto

### Documentação (desta feature)

```text
specs/003-financial-reports/
├── plan.md               # Este arquivo (saída do comando /speckit-plan)
├── research.md           # Saída da Fase 0 (comando /speckit-plan)
├── data-model.md         # Saída da Fase 1 (comando /speckit-plan)
├── quickstart.md         # Saída da Fase 1 (comando /speckit-plan)
├── contracts/            # Saída da Fase 1 (comando /speckit-plan)
│   └── reports-api.yaml
└── tasks.md              # Saída da Fase 2 (comando /speckit-tasks - NÃO é criado pelo /speckit-plan)
```

### Código-fonte (raiz do repositório)

```text
src/
├── Denarius.Domain/
│   └── Repositories/ITransactionRepository.cs        # + 4 consultas de relatório
├── Denarius.Application/
│   ├── IO/Reports/                                   # YearMonth, os 5 inputs e os seus outputs
│   ├── IO/Transactions/TransactionType.cs            # in/out como strings no JSON
│   ├── UseCases/Reports/                             # TimeProviderExtensions (hoje em São Paulo)
│   │   ├── GetMonthlySummary/
│   │   ├── GetExpensesByCategory/
│   │   ├── GetIncomeVsExpense/
│   │   ├── GetCumulativeExpenseComparison/
│   │   └── ListMonthlyTransactions/
│   └── DependencyInjection.cs                        # + 5 casos de uso, TimeProvider.System
├── Denarius.Infrastructure/
│   ├── Persistence/Configurations/TransactionConfiguration.cs   # + índice em Date
│   ├── Repositories/TransactionRepository.cs                   # + 4 consultas de relatório
│   └── Migrations/*_AddTransactionDateIndex.cs
└── Denarius.WebAPI/
    └── Controllers/ReportsController.cs

tests/
├── Denarius.Application.Tests/
│   ├── IO/Reports/YearMonthTests.cs
│   └── UseCases/Reports/                             # uma pasta por caso de uso
└── Denarius.WebAPI.Tests/Reports/ReportsControllerTests.cs
```

**Decisão de estrutura**: A mesma de [002-transaction-management](../002-transaction-management/plan.md):
a metade backend da aplicação web, no layout de Clean Architecture deste repositório, espelhado 1:1
em `tests/`. Relatórios não são uma entidade, então não acrescentam entidade, configuração nem
repositório próprios: eles leem `Transaction` (e `Category`) pelo `ITransactionRepository`, e o IO e
os casos de uso deles seguem as pastas `IO/{Feature}` e `UseCases/{Feature}/{Operation}` que as
outras features usam.

## Acompanhamento de complexidade

Não se aplica — o Constitution Check não apontou violações.
