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
