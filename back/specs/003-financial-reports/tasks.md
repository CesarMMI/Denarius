---

description: "Task list for the Financial Reports feature"
---

# Tasks: Financial Reports

**Input**: Design documents from `/specs/003-financial-reports/`

**Prerequisites**: plan.md (present), spec.md (present), research.md (present),
data-model.md (present), contracts/ (present)

**Tests**: Requested by the feature request — unit tests for every use case (empty month, only
income, only expense, month/year turn, current vs. past month, division by zero) and controller tests
for every endpoint. Within each story the tests come first and must fail before the code exists.

**Organization**: Tasks are grouped by user story, in `spec.md`'s priority order (P1–P5). Each story
adds one report end to end — repository query (when new), IO records, use case, DI registration,
controller action — and is testable on its own.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Different files, no dependency on an incomplete task
- **[Story]**: Which user story this task belongs to (US1–US5)
- File paths are exact, relative to the repository root (`back/`)

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm the starting point. No project, package or tool is added by this feature.

- [X] T001 Run `dotnet test Denarius.slnx` on the untouched solution and record the baseline (all
      three suites must pass before any change).

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The month type, "today" in São Paulo and the date index every report relies on.

**⚠️ CRITICAL**: No user story can be completed before this phase.

- [X] T002 [P] `YearMonthTests` — `TryParse` accepts exactly `yyyy-MM` (`2026-09`) and rejects `2026-9`,
      `2026-13`, `2026-00`, `09-2026`, `2026-09-01`, `abc`, blank and null; `AddMonths` across year
      boundaries in both directions; `DayCount` (30, 31, 28, 29 in 2028); `FromDate`; `CompareTo`;
      `ToString()` → `yyyy-MM` — in `tests/Denarius.Application.Tests/IO/Reports/YearMonthTests.cs`.
- [X] T003 [P] `YearMonth` `readonly record struct` ("a calendar month, held as its first day"):
      `Year`, `Month`, `FirstDay`, `DayCount`, `AddMonths(int)`, `FromDate(DateOnly)`,
      `TryParse(string?, IFormatProvider?, out YearMonth)` with `DateOnly.TryParseExact(value,
      "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, ...)`, `IComparable<YearMonth>`,
      `ToString()` → `yyyy-MM` — in `src/Denarius.Application/IO/Reports/YearMonth.cs`.
- [X] T004 [P] `TimeProviderExtensions.GetTodayInSaoPaulo(this TimeProvider)` — `GetUtcNow()`
      converted with `TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo")`, as a `DateOnly` — in
      `src/Denarius.Application/UseCases/Reports/TimeProviderExtensions.cs`.
- [X] T005 Register `TimeProvider.System` with `TryAddSingleton` in
      `src/Denarius.Application/DependencyInjection.cs`.
- [X] T006 Index `Transactions.Date`: `builder.HasIndex(t => t.Date)` in
      `src/Denarius.Infrastructure/Persistence/Configurations/TransactionConfiguration.cs`, then
      `dotnet ef migrations add AddTransactionDateIndex --project src/Denarius.Infrastructure
      --startup-project src/Denarius.WebAPI`; check that `Up()` only creates `IX_Transactions_Date`
      and `Down()` only drops it (`src/Denarius.Infrastructure/Migrations/*_AddTransactionDateIndex.cs`
      and the model snapshot).

**Checkpoint**: Foundation ready — every story below can start.

---

## Phase 3: User Story 1 - See how the month is going at a glance (Priority: P1) 🎯 MVP

**Goal**: `GET /api/reports/summary?month=YYYY-MM` returns the month's totals, savings rate,
projection and comparison with the previous month.

**Independent Test**: Seed two consecutive months, call the summary for the second one and check every
field against `quickstart.md` step 4.1–4.4.

### Tests for User Story 1

- [X] T007 [P] [US1] `GetMonthlySummaryUseCaseTests` — month without data (zeros, `SavingsRate` null,
      changes null); only income (`SavingsRate` 100); only expense (`SavingsRate` null, negative
      balance); savings rate rounded to two decimals; past month (projection = actual values); current
      month (`ExpenseToDate / today.Day × DayCount`, rounded; `ProjectedBalance` = income − projected
      expense; expense-to-date range `[day 1, today + 1)`); first day of the current month; future
      month (projection 0); previous month and its changes, including a zero previous value (null) and
      a negative previous balance (measured against its size); January compared with December of the
      previous year (repository range `[2025-12-01, 2026-02-01)`); no month → the current month in
      São Paulo, also when UTC is already in the next month — in
      `tests/Denarius.Application.Tests/UseCases/Reports/GetMonthlySummary/GetMonthlySummaryUseCaseTests.cs`.
- [X] T008 [P] [US1] `ReportsControllerTests` for the summary — `200` with the body; `month=2026-09`
      bound to `YearMonth(2026, 9)`; no month and an empty `month=` → `null`; `400` without calling the
      use case for `2026-13`, `2026-9`, `09-2026`, `2026-09-01`, `abc`; `savingsRate` serialized as
      `null` — in `tests/Denarius.WebAPI.Tests/Reports/ReportsControllerTests.cs`.

### Implementation for User Story 1

- [X] T009 [P] [US1] `SumByMonthAsync(DateOnly from, DateOnly to)` →
      `IEnumerable<(int Year, int Month, decimal Income, decimal Expense)>`: one row per month with
      transactions in `[from, to)`, income = sum of positive values, expense = size of the sum of
      negative values — declared in `src/Denarius.Domain/Repositories/ITransactionRepository.cs` and
      implemented in `src/Denarius.Infrastructure/Repositories/TransactionRepository.cs` as a single
      `GROUP BY` year/month query over the UTC midnights of the range.
- [X] T010 [P] [US1] `GetMonthlySummaryInput` (`YearMonth? Month`), `MonthlySummaryOutput` (`Month`,
      `TotalIncome`, `TotalExpense`, `Balance`, `SavingsRate` (`decimal?`), `ProjectedExpense`,
      `ProjectedBalance`, `PreviousMonth`) and `PreviousMonthSummaryOutput` (`TotalIncome`,
      `TotalExpense`, `Balance`, `TotalIncomeChange`, `TotalExpenseChange`, `BalanceChange`, each
      change `decimal?`) in `src/Denarius.Application/IO/Reports/`.
- [X] T011 [US1] `IGetMonthlySummaryUseCase` and `GetMonthlySummaryUseCase` (`ITransactionRepository`,
      `TimeProvider`) — percentages on a 0–100 scale and projected money rounded to two decimals
      `AwayFromZero`; `SavingsRate` null without income; change `(current − previous) / |previous| ×
      100`, null when the previous value is 0 — in
      `src/Denarius.Application/UseCases/Reports/GetMonthlySummary/` (depends on T003, T004, T009,
      T010).
- [X] T012 [US1] Register `IGetMonthlySummaryUseCase` in `src/Denarius.Application/DependencyInjection.cs`.
- [X] T013 [US1] `ReportsController` (`[ApiController]`, `api/[controller]`) with
      `[HttpGet("summary")] Summary([FromQuery] YearMonth? month)` delegating to the use case — in
      `src/Denarius.WebAPI/Controllers/ReportsController.cs` (depends on T011).

**Checkpoint**: The summary works end to end — the MVP of the dashboard.

---

## Phase 4: User Story 2 - See where the money went (Priority: P2)

**Goal**: `GET /api/reports/expensesByCategory?month=YYYY-MM` returns the month's total expense and
each category's amount and share, largest first, with "Outras" beyond eight categories.

**Independent Test**: `quickstart.md` step 4.5 — September has ten categories (seven + Outras), August
three.

### Tests for User Story 2

- [X] T014 [P] [US2] `GetExpensesByCategoryUseCaseTests` — month without expense (total 0, no items);
      largest first with percentages, ties by name, name and color from the category; exactly eight
      categories (no Outras); ten categories (seven + Outras last, with null id and color, the sum of
      the rest and its share); repository range for the month, crossing the year for December; no
      month → current month in São Paulo — in
      `tests/Denarius.Application.Tests/UseCases/Reports/GetExpensesByCategory/GetExpensesByCategoryUseCaseTests.cs`.
- [X] T015 [P] [US2] Controller tests for `expensesByCategory` — `200` with the body (Outras with
      `null` id/color), month binding, `400` for an invalid month — in
      `tests/Denarius.WebAPI.Tests/Reports/ReportsControllerTests.cs` (after T008, same file).

### Implementation for User Story 2

- [X] T016 [P] [US2] `SumExpensesByCategoryAsync(DateOnly from, DateOnly to)` →
      `IEnumerable<(Category Category, decimal Expense)>`: expense per category with expenses in
      `[from, to)`, grouped by `CategoryId` and joined to `Categories` — in
      `src/Denarius.Domain/Repositories/ITransactionRepository.cs` and
      `src/Denarius.Infrastructure/Repositories/TransactionRepository.cs` (after T009, same files).
- [X] T017 [P] [US2] `GetExpensesByCategoryInput`, `ExpensesByCategoryOutput` (`Total`, `Items`) and
      `CategoryExpenseOutput` (`CategoryId` `Guid?`, `CategoryName`, `Color` `string?`, `Amount`,
      `Percentage`) in `src/Denarius.Application/IO/Reports/`.
- [X] T018 [US2] `IGetExpensesByCategoryUseCase` and `GetExpensesByCategoryUseCase` — sort by amount
      descending then name; more than 8 → the 7 largest plus "Outras"; percentage = amount / total ×
      100, two decimals — in `src/Denarius.Application/UseCases/Reports/GetExpensesByCategory/`
      (depends on T016, T017).
- [X] T019 [US2] Register `IGetExpensesByCategoryUseCase` in `src/Denarius.Application/DependencyInjection.cs`.
- [X] T020 [US2] `[HttpGet("expensesByCategory")] ExpensesByCategory([FromQuery] YearMonth? month)` in
      `src/Denarius.WebAPI/Controllers/ReportsController.cs` (depends on T018).

**Checkpoint**: Summary and expenses by category work independently.

---

## Phase 5: User Story 3 - Follow income against expense over the months (Priority: P3)

**Goal**: `GET /api/reports/incomeVsExpense?month=YYYY-MM&months=12` returns a continuous,
chronological series of monthly income, expense and balance.

**Independent Test**: `quickstart.md` step 4.6 — twelve months ending in October 2026, with February to
July at zero.

### Tests for User Story 3

- [X] T021 [P] [US3] `GetIncomeVsExpenseUseCaseTests` — default 12 months ending in the month; months
      without data filled with zeros (and an empty history: all zeros); a series crossing the year
      (`month=2026-02`, `months=4` → 2025-11 … 2026-02, repository range `[2025-11-01, 2026-03-01)`);
      `months=1`; balance = income − expense, with only income and with only expense; no month →
      current month in São Paulo — in
      `tests/Denarius.Application.Tests/UseCases/Reports/GetIncomeVsExpense/GetIncomeVsExpenseUseCaseTests.cs`.
- [X] T022 [P] [US3] Controller tests for `incomeVsExpense` — `200` with the list; `months` default 12,
      `months=1` and `months=24` bound; `400` without calling the use case for `months=0`, `25`, `abc`
      and for an invalid month — in `tests/Denarius.WebAPI.Tests/Reports/ReportsControllerTests.cs`.

### Implementation for User Story 3

- [X] T023 [P] [US3] `GetIncomeVsExpenseInput` (`YearMonth? Month`, `int Months = 12`) and
      `IncomeVsExpenseOutput` (`Month`, `Income`, `Expense`, `Balance`) in
      `src/Denarius.Application/IO/Reports/`.
- [X] T024 [US3] `IGetIncomeVsExpenseUseCase` and `GetIncomeVsExpenseUseCase` — one entry per month from
      `month − (months − 1)` to `month`, from `SumByMonthAsync`, zeros where there is no row — in
      `src/Denarius.Application/UseCases/Reports/GetIncomeVsExpense/` (depends on T009, T023).
- [X] T025 [US3] Register `IGetIncomeVsExpenseUseCase` in `src/Denarius.Application/DependencyInjection.cs`.
- [X] T026 [US3] `[HttpGet("incomeVsExpense")] IncomeVsExpense([FromQuery] YearMonth? month,
      [FromQuery, Range(1, 24)] int months = 12)` in
      `src/Denarius.WebAPI/Controllers/ReportsController.cs` (depends on T024).

**Checkpoint**: Three reports work independently.

---

## Phase 6: User Story 4 - Compare this month's spending pace with last month's (Priority: P4)

**Goal**: `GET /api/reports/cumulativeExpenses?month=YYYY-MM` returns the daily running total of
expense for the month and the previous month, and their number of days.

**Independent Test**: `quickstart.md` step 4.7 — October stops at day 3, September has 30 days.

### Tests for User Story 4

- [X] T027 [P] [US4] `GetCumulativeExpenseComparisonUseCaseTests` — past month: one entry per day, the
      running total unchanged on days without expense and ending at the month's total; month without
      data: zeros for every day; current month: stops at today, the previous month complete; future
      month: no days (and its previous month, when also future, no days); January vs. December of the
      previous year (31 + 31 days, repository range `[2025-12-01, 2026-02-01)`); February 2026 (28) and
      2028 (29); no month → current month in São Paulo — in
      `tests/Denarius.Application.Tests/UseCases/Reports/GetCumulativeExpenseComparison/GetCumulativeExpenseComparisonUseCaseTests.cs`.
- [X] T028 [P] [US4] Controller tests for `cumulativeExpenses` — `200` with both series and day counts,
      month binding, `400` for an invalid month — in
      `tests/Denarius.WebAPI.Tests/Reports/ReportsControllerTests.cs`.

### Implementation for User Story 4

- [X] T029 [P] [US4] `SumExpensesByDayAsync(DateOnly from, DateOnly to)` →
      `IEnumerable<(DateOnly Date, decimal Expense)>`: expense per day with expenses in `[from, to)`,
      grouped by the UTC year, month and day — in
      `src/Denarius.Domain/Repositories/ITransactionRepository.cs` and
      `src/Denarius.Infrastructure/Repositories/TransactionRepository.cs` (after T016, same files).
- [X] T030 [P] [US4] `GetCumulativeExpenseComparisonInput`, `CumulativeExpenseComparisonOutput`
      (`CurrentMonth`, `PreviousMonth`, `DaysInCurrentMonth`, `DaysInPreviousMonth`) and
      `AccumulatedExpenseOutput` (`Day`, `Accumulated`) in `src/Denarius.Application/IO/Reports/`.
- [X] T031 [US4] `IGetCumulativeExpenseComparisonUseCase` and `GetCumulativeExpenseComparisonUseCase` —
      one query for both months; each series from day 1 to the month's last day, today for the current
      month, none for a future month — in
      `src/Denarius.Application/UseCases/Reports/GetCumulativeExpenseComparison/` (depends on T029,
      T030).
- [X] T032 [US4] Register `IGetCumulativeExpenseComparisonUseCase` in
      `src/Denarius.Application/DependencyInjection.cs`.
- [X] T033 [US4] `[HttpGet("cumulativeExpenses")] CumulativeExpenses([FromQuery] YearMonth? month)` in
      `src/Denarius.WebAPI/Controllers/ReportsController.cs` (depends on T031).

**Checkpoint**: Four reports work independently.

---

## Phase 7: User Story 5 - List everything that happened in the month (Priority: P5)

**Goal**: `GET /api/reports/transactions?month=YYYY-MM` returns every transaction of the month with
category name, type and positive amount, newest first.

**Independent Test**: `quickstart.md` step 4.8 — the twelve September transactions, newest first.

### Tests for User Story 5

- [X] T034 [P] [US5] `ListMonthlyTransactionsUseCaseTests` — every transaction mapped (category name,
      `In`/`Out` from the sign, absolute amount, date and description); newest date first, ties by the
      most recently created; no cap (30 transactions → 30); month without transactions → empty;
      repository range for the month, crossing the year for December; no month → current month in São
      Paulo — in
      `tests/Denarius.Application.Tests/UseCases/Reports/ListMonthlyTransactions/ListMonthlyTransactionsUseCaseTests.cs`.
- [X] T035 [P] [US5] Controller tests for `transactions` — `200` with the list, `type` serialized as
      `"in"`/`"out"`, month binding, `400` for an invalid month — in
      `tests/Denarius.WebAPI.Tests/Reports/ReportsControllerTests.cs`.

### Implementation for User Story 5

- [X] T036 [P] [US5] `GetWithCategoryNameAsync(DateOnly from, DateOnly to)` →
      `IEnumerable<(Transaction Transaction, string CategoryName)>`: the transactions in `[from, to)`
      joined to their category's name, `AsNoTracking` — in
      `src/Denarius.Domain/Repositories/ITransactionRepository.cs` and
      `src/Denarius.Infrastructure/Repositories/TransactionRepository.cs` (after T029, same files).
- [X] T037 [P] [US5] `TransactionType` serialized as `"all"`/`"in"`/`"out"`:
      `[JsonConverter(typeof(JsonStringEnumConverter<TransactionType>))]` and
      `[JsonStringEnumMemberName]` on each member, in
      `src/Denarius.Application/IO/Transactions/TransactionType.cs` (query binding unchanged).
- [X] T038 [P] [US5] `ListMonthlyTransactionsInput` and `MonthlyTransactionOutput` (`Id`, `Date`,
      `Description`, `CategoryName`, `Type`, `Amount`, built from a `Transaction` and its category
      name) in `src/Denarius.Application/IO/Reports/`.
- [X] T039 [US5] `IListMonthlyTransactionsUseCase` and `ListMonthlyTransactionsUseCase` — order by
      `Date` descending, then `CreatedAt` descending — in
      `src/Denarius.Application/UseCases/Reports/ListMonthlyTransactions/` (depends on T036–T038).
- [X] T040 [US5] Register `IListMonthlyTransactionsUseCase` in
      `src/Denarius.Application/DependencyInjection.cs`.
- [X] T041 [US5] `[HttpGet("transactions")] Transactions([FromQuery] YearMonth? month)` in
      `src/Denarius.WebAPI/Controllers/ReportsController.cs` (depends on T039).

**Checkpoint**: All five reports work independently.

---

## Phase 8: Polish & Cross-Cutting Concerns

- [X] T042 Run `dotnet test Denarius.slnx` — all three suites pass, with the new tests (Constitution
      Principle IV): Domain.Tests 35, Application.Tests 133 (54 before), WebAPI.Tests 63 (38 before) — 231
      in all. `RoutingExtensionsTests` lists every route, so its expectation gained the five report routes.
- [X] T043 Run `quickstart.md` on a scratch database: migration applied, rolled back to
      `AddTransaction` and re-applied (Principle III); seed; every call of step 4 matches; `400`s; the
      volume check of step 5; `/openapi/v1.json` lists the five routes; drop the scratch database.
      Ran on 2026-10-03 against PostgreSQL 18 (server time zone America/Sao_Paulo, so a UTC-midnight date
      grouped in the wrong zone would have moved to the previous day): `IX_Transactions_Date` created,
      dropped by the rollback with `IX_Transactions_CategoryId` untouched, created again; every response of
      step 4 matched (the generated SQL groups by `date_part(... AT TIME ZONE 'UTC')` with `SUM(CASE ...)`
      over the date range only); all `400`s; with 10,027 transactions every report answered in 4–20 ms,
      the plan using a bitmap scan on `IX_Transactions_Date`; the OpenAPI document lists the five routes.
      The same database then served the front's visual check, and was dropped. The development database
      was not touched: it still needs `dotnet ef database update` for the index.
- [X] T044 [P] Re-check `contracts/reports-api.yaml` and `data-model.md` against the shipped code and
      mark this task list complete.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup — blocks every story (`YearMonth`, "today", the index).
- **User Stories (Phases 3–7)**: Depend on Foundational. Built in priority order (P1 → P5); each one
  is testable on its own once built.
- **Polish (Phase 8)**: Depends on the five stories.

### User Story Dependencies

- **US1 (P1)**: Foundational only. Creates `ReportsController` and `SumByMonthAsync`.
- **US2 (P2)**: Foundational; adds an action to the controller US1 created.
- **US3 (P3)**: Reuses US1's `SumByMonthAsync`; adds an action.
- **US4 (P4)**: Foundational; adds a query and an action.
- **US5 (P5)**: Foundational; adds a query and an action.

### Within Each User Story

- Tests first, failing; then repository query → IO records → use case → DI → controller action.
- `ITransactionRepository.cs`, `TransactionRepository.cs`, `DependencyInjection.cs`,
  `ReportsController.cs` and `ReportsControllerTests.cs` are shared files: their tasks run one story
  at a time even when marked [P] against the other files of the same story.

### Parallel Opportunities

- T002/T003/T004 (Foundational): different files.
- In each story, the use case tests, the controller tests, the repository query and the IO records
  touch different files and can be written together.

---

## Parallel Example: User Story 1

```bash
Task: "GetMonthlySummaryUseCaseTests in tests/Denarius.Application.Tests/UseCases/Reports/GetMonthlySummary/GetMonthlySummaryUseCaseTests.cs"
Task: "ReportsControllerTests (summary) in tests/Denarius.WebAPI.Tests/Reports/ReportsControllerTests.cs"
Task: "SumByMonthAsync in src/Denarius.Domain/Repositories/ITransactionRepository.cs + src/Denarius.Infrastructure/Repositories/TransactionRepository.cs"
Task: "GetMonthlySummaryInput/MonthlySummaryOutput/PreviousMonthSummaryOutput in src/Denarius.Application/IO/Reports/"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

Setup + Foundational, then the summary: the dashboard's headline cards can ship on their own.

### Incremental Delivery

1. Setup + Foundational → `YearMonth`, today in São Paulo, the date index.
2. US1 → summary (MVP).
3. US2 → expenses by category.
4. US3 → income vs. expense series.
5. US4 → cumulative comparison.
6. US5 → transactions of the month.
7. Polish → full test run and the quickstart on PostgreSQL.

---

## Notes

- [P] tasks = different files, no dependencies.
- Quote the rules from data-model.md when implementing: expense is positive, percentages 0–100 with
  two decimals, `null` for not applicable.
- The repository queries have no automated test project; T043's quickstart run is what proves their
  SQL.
