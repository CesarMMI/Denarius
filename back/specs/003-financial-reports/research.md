# Phase 0 Research: Financial Reports

Each entry resolves a decision the request left open, or a conflict between the request's wording and
a convention this codebase already follows. Where the request said "definir e documentar", the
decision is recorded here and in [spec.md](./spec.md) → Assumptions.

## How money in and money out are modeled

- **Decision**: Follow the existing model exactly: the sign of `Transaction.Value` (`decimal`,
  `numeric(18,2)`) is the type — positive is money in (income), negative is money out (expense), zero
  is impossible (the entity rejects it). Categories carry no type. The reports add expenses up as
  `-SUM(value)` over negative values, so every expense the API returns is a positive amount and
  `balance = income - expense`. Every money value is `decimal` end to end.
- **Rationale**: The request asks to discover and follow the model; there is no stored type to read,
  and `TransactionType` (`All`/`In`/`Out`) only exists to filter the transaction list by sign.
- **Alternatives considered**: Returning signed expenses (as `TransactionOutput.value` does) — rejected
  for the reports, where "total expense" and chart bars read naturally as positive amounts and the
  request defines the balance as income − expense.

## Dates and the America/Sao_Paulo time zone

- **Decision**: Transaction dates are calendar days. The front sends each day as its UTC midnight
  (`front/src/app/shared/date-utils/date-utils.ts`) and the API filters months in UTC, so the reports
  group by the UTC date parts of `Transactions.Date` and never convert them. The time zone decides
  only *which day is today*: `TimeProvider.GetUtcNow()` converted to `America/Sao_Paulo`
  (`TimeZoneInfo.FindSystemTimeZoneById`, IANA id — resolved through ICU on Windows and tzdata on
  Linux). "Today" sets the current month when `month` is omitted, the days elapsed and the "expense
  to date" of the projection, and where the current month's cumulative series stops.
- **Rationale**: Converting the stored dates would move every transaction to the previous day
  (00:00 UTC is 21:00 of the day before in São Paulo), putting a September 1 expense into August. The
  only instant the reports deal with is "now", and that is where São Paulo matters: at 22:00 of
  September 30 in São Paulo the server's UTC clock already says October 1.
- **Alternatives considered**: `DateTime.Now`/`TimeZoneInfo.Local` — depends on the server's zone and
  can't be tested; a custom `IClock` interface — the BCL `TimeProvider` already is that abstraction,
  needs no package, and NSubstitute can stub its virtual `GetUtcNow()`.

## The `month` parameter

- **Decision**: A `YearMonth` value type in `Application/IO/Reports` with a static
  `TryParse(string?, IFormatProvider?, out YearMonth)` that accepts exactly `yyyy-MM`
  (`DateOnly.TryParseExact`, invariant culture). Controllers take `[FromQuery] YearMonth? month`; MVC
  binds it through `TryParse`, and `[ApiController]` turns a failed parse into a `400`
  `ValidationProblem` (`errors.month`) before the action runs. A missing or empty `month` binds to
  `null`, and the use case takes the current month in São Paulo. Inputs carry `YearMonth?`;
  outputs write months as `yyyy-MM` strings.
- **Rationale**: It is how this API already validates query parameters — an unknown `type`/`orderBy`
  or a malformed `categoryId` fails model binding the same way — so the controllers stay one line of
  delegation each, and every report rejects a bad month identically. Verified with a probe host:
  `2026-09` binds; `2026-9`, `2026-13`, `2026-00`, `09-2026`, `2026-09-01`, `abc` and ` 2026-09` are
  `400`; `?month=` binds to `null`.
- **Alternatives considered**: A `string? month` parsed in each action — five copies of the same
  parsing, or a helper, in "thin" controllers; `DateTime?`/`DateOnly?` — their binders accept many
  formats (`2026-09-01`, `09/2026`), so `YYYY-MM` would not be enforced; a Domain value object — the
  month is a reporting input, not something an entity stores, and the IO boundary keeps
  domain types out of Inputs (see the `add-value-object` skill).

## The number of months of the income-vs-expense series

- **Decision**: `[FromQuery, Range(1, 24)] int months = 12` on the action. Out of range or not a
  number → `400` `ValidationProblem` (`errors.months`) from `[ApiController]`, without calling the
  use case.
- **Rationale**: Same mechanism and response shape as the invalid month; the request puts input
  validation in the controller.
- **Alternatives considered**: Validating in the use case and throwing `AppException` — a different
  `400` shape (`ProblemDetails` with `detail`) for the same kind of error as an invalid month.

## Routes

- **Decision**: `ReportsController` under the existing `api/[controller]` route, with camelCase
  segments: `GET /api/reports/summary`, `/api/reports/expensesByCategory`,
  `/api/reports/incomeVsExpense`, `/api/reports/cumulativeExpenses`, `/api/reports/transactions`.
  Written as literal templates (`[HttpGet("expensesByCategory")]`), not `[action]`, so renaming a
  method can't move a public route.
- **Rationale**: The request lists `/reports/summary`, `/reports/expenses-by-category`, ... but also
  asks to follow the existing conventions and not to introduce new naming styles. Every route lives
  under `/api`, and the API exposes camelCase routes and enum values on purpose (`CamelCaseRouteTransformer`,
  commit `4eb9aad`, and the front calling `orderBy=categoryName`). Kebab-case would be the first
  route in a different style.
- **Alternatives considered**: The literal kebab-case paths — rejected for the reason above; it is a
  three-string change in the controller (and one in the front service) if the owner prefers them.

## Aggregation in the database

- **Decision**: Four new `ITransactionRepository` queries, implemented in
  `TransactionRepository` with EF Core LINQ that PostgreSQL executes as `WHERE "Date" >= @from AND
  "Date" < @to ... GROUP BY ... SUM(...)`:
  - `SumByMonthAsync(from, to)` → `(Year, Month, Income, Expense)` per month with transactions
    (`GROUP BY` year and month; income and expense as conditional sums in the same pass).
  - `SumExpensesByCategoryAsync(from, to)` → `(Category, Expense)` per category with expenses (a
    grouped subquery joined to `Categories` for the name and color).
  - `SumExpensesByDayAsync(from, to)` → `(Date, Expense)` per day with expenses.
  - `GetWithCategoryNameAsync(from, to)` → `(Transaction, CategoryName)` for every transaction in the
    range (joined to `Categories`; not an aggregate, but limited to the month).
  Ranges are `DateOnly` calendar days, `[from, to)`; the repository turns them into UTC midnights,
  the representation it stores. Results are named tuples. Read-only queries use `AsNoTracking()`.
- **Rationale**: The request requires `GROUP BY`/`SUM` in the database instead of loading everything
  (the existing list use cases load every transaction and filter in memory, which the request rules
  out here). Keeping the sums in the repository and the rules in the use cases keeps both testable:
  the rules in `Denarius.Application.Tests` with a substituted repository, the SQL in the quickstart
  run against PostgreSQL. Named tuples already appear in this codebase's signatures
  (`IUpdateTransactionUseCase`'s `(Guid Id, UpdateTransactionInput Input)`) and avoid read-model
  types in `Domain` that only these queries would use; `DateOnly` keeps "a day is stored as its UTC
  midnight" inside Infrastructure.
- **Alternatives considered**: One "report repository" interface in `Application` — the constitution
  allows it, but every repository interface here lives in `Domain/Repositories`; raw SQL — EF Core
  translates every query needed, keeping the queries typed and provider-neutral; loading the month's
  transactions and summing in the use case — explicitly ruled out by the request.

## Indexes

- **Decision**: Add `IX_Transactions_Date` (B-tree on `Date`) through
  `TransactionConfiguration.HasIndex(t => t.Date)` and a migration, `AddTransactionDateIndex`, whose
  `Down()` drops it. `IX_Transactions_CategoryId` already exists (migration `AddTransaction`, for the
  foreign key), so nothing is added for the category.
- **Rationale**: Every report filters by a date range first; the category grouping and the join to
  `Categories` then work on the month's rows only.
- **Alternatives considered**: A composite `(CategoryId, Date)` or a covering index with `INCLUDE
  ("Value", "CategoryId")` — measurable only at volumes far beyond this single-tenant app's, and a
  wider index on every write.

## Percentages, rounding and division by zero

- **Decision**: Every percentage is on a 0–100 scale, rounded to two decimals with
  `MidpointRounding.AwayFromZero`: `savingsRate = balance / totalIncome × 100`, category
  `percentage = amount / total × 100`, and each previous-month change
  `(current − previous) / |previous| × 100`. Projected money is rounded to cents the same way.
  `savingsRate` is `null` when the month has no income; a change is `null` when the previous value is
  zero. The savings rate can be negative (expense above income).
- **Rationale**: One scale for every field that holds a percentage, so `percentage: 25.5` and
  `savingsRate: 25.5` mean the same thing; `null` says "not applicable" where 0% would be a false
  statement (no income means nothing could be saved, and a change from zero has no base). Dividing by
  the size of the previous value keeps the sign meaningful: a balance from −100.00 to 50.00 is
  +150%, not −150%.
- **Alternatives considered**: Ratios (0–1) — matches the request's `balance / totalIncome` literally,
  but makes a field named `percentage` hold 0.255; `0` without income — indistinguishable from
  "earned and saved nothing".

## Projection

- **Decision**: Compare the report month with the current month in São Paulo. Past month: projected
  expense = total expense, projected balance = balance. Future month: both zero. Current month:
  `round(expenseToDate / today.Day × daysInMonth, 2)`, where `expenseToDate` sums expenses dated from
  the 1st through today (`SumByMonthAsync(firstDay, today + 1)`), and projected balance = the month's
  total income − projected expense.
- **Rationale**: The request's rule, read literally: "despesa até hoje" excludes expenses already
  recorded for later days of the month, and today counts as an elapsed day, so the divisor is never
  zero. Only expense is projected; income usually arrives once and is taken as known.
- **Alternatives considered**: Using the month's total expense as "expense to date" — overstates the
  pace whenever a future bill is recorded early.

## "Outras" in the expenses by category

- **Decision**: Sort by amount descending, then by category name; with more than eight categories,
  keep the first seven and add the rest into a final entry `{ categoryId: null, categoryName:
  "Outras", color: null }`, so the list never exceeds eight entries. "Outras" is always last.
  Percentages are computed for every entry against the month's total expense.
- **Rationale**: Eight slices is what a doughnut can still show legibly; the request's threshold is
  "more than 8 categories". The tail stays last because it is the tail, even if its sum is larger than
  some categories shown on their own.
- **Alternatives considered**: Eight categories plus "Outras" (nine entries) — exceeds the threshold
  the request uses; sorting "Outras" among the categories by its sum — reads as if "Outras" were a
  category.

## Continuous series

- **Decision**: Income vs. expense builds the list of months in the use case — from
  `month − (months − 1)` to `month` — and fills each from the grouped sums, with zeros where the
  database returned no row. The cumulative comparison builds one entry per day from 1 to the last day
  of each series (today for the current month, none for a future month, the month's last day
  otherwise), carrying the running total across days without expenses.
- **Rationale**: `GROUP BY` only returns months and days that have rows; the gaps are a rule of the
  report ("série contínua, sem buracos"), so they are filled where the rules live and are unit tested.
- **Alternatives considered**: `generate_series` in SQL — moves the rule into untested SQL.

## The transaction type in the monthly transactions

- **Decision**: The new `type` field reuses `TransactionType`, set from the sign (`In` for a positive
  value, `Out` for a negative one), and `amount` is the absolute value. `TransactionType` gets
  `[JsonConverter(typeof(JsonStringEnumConverter<TransactionType>))]` and
  `[JsonStringEnumMemberName]` on its members, so it serializes as `"in"`/`"out"` — the same values
  the `type` query filter of `GET /api/transactions` accepts.
- **Rationale**: The domain's own vocabulary for the type is `In`/`Out`; an attribute on the type
  works the same in the API host, in the test host and in the generated OpenAPI document, with no
  global serializer setting. System.Text.Json ships with the framework, so `Application` takes no
  new dependency. Query binding uses the enum's type converter, which ignores these attributes.
- **Alternatives considered**: A global `JsonStringEnumConverter` in `Program.cs` — the test hosts and
  the OpenAPI generator read different options and would each need it; a `string` property —
  untyped.

## Shapes of the outputs

- **Decision**: One Input and one Output record per report under `IO/Reports`, written like the
  existing records (explicit `{ get; init; }` properties and a constructor), named after the use case
  (`GetMonthlySummaryInput`, `MonthlySummaryOutput`, ...). Item records for the lists
  (`CategoryExpenseOutput`, `IncomeVsExpenseOutput`, `AccumulatedExpenseOutput`,
  `MonthlyTransactionOutput`) and `PreviousMonthSummaryOutput` for the summary's comparison. The
  shapes are exactly the request's; `previousMonth` in the cumulative report is the previous
  month's series, as requested.
- **Rationale**: The request asks for dedicated Inputs and Outputs and no entity as a response.
- **Alternatives considered**: Positional records — more concise, but not the style of the existing
  IO records.

## Documentation

- **Decision**: [contracts/reports-api.yaml](./contracts/reports-api.yaml) documents the five
  endpoints in the same OpenAPI 3.0.3 form as the other features' contracts. The runtime OpenAPI
  document (`AddOpenApi`/`MapOpenApi`, served at `/openapi/v1.json` in Development) lists the new
  controller with no extra code.
- **Rationale**: These are the two documentation mechanisms the project already has; there is no
  backend README.
- **Alternatives considered**: Adding Swagger UI or XML comments — a new mechanism the project
  doesn't use.

## Testing approach

- **Decision**: Use case tests substitute `ITransactionRepository` (asserting the exact `[from, to)`
  ranges requested, including year turns) and `TimeProvider` (`GetUtcNow()` stubbed, e.g.
  `2026-10-01T02:00Z` to prove the São Paulo month); `YearMonth` gets its own tests; controller tests
  follow `TransactionsControllerTests` (hand-written fakes, `TestServer`, `AddApplicationPart`).
  Repository queries are exercised end to end by the quickstart against a scratch PostgreSQL
  database seeded with known rows.
- **Rationale**: Matches the existing test projects and the constitution's Principle IV; there is no
  Infrastructure test project and adding one (with a database) is out of scope.
- **Alternatives considered**: `Microsoft.Extensions.TimeProvider.Testing`'s `FakeTimeProvider` — a new
  package for what one stubbed virtual method does; an Infrastructure test project with Testcontainers
  — a new dependency and pattern.

**Output**: All Technical Context items are resolved; no `NEEDS CLARIFICATION` remain.
