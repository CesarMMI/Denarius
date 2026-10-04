# Contracts: Reports Dashboard

## Consumed API

Defined by the backend in
[`back/specs/003-financial-reports/contracts/reports-api.yaml`](../../../../back/specs/003-financial-reports/contracts/reports-api.yaml).
Every request goes through `ReportsService`, which returns `{ url, params }` for an `httpResource`.
`month` is sent as `YYYY-MM`; without a month the API reports the current month in São Paulo.

| Service method                    | Request                                                         | Response type                 |
| --------------------------------- | --------------------------------------------------------------- | ----------------------------- |
| `summary(month)`                  | `GET {apiUrl}/reports/summary?month=YYYY-MM`                    | `MonthlySummary`              |
| `expensesByCategory(month)`       | `GET {apiUrl}/reports/expensesByCategory?month=YYYY-MM`         | `ExpensesByCategory`          |
| `incomeVsExpense(month, months?)` | `GET {apiUrl}/reports/incomeVsExpense?month=YYYY-MM[&months=N]` | `IncomeVsExpense[]`           |
| `cumulativeExpenses(month)`       | `GET {apiUrl}/reports/cumulativeExpenses?month=YYYY-MM`         | `CumulativeExpenseComparison` |
| `transactions(month)`             | `GET {apiUrl}/reports/transactions?month=YYYY-MM`               | `MonthlyTransaction[]`        |

An invalid month is a `400` from the API; the page never sends one (the month field only produces
valid months), and any failed request shows the block's error state.

## Exposed UI

- Route `/reports` (lazy, `loadChildren` from `app.routes.ts`), linked first in the sidenav as
  "Relatórios" (`insights` icon).
- The transactions page (`/transactions`) accepts `?month=YYYY-MM`, as it accepts `?categoryId=`: its
  filters open on that month. A value that isn't a valid `YYYY-MM` is ignored. The list's "Ver todas"
  links there.
- Block components (inputs → outputs), all presentational:

| Component                         | Inputs                                                                                                              | Outputs |
| --------------------------------- | ------------------------------------------------------------------------------------------------------------------- | ------- |
| `app-report-card`                 | `heading`, `subtitle?`, `report: Resource<unknown>`, `empty`, `emptyText`, `errorText`; content as an `ng-template` | `retry` |
| `app-summary-cards`               | `summary: Resource<MonthlySummary \| undefined>`                                                                    | `retry` |
| `app-expenses-by-category-chart`  | `expenses: Resource<ExpensesByCategory \| undefined>`                                                               | `retry` |
| `app-income-vs-expense-chart`     | `series: Resource<IncomeVsExpense[] \| undefined>`                                                                  | `retry` |
| `app-cumulative-comparison-chart` | `comparison: Resource<CumulativeExpenseComparison \| undefined>`, `month: Date \| null`                             | `retry` |
| `app-transactions-list`           | `transactions: Resource<MonthlyTransaction[] \| undefined>`, `month: Date \| null`                                  | `retry` |
