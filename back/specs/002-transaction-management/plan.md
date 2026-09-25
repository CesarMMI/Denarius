# Implementation Plan: Transaction Management

**Branch**: `002-transaction-management` | **Date**: 2026-09-22 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-transaction-management/spec.md`

**Note**: This is a **retroactive** plan — it documents the design already shipped for this
feature, verified against the source under `src/` and its test suites under `tests/`, rather
than proposing new work. `/speckit-tasks` run against this plan should find nothing outstanding
beyond the documentation gap noted under Research. Updated 2026-09-24 to cover User Story 3
(list filtering and sorting), which was designed and shipped in the same change as this update.

## Summary

Transaction management provides CRUD over individual financial transactions — a date, a value
(positive or negative), a required category reference, and an optional description. It is
implemented as one vertical slice through the existing four-layer architecture: a
self-validating `Transaction` domain entity enforces the business rules; five single-purpose
Application use cases (Create/Update/Delete/GetById/List) orchestrate persistence, with
Create/Update additionally verifying the referenced `Category` exists; EF Core persists
`Transaction` to PostgreSQL with a restrict-on-delete foreign key to `Category`; a single
`TransactionsController` exposes the use cases over REST; and `GlobalExceptionHandler` translates
domain/not-found exceptions into `ProblemDetails` responses. Like
[001-category-management](../001-category-management/plan.md), the `List` use case takes a
`ListTransactionsInput` (description, `DateRef` month, type, category, `orderBy`/`asc`) and
applies the filters and sort in memory over the loaded transactions; there is no pagination.

## Technical Context

**Language/Version**: C# 13 / .NET 10 (`net10.0`)

**Primary Dependencies**: ASP.NET Core (`Microsoft.AspNetCore.OpenApi`) for the web host; Entity
Framework Core 10 + `Npgsql.EntityFrameworkCore.PostgreSQL` for persistence;
`Microsoft.Extensions.DependencyInjection.Abstractions` for the Application layer's DI contracts

**Storage**: PostgreSQL, accessed via EF Core code-first migrations
(`Denarius.Infrastructure/Migrations`); a `Transactions` table with a restrict-on-delete foreign
key to `Categories.Id` and a non-unique index on `CategoryId`

**Testing**: xUnit across all three test projects; `Denarius.Domain.Tests` and
`Denarius.Application.Tests` use NSubstitute for repository/unit-of-work doubles;
`Denarius.WebAPI.Tests` uses `Microsoft.AspNetCore.TestHost` for in-process integration tests

**Target Platform**: ASP.NET Core Web API (server-side), consumed by the Denarius Angular
front-end (separate `front/` project) and reachable at `api/transactions`

**Project Type**: web — backend half of a two-project web application; this plan covers the
backend only

**Performance Goals**: None formally specified. Current behavior: `GET /api/transactions` loads
every transaction into memory once per request, then filters and sorts in the use case (plus one
extra query for category names, only when sorting by `CategoryName`) — the same "acceptable at
today's scale" posture as `ListCategoriesUseCase`. Pushing the filters into the repository
query is the natural next step if volumes outgrow this (see research.md → List filtering and
sorting).

**Constraints**: Governed by `.specify/memory/constitution.md` — API responses must stay
backward compatible (Principle I), code must stay in its Clean Architecture layer (Principle
II), any future migration needs a verified rollback (Principle III), and `dotnet test` must pass
across all three suites (Principle IV).

**Scale/Scope**: Single-tenant personal-finance usage; transaction counts expected in the
hundreds to low thousands per user — the unpaginated, in-memory-filtered list is sized for this
range, not for large multi-tenant volumes.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|---|---|---|
| I. Public API Compatibility | PASS | User Story 3 is **additive**: `GET /api/transactions` gains six optional query parameters (`description`, `dateRef`, `type`, `categoryId`, `orderBy`, `asc`), and a call with none of them still returns every transaction with an unchanged response shape. The only observable difference for existing callers is that the previously unspecified order is now date descending. No other route, DTO, or status code changed. |
| II. Service Boundary Adherence | PASS | Validation lives in `Denarius.Domain` (`Transaction`); orchestration in `Denarius.Application` use cases depending only on `Domain` (plus `ICategoryRepository` for the cross-entity existence check and for category names when sorting by `CategoryName`); EF Core specifics confined to `Denarius.Infrastructure`; `Denarius.WebAPI`'s `TransactionsController` only binds query parameters and calls use-case interfaces. No layer is skipped. |
| III. Migration Rollback Discipline | PASS | The migration that creates the `Transactions` table (`20260814132713_AddTransaction`) has a clean, non-destructive `Down()` (`DropTable`). User Story 3 adds no migration. |
| IV. Test Suite Verification | PASS | The `Transaction` entity and all five use cases have dedicated xUnit coverage; `TransactionsController` itself is covered by `tests/Denarius.WebAPI.Tests/Transactions/TransactionsControllerTests.cs` (added by `/speckit-implement` on 2026-09-22, closing the gap Research originally flagged). User Story 3's filters, sort options, query binding, and 400-on-invalid-parameter behavior are covered in `ListTransactionsUseCaseTests.cs` and `TransactionsControllerTests.cs`. |

No violations — Complexity Tracking is not needed.

*Re-checked after Phase 1 design: unchanged — data-model.md and contracts/ describe the shipped
design, they don't introduce anything new to re-gate.*

## Project Structure

### Documentation (this feature)

```text
specs/002-transaction-management/
├── plan.md               # This file (/speckit-plan command output)
├── research.md           # Phase 0 output (/speckit-plan command)
├── data-model.md         # Phase 1 output (/speckit-plan command)
├── quickstart.md         # Phase 1 output (/speckit-plan command)
├── contracts/            # Phase 1 output (/speckit-plan command)
└── tasks.md              # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
src/
├── Denarius.Domain/
│   ├── Entities/Transaction.cs
│   └── Repositories/ITransactionRepository.cs
├── Denarius.Application/
│   ├── IO/Transactions/               # CreateTransactionInput, UpdateTransactionInput,
│   │                                   # ListTransactionsInput, TransactionOutput,
│   │                                   # TransactionType, TransactionOrderField
│   └── UseCases/Transactions/         # Create, Update, Delete, GetById, List
├── Denarius.Infrastructure/
│   ├── Persistence/Configurations/TransactionConfiguration.cs
│   ├── Repositories/TransactionRepository.cs
│   └── Migrations/20260814132713_AddTransaction.cs
└── Denarius.WebAPI/
    └── Controllers/TransactionsController.cs

tests/
├── Denarius.Domain.Tests/Entities/TransactionTests.cs
├── Denarius.Application.Tests/UseCases/Transactions/   # one folder per use case
└── Denarius.WebAPI.Tests/Transactions/TransactionsControllerTests.cs
```

**Structure Decision**: Same as [001-category-management](../001-category-management/plan.md) —
the template's Option 2 (web application: separate backend/frontend) narrowed to the backend
only, using this repo's actual Clean Architecture layout (`Denarius.Domain` /
`Denarius.Application` / `Denarius.Infrastructure` / `Denarius.WebAPI` under `src/`, mirrored 1:1
under `tests/`) rather than the template's generic `models/services/api` split.

## Complexity Tracking

Not applicable — the Constitution Check reported no violations.
