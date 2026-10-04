# Data Model: Reports Dashboard

The front stores nothing. Its model is the five report shapes it receives, typed in
`src/app/reports/types/report.ts` to mirror `back/specs/003-financial-reports/contracts/reports-api.yaml`
(money is `number` here — the API sends decimals as JSON numbers; months are `YYYY-MM` strings; dates are
ISO strings at UTC midnight).

## Types

```ts
interface MonthlySummary {
	month: string; // 'YYYY-MM'
	totalIncome: number;
	totalExpense: number; // positive
	balance: number;
	savingsRate: number | null; // 0–100 scale; null without income
	projectedExpense: number;
	projectedBalance: number;
	previousMonth: PreviousMonthSummary;
}

interface PreviousMonthSummary {
	totalIncome: number;
	totalExpense: number;
	balance: number;
	totalIncomeChange: number | null; // 0–100 scale; null when the previous value is 0
	totalExpenseChange: number | null;
	balanceChange: number | null;
}

interface ExpensesByCategory {
	total: number;
	items: CategoryExpense[]; // largest first, "Outras" last
}

interface CategoryExpense {
	categoryId: string | null; // null for "Outras"
	categoryName: string;
	color: string | null; // '#RRGGBB'; null for "Outras"
	amount: number;
	percentage: number; // 0–100 scale
}

interface IncomeVsExpense {
	// the endpoint returns IncomeVsExpense[]
	month: string;
	income: number;
	expense: number;
	balance: number;
}

interface CumulativeExpenseComparison {
	currentMonth: AccumulatedExpense[];
	previousMonth: AccumulatedExpense[];
	daysInCurrentMonth: number;
	daysInPreviousMonth: number;
}

interface AccumulatedExpense {
	day: number;
	accumulated: number;
}

interface MonthlyTransaction {
	// the endpoint returns MonthlyTransaction[]
	id: string;
	date: string;
	description: string | null;
	categoryName: string;
	type: 'in' | 'out';
	amount: number; // positive; type says the sign
}
```

## Page state (`ReportsPage`)

| State                                                                                    | Type                           | Rule                                                                                                                                                                      |
| ---------------------------------------------------------------------------------------- | ------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `month`                                                                                  | `WritableSignal<Date \| null>` | Day 1 of the selected month, local time; starts on the current month in São Paulo. Two-way bound to the month field; "Mês anterior" / "Próximo mês" move it by one month. |
| `summary`, `expensesByCategory`, `incomeVsExpense`, `cumulativeExpenses`, `transactions` | `HttpResourceRef<…>`           | One per block, each from `ReportsService` with `month()`; a new month reloads all five, `retry` from a block reloads its own.                                             |

## Derived view state (per block)

| Block                 | Empty when                                  | Shows                                                                                                                                                                     |
| --------------------- | ------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Summary cards         | income and expense are both 0               | 5 cards, expenses as negative values without color; changes as ↑/↓ + percent + "vs. {previous month}", good/bad color by metric                                           |
| Expenses by category  | `items` is empty                            | doughnut, at most five slices (past five items, the four largest and "Outras" with the rest, its share recomputed from `total`), category color, "Outras" neutral         |
| Income vs. expense    | every month has income and expense 0        | grouped columns per month, income green, expense red                                                                                                                      |
| Cumulative comparison | every accumulated value of both series is 0 | two lines over days 1…max(days), current red, previous neutral                                                                                                            |
| Transactions list     | the list is empty                           | title with the month's count; table of the first ten (newest): dd/MM, description + category, signed amount (red when `out`); "Ver todas" → `/transactions?month=YYYY-MM` |
