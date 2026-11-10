# Plano: Relatórios financeiros

**Spec**: [spec.md](./spec.md) · **Entregue em**: `9a4452a`, com a alteração dos lançamentos futuros em `916b9cc` · **Contrato**: [contracts/reports-api.yaml](./contracts/reports-api.yaml) · **Pesquisa**: [research.md](./research.md)

## Conferência com as regras

- Camadas (B1): as regras dos relatórios ficam no Application, que depende só do `Domain` e do `TimeProvider` da BCL; o `Infrastructure` implementa as consultas; o `ReportsController` só faz binding e validação e delega.
- Compatibilidade da API (B2): aditiva. Um controller novo com cinco rotas `GET`; `TransactionType` ganha atributos de JSON (`in`/`out`) sem afetar corpos existentes, e o `type` da query da listagem continua com binding sem diferenciar maiúsculas.
- Migrations (B3): `AddTransactionDateIndex` cria `IX_Transactions_Date` no `Up()` e o remove no `Down()`, sem tocar dados; rollback verificado em banco descartável.
- Segurança (II): `month` e `months` validados antes de qualquer caso de uso (`YearMonth.TryParse`, `[Range]`), com 400; consultas em LINQ; sem dado financeiro em logs; sem dependência nova.
- Performance (III): SC-002 por `GROUP BY`/`SUM` no PostgreSQL, só sobre os meses pedidos, com o índice em `Date`.

## Mudanças

- **Domínio**: quatro consultas novas no `ITransactionRepository`. Sem entidade nova: os relatórios só leem `Transaction` e `Category` (o tipo vem do sinal do valor).
- **Aplicação**: `IO/Reports` (`YearMonth` e os inputs e outputs de cada relatório) e um caso de uso por relatório em `UseCases/Reports` (`GetMonthlySummary`, `GetExpensesByCategory`, `GetIncomeVsExpense`, `GetCumulativeExpenseComparison`, `ListMonthlyTransactions`), com as regras da spec; "hoje" vem de um `TimeProvider` injetado, convertido para America/Sao_Paulo.
- **Infraestrutura**: as quatro consultas em `TransactionRepository` e a migration do índice em `Transactions.Date` (`timestamp with time zone`, dias à meia-noite UTC).
- **API**: `ReportsController` em `api/reports` (`summary`, `expensesByCategory`, `incomeVsExpense`, `cumulativeExpenses`, `transactions`), descrito em `contracts/reports-api.yaml`. Contratos existentes inalterados (FR-017).
- **Lançamentos futuros (2026-10-09)**: só dois casos de uso mudaram, sem consulta, migration nem DTO novos. `GetCumulativeExpenseComparisonUseCase.Accumulate` estende a série do mês atual até a última despesa do mês (considerando só as chaves do mês acumulado), e `GetMonthlySummaryUseCase.ProjectAsync` soma `TotalExpense − expenseToDate` à projeção. Só as descrições do contrato mudaram.
- **Documentos vivos**: as convenções e regras gerais dos relatórios (mês `yyyy-MM`, entrada "Outras", série de meses) estão em [`docs/api.md`](../../api.md); o detalhe de cada relatório está nesta spec.

## Decisões

Detalhadas em [research.md](./research.md).

- Um endpoint e um caso de uso por relatório, controllers finos.
- Mês como `YearMonth` com `TryParse`, para o MVC devolver 400 antes do caso de uso.
- Somas e agrupamentos no banco, e não em memória, para cumprir o SC-002.
- Projeção: `round(expenseToDate × dias ÷ today.Day, 2) + despesa depois de hoje`. As duas somas vêm de consultas separadas, e uma gravação entre elas poderia errar a diferença numa resposta; o risco é aceito (único usuário, a próxima consulta corrige) ([research.md](./research.md#projeção)).
- Contrato da alteração de 2026-10-09 editado no mesmo arquivo, por ser só de significado (B2: compatível).

## Testes previstos

Escritos antes do código, vistos falhar.

- **Application.Tests**: cada caso de uso (mês vazio, só receita, só despesa, virada de mês e de ano, mês atual contra passado e futuro, divisão por zero, "Outras") e `YearMonthTests`.
- **Lançamentos futuros**: série até a última despesa, receita futura que não estende, despesa no último dia, mês seguinte ao atual, projeção com despesa futura, dia 1º sem despesa até hoje e último dia (SC-006, SC-007). Dois testes mudaram de expectativa porque o requisito mudou (`Execute_CurrentMonth_StopsAtTodayAndKeepsThePreviousMonthWhole` e `Execute_CurrentMonth_ProjectsTheExpenseToDateOverTheDaysElapsed`); o caso sem despesa futura continua coberto.
- **WebAPI.Tests**: `ReportsControllerTests` com binding, padrões, 400s e formato do JSON.
- **Infrastructure.Tests**: a tradução das consultas para SQL, no PostgreSQL descartável.

## Verificação

1. `dotnet test` com as quatro suítes verdes (exige o Docker).
2. Migration: aplicar, voltar para `AddTransaction` (só `IX_Transactions_CategoryId` resta) e reaplicar, num banco descartável.
3. Com a API no ar e um histórico conhecido, chamar os cinco relatórios e conferir os totais, a projeção e as séries à mão, incluindo os 400 (`month=2026-13`, `2026-9`, `09-2026`; `months=0`, `25`, `abc`) e o mês sem dados (zeros e `savingsRate` nulo).
4. SC-002: com 10.000 transações, cada relatório responde `200` em bem menos de um segundo.
