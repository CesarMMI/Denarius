# Modelo de dados: Relatórios financeiros

Os relatórios não armazenam nada: eles leem `Transaction` ([002-transaction-management](../002-transaction-management/data-model.md))
e `Category` ([001-category-management](../001-category-management/data-model.md)). Este documento
descreve o tipo de mês, os Inputs e Outputs dos cinco casos de uso, as consultas de repositório de
que eles leem e a única mudança de esquema (um índice).

## YearMonth (`Application/IO/Reports/YearMonth.cs`)

Um mês do calendário; um `readonly record struct` que guarda o primeiro dia do mês.

| Membro | Significado |
|---|---|
| `Year`, `Month` | O ano e o número do mês (1–12). |
| `FirstDay` | `DateOnly` — o dia 1 do mês. |
| `DayCount` | Quantidade de dias do mês (28–31, incluindo anos bissextos). |
| `AddMonths(int)` | O mês que fica essa quantidade de meses depois (negativo: antes); atravessa anos. |
| `FromDate(DateOnly)` | O mês a que um dia pertence. |
| `TryParse(string?, IFormatProvider?, out YearMonth)` | Aceita exatamente `yyyy-MM` (`2026-09`); qualquer outra coisa — `2026-9`, `2026-13`, `2026-00`, `09-2026`, `2026-09-01`, em branco — falha. O MVC faz o binding dos parâmetros de query por meio dele. |
| `CompareTo(YearMonth)` | Ordem cronológica. |
| `ToString()` | `yyyy-MM`. |

## Inputs (`Application/IO/Reports/`)

O `Month` de todo input é opcional: `null` significa o mês atual em America/Sao_Paulo, resolvido
pelo caso de uso.

| Record | Campos |
|---|---|
| `GetMonthlySummaryInput` | `YearMonth? Month` |
| `GetExpensesByCategoryInput` | `YearMonth? Month` |
| `GetIncomeVsExpenseInput` | `YearMonth? Month` — o último mês da série; `int Months = 12` — validado em 1–24 pelo controller (`[Range(1, 24)]`) |
| `GetCumulativeExpenseComparisonInput` | `YearMonth? Month` |
| `ListMonthlyTransactionsInput` | `YearMonth? Month` |

## Outputs (`Application/IO/Reports/`)

Dinheiro é `decimal`. A despesa é sempre um valor positivo. Os percentuais estão na escala 0–100,
arredondados para duas casas decimais (`AwayFromZero`). Os meses são strings `yyyy-MM`.

### `MonthlySummaryOutput` (GetMonthlySummary)

| Campo | Tipo | Regra |
|---|---|---|
| `Month` | `string` | O mês do relatório. |
| `TotalIncome` | `decimal` | Soma dos valores positivos do mês. |
| `TotalExpense` | `decimal` | Soma dos tamanhos dos valores negativos do mês. |
| `Balance` | `decimal` | `TotalIncome − TotalExpense`. |
| `SavingsRate` | `decimal?` | `Balance / TotalIncome × 100`; `null` quando `TotalIncome` é 0; negativa quando a despesa passa da receita. |
| `ProjectedExpense` | `decimal` | Mês atual: `ExpenseToDate / today.Day × DayCount`, em que `ExpenseToDate` cobre do dia 1 até hoje; mês passado: `TotalExpense`; mês futuro: 0. Arredondado para centavos. |
| `ProjectedBalance` | `decimal` | Mês atual: `TotalIncome − ProjectedExpense`; mês passado: `Balance`; mês futuro: 0. |
| `PreviousMonth` | `PreviousMonthSummaryOutput` | Veja abaixo. |

`PreviousMonthSummaryOutput`: `TotalIncome`, `TotalExpense`, `Balance` (`decimal`, mesmas regras,
para o mês anterior — dezembro do ano anterior no caso de janeiro) e `TotalIncomeChange`,
`TotalExpenseChange`, `BalanceChange` (`decimal?`): `(current − previous) / |previous| × 100`, `null`
quando o valor anterior é 0.

### `ExpensesByCategoryOutput` (GetExpensesByCategory)

| Campo | Tipo | Regra |
|---|---|---|
| `Total` | `decimal` | A despesa total do mês. |
| `Items` | `IEnumerable<CategoryExpenseOutput>` | Do maior valor para o menor, empates pelo nome da categoria; com mais de 8 categorias, as 7 maiores e depois "Outras". Vazio quando o mês não tem despesa. |

`CategoryExpenseOutput`: `CategoryId` (`Guid?`, `null` para "Outras"), `CategoryName` (`string`),
`Color` (`string?`, o `#RRGGBB` da categoria, `null` para "Outras"), `Amount` (`decimal`),
`Percentage` (`decimal`, `Amount / Total × 100`).

### `IncomeVsExpenseOutput` (GetIncomeVsExpense, devolvido como lista)

Um por mês da série, do mais antigo para o mais recente, `Months` entradas terminando no mês do
relatório: `Month` (`string`), `Income`, `Expense`, `Balance` (`decimal`, `Income − Expense`). Um
mês sem transações aparece com zeros.

### `CumulativeExpenseComparisonOutput` (GetCumulativeExpenseComparison)

| Campo | Tipo | Regra |
|---|---|---|
| `CurrentMonth` | `IEnumerable<AccumulatedExpenseOutput>` | O mês do relatório, do dia 1 ao último dia — até hoje quando é o mês atual, sem nenhum dia quando é um mês futuro. |
| `PreviousMonth` | `IEnumerable<AccumulatedExpenseOutput>` | O mês anterior, com a mesma regra. |
| `DaysInCurrentMonth` | `int` | Dias do mês do relatório. |
| `DaysInPreviousMonth` | `int` | Dias do mês anterior. |

`AccumulatedExpenseOutput`: `Day` (`int`, 1–31), `Accumulated` (`decimal`, a despesa do mês do dia
1 até aquele dia; inalterada nos dias sem despesa).

### `MonthlyTransactionOutput` (ListMonthlyTransactions, devolvido como lista)

Todas as transações com data no mês, da data mais recente para a mais antiga e, depois, das criadas
mais recentemente para as mais antigas: `Id` (`Guid`), `Date` (`DateTime`, o dia do calendário à
meia-noite UTC, como em `TransactionOutput`), `Description` (`string?`), `CategoryName` (`string`),
`Type` (`TransactionType`: `In` quando o valor é positivo, `Out` quando é negativo — serializado como
`in`/`out`), `Amount` (`decimal`, o tamanho do valor).

## Consultas do repositório (`Domain/Repositories/ITransactionRepository.cs`)

Os intervalos são dias do calendário, `from` inclusivo e `to` exclusivo; a Infrastructure os compara
como meias-noites UTC, a forma em que as datas são armazenadas. Cada consulta roda um único comando
SQL, só sobre o intervalo.

| Consulta | Retorna | Formato do SQL |
|---|---|---|
| `SumByMonthAsync(DateOnly from, DateOnly to)` | `(int Year, int Month, decimal Income, decimal Expense)` por mês com transações | `GROUP BY` o ano e o mês UTC de `Date`; `SUM(CASE WHEN "Value" > 0 ...)` e `-SUM(CASE WHEN "Value" < 0 ...)` |
| `SumExpensesByCategoryAsync(DateOnly from, DateOnly to)` | `(Category Category, decimal Expense)` por categoria com despesas | `-SUM("Value") ... WHERE "Value" < 0 GROUP BY "CategoryId"`, com join em `Categories` |
| `SumExpensesByDayAsync(DateOnly from, DateOnly to)` | `(DateOnly Date, decimal Expense)` por dia com despesas | `GROUP BY` o ano, o mês e o dia UTC de `Date`, `WHERE "Value" < 0` |
| `GetWithCategoryNameAsync(DateOnly from, DateOnly to)` | `(Transaction Transaction, string CategoryName)` para cada transação | `Transactions JOIN Categories`, filtrado pelo intervalo |

## Mudança de esquema

| Índice | Tabela | Coluna | Migration |
|---|---|---|---|
| `IX_Transactions_Date` *(novo)* | `Transactions` | `Date` | `AddTransactionDateIndex` — o `Up()` o cria, o `Down()` o remove. |
| `IX_Transactions_CategoryId` *(existente)* | `Transactions` | `CategoryId` | `AddTransaction` — inalterado. |

Nenhuma mudança de tabela, coluna ou dados.
