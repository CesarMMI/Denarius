# Contratos: Painel de relatórios

## API consumida

Definida pelo backend em
[`back/specs/003-financial-reports/contracts/reports-api.yaml`](../../../../back/specs/003-financial-reports/contracts/reports-api.yaml).
Toda requisição passa pelo `ReportsService`, que devolve `{ url, params }` para um `httpResource`. O `month` é enviado
como `YYYY-MM`; sem mês, a API informa o mês atual em São Paulo.

| Método do service                 | Requisição                                                      | Tipo da resposta              |
| --------------------------------- | --------------------------------------------------------------- | ----------------------------- |
| `summary(month)`                  | `GET {apiUrl}/reports/summary?month=YYYY-MM`                    | `MonthlySummary`              |
| `expensesByCategory(month)`       | `GET {apiUrl}/reports/expensesByCategory?month=YYYY-MM`         | `ExpensesByCategory`          |
| `incomeVsExpense(month, months?)` | `GET {apiUrl}/reports/incomeVsExpense?month=YYYY-MM[&months=N]` | `IncomeVsExpense[]`           |
| `cumulativeExpenses(month)`       | `GET {apiUrl}/reports/cumulativeExpenses?month=YYYY-MM`         | `CumulativeExpenseComparison` |
| `transactions(month)`             | `GET {apiUrl}/reports/transactions?month=YYYY-MM`               | `MonthlyTransaction[]`        |

Um mês inválido é um `400` da API; a página nunca envia um (o campo de mês só produz meses válidos), e qualquer
requisição que falhe mostra o estado de erro do bloco.

## UI exposta

- Rota `/reports` (lazy, `loadChildren` a partir do `app.routes.ts`), com link em primeiro lugar na sidenav como
  "Relatórios" (ícone `insights`).
- A página de transações (`/transactions`) aceita `?month=YYYY-MM`, como aceita `?categoryId=`: os seus filtros abrem
  naquele mês. Um valor que não seja um `YYYY-MM` válido é ignorado. O "Ver todas" da lista leva para lá.
- Componentes dos blocos (inputs → outputs), todos de apresentação:

| Componente                        | Inputs                                                                                                              | Outputs |
| --------------------------------- | ------------------------------------------------------------------------------------------------------------------- | ------- |
| `app-report-card`                 | `heading`, `subtitle?`, `report: Resource<unknown>`, `empty`, `emptyText`, `errorText`; conteúdo como `ng-template` | `retry` |
| `app-summary-cards`               | `summary: Resource<MonthlySummary \| undefined>`                                                                    | `retry` |
| `app-expenses-by-category-chart`  | `expenses: Resource<ExpensesByCategory \| undefined>`                                                               | `retry` |
| `app-income-vs-expense-chart`     | `series: Resource<IncomeVsExpense[] \| undefined>`                                                                  | `retry` |
| `app-cumulative-comparison-chart` | `comparison: Resource<CumulativeExpenseComparison \| undefined>`, `month: Date \| null`                             | `retry` |
| `app-transactions-list`           | `transactions: Resource<MonthlyTransaction[] \| undefined>`, `month: Date \| null`                                  | `retry` |
