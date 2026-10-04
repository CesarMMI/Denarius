# Implementation Plan: Financial Reports

**Branch**: `003-financial-reports` | **Date**: 2026-10-03 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/003-financial-reports/spec.md`

## Summary

Five read-only reports for the front's reports dashboard, each one a vertical slice through the
existing layers: a `GET` action on a new `ReportsController` that only binds and validates its query
and delegates to a single-purpose Application use case (`GetMonthlySummary`,
`GetExpensesByCategory`, `GetIncomeVsExpense`, `GetCumulativeExpenseComparison`,
`ListMonthlyTransactions`), each with its own Input and Output records under `IO/Reports`. The use
cases hold the report rules (default month, savings rate, projection, percentage changes, "Outras",
continuous series, running totals) and get their numbers from four new `ITransactionRepository`
queries that `GROUP BY`/`SUM` in PostgreSQL over the requested months only. The month travels as a
`YYYY-MM` `YearMonth` bound by MVC through its `TryParse`, so an invalid month is a `400` before any
use case runs, exactly like the existing invalid `type`/`orderBy` values. "Today" — and so the
current month — is the day in America/Sao_Paulo, read from an injected `TimeProvider`; transaction
dates stay calendar days. A new migration adds the missing index on `Transactions.Date`
(`CategoryId` is already indexed).

## Technical Context

**Language/Version**: C# 14 / .NET 10 (`net10.0`)

**Primary Dependencies**: ASP.NET Core MVC (`Microsoft.AspNetCore.OpenApi`); Entity Framework Core
10 + `Npgsql.EntityFrameworkCore.PostgreSQL` 10; `System.TimeProvider` (BCL) for "today"; no new
packages

**Storage**: PostgreSQL via EF Core code-first migrations. `Transactions.Date` is
`timestamp with time zone` holding each calendar day at UTC midnight; `Value` is `numeric(18,2)`,
positive for money in and negative for money out. New index `IX_Transactions_Date`.

**Testing**: xUnit; `Denarius.Application.Tests` with NSubstitute for `ITransactionRepository` and
`TimeProvider`; `Denarius.WebAPI.Tests` with `TestServer` and hand-written use case fakes. There is
no Infrastructure test project, so the SQL translation of the new queries is validated against
PostgreSQL by the quickstart.

**Target Platform**: ASP.NET Core Web API at `api/reports`, consumed by the Denarius Angular front

**Project Type**: web — backend half of a two-project web application; this plan covers the backend

**Performance Goals**: Each report under one second for a history of 10,000 transactions (SC-002):
each request reads only the months it covers, with the sums done by PostgreSQL over the date index.

**Constraints**: Governed by `.specify/memory/constitution.md` (see Constitution Check). From the
request: money as `decimal`, never `float`/`double`; dates in America/Sao_Paulo; `month` as
`YYYY-MM`, current month when omitted, invalid → `400`; one endpoint and one use case per report;
thin controllers.

**Scale/Scope**: Single-tenant personal finance — hundreds to low thousands of transactions a year,
a few dozen categories. Five endpoints, five use cases, four repository queries, one migration.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|---|---|---|
| I. Public API Compatibility | PASS | Additive only: a new `ReportsController` with five new `GET` routes and new response shapes. `TransactionType` gains JSON attributes so the new `type` field reads `in`/`out`; no existing request or response body carries that enum, and the existing `type` query parameter keeps binding case-insensitively through its type converter, so no existing contract changes. |
| II. Service Boundary Adherence | PASS | `Denarius.Domain` gets four query signatures on `ITransactionRepository` and no new dependency. `Denarius.Application` holds every report rule and depends only on `Domain` plus the BCL `TimeProvider`. `Denarius.Infrastructure` implements the queries with EF Core. `ReportsController` binds, validates (`YearMonth`, `[Range]`) and delegates — no rule in `WebAPI`, no layer skipped. |
| III. Migration Rollback Discipline | PASS | One migration, `AddTransactionDateIndex`: `Up()` creates `IX_Transactions_Date`, `Down()` drops it — no data touched. The quickstart applies it, rolls it back to `AddTransaction` and re-applies it on a scratch database before review. |
| IV. Test Suite Verification | PASS | Each use case gets `Denarius.Application.Tests` coverage (empty month, only income, only expense, month/year turn, current vs. past/future month, division by zero), `YearMonth` its own tests, and `ReportsController` `Denarius.WebAPI.Tests` coverage (binding, defaults, `400`s, JSON shape). `dotnet test` must pass across all three suites. |

No violations — Complexity Tracking is not needed.

*Re-checked after Phase 1 design: unchanged. The design adds no project, package or layer
dependency; data-model.md and contracts/ only describe the additions above.*

## Project Structure

### Documentation (this feature)

```text
specs/003-financial-reports/
├── plan.md               # This file (/speckit-plan command output)
├── research.md           # Phase 0 output (/speckit-plan command)
├── data-model.md         # Phase 1 output (/speckit-plan command)
├── quickstart.md         # Phase 1 output (/speckit-plan command)
├── contracts/            # Phase 1 output (/speckit-plan command)
│   └── reports-api.yaml
└── tasks.md              # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
src/
├── Denarius.Domain/
│   └── Repositories/ITransactionRepository.cs        # + 4 report queries
├── Denarius.Application/
│   ├── IO/Reports/                                   # YearMonth, the 5 inputs and their outputs
│   ├── IO/Transactions/TransactionType.cs            # in/out as JSON strings
│   ├── UseCases/Reports/                             # TimeProviderExtensions (today in São Paulo)
│   │   ├── GetMonthlySummary/
│   │   ├── GetExpensesByCategory/
│   │   ├── GetIncomeVsExpense/
│   │   ├── GetCumulativeExpenseComparison/
│   │   └── ListMonthlyTransactions/
│   └── DependencyInjection.cs                        # + 5 use cases, TimeProvider.System
├── Denarius.Infrastructure/
│   ├── Persistence/Configurations/TransactionConfiguration.cs   # + index on Date
│   ├── Repositories/TransactionRepository.cs                   # + 4 report queries
│   └── Migrations/*_AddTransactionDateIndex.cs
└── Denarius.WebAPI/
    └── Controllers/ReportsController.cs

tests/
├── Denarius.Application.Tests/
│   ├── IO/Reports/YearMonthTests.cs
│   └── UseCases/Reports/                             # one folder per use case
└── Denarius.WebAPI.Tests/Reports/ReportsControllerTests.cs
```

**Structure Decision**: Same as [002-transaction-management](../002-transaction-management/plan.md):
the backend half of the web application, in this repo's Clean Architecture layout, mirrored 1:1 under
`tests/`. Reports are not an entity, so they add no entity, configuration or repository of their
own: they read `Transaction` (and `Category`) through `ITransactionRepository`, and their IO and use
cases follow the `IO/{Feature}` and `UseCases/{Feature}/{Operation}` folders the other features use.

## Complexity Tracking

Not applicable — the Constitution Check reported no violations.
