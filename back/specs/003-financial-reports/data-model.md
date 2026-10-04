# Data Model: Financial Reports

The reports store nothing: they read `Transaction` ([002-transaction-management](../002-transaction-management/data-model.md))
and `Category` ([001-category-management](../001-category-management/data-model.md)). This document
describes the month type, the Inputs and Outputs of the five use cases, the repository queries they
read from, and the one schema change (an index).

## YearMonth (`Application/IO/Reports/YearMonth.cs`)

A calendar month; a `readonly record struct` holding the month's first day.

| Member | Meaning |
|---|---|
| `Year`, `Month` | The month's year and number (1–12). |
| `FirstDay` | `DateOnly` — day 1 of the month. |
| `DayCount` | Days in the month (28–31, leap years included). |
| `AddMonths(int)` | The month that many months later (negative: earlier); crosses years. |
| `FromDate(DateOnly)` | The month a day belongs to. |
| `TryParse(string?, IFormatProvider?, out YearMonth)` | Accepts exactly `yyyy-MM` (`2026-09`); anything else — `2026-9`, `2026-13`, `2026-00`, `09-2026`, `2026-09-01`, blank — fails. MVC binds query parameters through it. |
| `CompareTo(YearMonth)` | Chronological order. |
| `ToString()` | `yyyy-MM`. |

## Inputs (`Application/IO/Reports/`)

Every input's `Month` is optional: `null` means the current month in America/Sao_Paulo, resolved by
the use case.

| Record | Fields |
|---|---|
| `GetMonthlySummaryInput` | `YearMonth? Month` |
| `GetExpensesByCategoryInput` | `YearMonth? Month` |
| `GetIncomeVsExpenseInput` | `YearMonth? Month` — the series' last month; `int Months = 12` — validated 1–24 by the controller (`[Range(1, 24)]`) |
| `GetCumulativeExpenseComparisonInput` | `YearMonth? Month` |
| `ListMonthlyTransactionsInput` | `YearMonth? Month` |

## Outputs (`Application/IO/Reports/`)

Money is `decimal`. Expense is always a positive amount. Percentages are on a 0–100 scale, rounded to
two decimals (`AwayFromZero`). Months are `yyyy-MM` strings.

### `MonthlySummaryOutput` (GetMonthlySummary)

| Field | Type | Rule |
|---|---|---|
| `Month` | `string` | The report month. |
| `TotalIncome` | `decimal` | Sum of the month's positive values. |
| `TotalExpense` | `decimal` | Sum of the sizes of the month's negative values. |
| `Balance` | `decimal` | `TotalIncome − TotalExpense`. |
| `SavingsRate` | `decimal?` | `Balance / TotalIncome × 100`; `null` when `TotalIncome` is 0; negative when expense exceeds income. |
| `ProjectedExpense` | `decimal` | Current month: `ExpenseToDate / today.Day × DayCount`, where `ExpenseToDate` covers day 1 through today; past month: `TotalExpense`; future month: 0. Rounded to cents. |
| `ProjectedBalance` | `decimal` | Current month: `TotalIncome − ProjectedExpense`; past month: `Balance`; future month: 0. |
| `PreviousMonth` | `PreviousMonthSummaryOutput` | See below. |

`PreviousMonthSummaryOutput`: `TotalIncome`, `TotalExpense`, `Balance` (`decimal`, same rules, for
the month before — December of the previous year for a January) and `TotalIncomeChange`,
`TotalExpenseChange`, `BalanceChange` (`decimal?`): `(current − previous) / |previous| × 100`, `null`
when the previous value is 0.

### `ExpensesByCategoryOutput` (GetExpensesByCategory)

| Field | Type | Rule |
|---|---|---|
| `Total` | `decimal` | The month's total expense. |
| `Items` | `IEnumerable<CategoryExpenseOutput>` | Largest amount first, ties by category name; with more than 8 categories, the 7 largest and then "Outras". Empty when the month has no expense. |

`CategoryExpenseOutput`: `CategoryId` (`Guid?`, `null` for "Outras"), `CategoryName` (`string`),
`Color` (`string?`, the category's `#RRGGBB`, `null` for "Outras"), `Amount` (`decimal`), `Percentage`
(`decimal`, `Amount / Total × 100`).

### `IncomeVsExpenseOutput` (GetIncomeVsExpense, returned as a list)

One per month of the series, oldest first, `Months` entries ending in the report month:
`Month` (`string`), `Income`, `Expense`, `Balance` (`decimal`, `Income − Expense`). A month without
transactions is present with zeros.

### `CumulativeExpenseComparisonOutput` (GetCumulativeExpenseComparison)

| Field | Type | Rule |
|---|---|---|
| `CurrentMonth` | `IEnumerable<AccumulatedExpenseOutput>` | The report month, day 1 to its last day — to today when it is the current month, no days when it is a future month. |
| `PreviousMonth` | `IEnumerable<AccumulatedExpenseOutput>` | The month before, under the same rule. |
| `DaysInCurrentMonth` | `int` | Days in the report month. |
| `DaysInPreviousMonth` | `int` | Days in the month before. |

`AccumulatedExpenseOutput`: `Day` (`int`, 1–31), `Accumulated` (`decimal`, the month's expense from
day 1 through that day; unchanged on days without expense).

### `MonthlyTransactionOutput` (ListMonthlyTransactions, returned as a list)

Every transaction dated in the month, newest date first, then most recently created first:
`Id` (`Guid`), `Date` (`DateTime`, the calendar day at UTC midnight, as in `TransactionOutput`),
`Description` (`string?`), `CategoryName` (`string`), `Type` (`TransactionType`: `In` when the value
is positive, `Out` when negative — serialized `in`/`out`), `Amount` (`decimal`, the value's size).

## Repository queries (`Domain/Repositories/ITransactionRepository.cs`)

Ranges are calendar days, `from` inclusive and `to` exclusive; Infrastructure compares them as UTC
midnights, the form in which dates are stored. Each query runs one SQL statement over the range only.

| Query | Returns | SQL shape |
|---|---|---|
| `SumByMonthAsync(DateOnly from, DateOnly to)` | `(int Year, int Month, decimal Income, decimal Expense)` per month with transactions | `GROUP BY` the UTC year and month of `Date`; `SUM(CASE WHEN "Value" > 0 ...)` and `-SUM(CASE WHEN "Value" < 0 ...)` |
| `SumExpensesByCategoryAsync(DateOnly from, DateOnly to)` | `(Category Category, decimal Expense)` per category with expenses | `-SUM("Value") ... WHERE "Value" < 0 GROUP BY "CategoryId"`, joined to `Categories` |
| `SumExpensesByDayAsync(DateOnly from, DateOnly to)` | `(DateOnly Date, decimal Expense)` per day with expenses | `GROUP BY` the UTC year, month and day of `Date`, `WHERE "Value" < 0` |
| `GetWithCategoryNameAsync(DateOnly from, DateOnly to)` | `(Transaction Transaction, string CategoryName)` for each transaction | `Transactions JOIN Categories`, filtered by the range |

## Schema change

| Index | Table | Column | Migration |
|---|---|---|---|
| `IX_Transactions_Date` *(new)* | `Transactions` | `Date` | `AddTransactionDateIndex` — `Up()` creates it, `Down()` drops it. |
| `IX_Transactions_CategoryId` *(existing)* | `Transactions` | `CategoryId` | `AddTransaction` — unchanged. |

No table, column or data changes.
