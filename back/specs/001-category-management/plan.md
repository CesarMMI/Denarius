# Implementation Plan: Category Management

**Branch**: `001-category-management` | **Date**: 2026-09-21 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-category-management/spec.md`

**Note**: This is a **retroactive** plan — it documents the design already shipped for this
feature, verified against the source under `src/` and its test suites under `tests/`, rather
than proposing new work. `/speckit-tasks` run against this plan should find nothing outstanding
beyond the documentation gap noted under Research.

## Summary

Category management provides full CRUD over spending categories (name + color) plus a usage
view — per-category transaction count and balance, optionally scoped to a calendar month,
filterable by name/usage, and sortable by name/count/balance. It is implemented as one vertical
slice through the existing four-layer architecture: a self-validating `Category` domain entity
and `Color` value object enforce the business rules; five single-purpose Application use cases
(Create/Update/Delete/GetById/List) orchestrate persistence and aggregation; EF Core persists
`Category` to PostgreSQL with a restrict-on-delete foreign key from `Transaction`; a single
`CategoriesController` exposes the use cases over REST; and `GlobalExceptionHandler` translates
domain/not-found exceptions into `ProblemDetails` responses.

## Technical Context

**Language/Version**: C# 13 / .NET 10 (`net10.0`)

**Primary Dependencies**: ASP.NET Core (`Microsoft.AspNetCore.OpenApi`) for the web host; Entity
Framework Core 10 + `Npgsql.EntityFrameworkCore.PostgreSQL` for persistence;
`Microsoft.Extensions.DependencyInjection.Abstractions` for the Application layer's DI contracts

**Storage**: PostgreSQL, accessed via EF Core code-first migrations
(`Denarius.Infrastructure/Migrations`); a `Categories` table with a restrict-on-delete foreign
key from `Transactions.CategoryId`

**Testing**: xUnit across all three test projects; `Denarius.Domain.Tests` and
`Denarius.Application.Tests` use NSubstitute for repository/unit-of-work doubles;
`Denarius.WebAPI.Tests` uses `Microsoft.AspNetCore.TestHost` for in-process integration tests

**Target Platform**: ASP.NET Core Web API (server-side), consumed by the Denarius Angular
front-end (separate `front/` project) and reachable at `api/categories`

**Project Type**: web — backend half of a two-project web application; this plan covers the
backend only

**Performance Goals**: None formally specified. Current behavior: `GET /api/categories` loads
every category and every transaction into memory once per request and aggregates with LINQ (see
Research: Usage aggregation strategy) — acceptable at today's personal-finance data scale, not
benchmarked beyond that.

**Constraints**: Governed by `.specify/memory/constitution.md` — API responses must stay
backward compatible (Principle I), code must stay in its Clean Architecture layer (Principle
II), any future migration needs a verified rollback (Principle III), and `dotnet test` must pass
across all three suites (Principle IV).

**Scale/Scope**: Single-tenant personal-finance usage; category counts in the tens, transaction
counts expected in the hundreds to low thousands per user — the unpaginated list and full
in-memory aggregation are sized for this range, not for large multi-tenant volumes.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|---|---|---|
| I. Public API Compatibility | PASS | This plan documents the existing `api/categories` surface as-is; no change is proposed. |
| II. Service Boundary Adherence | PASS | Validation lives in `Denarius.Domain` (`Category`, `Color`); orchestration in `Denarius.Application` use cases depending only on `Domain`; EF Core specifics confined to `Denarius.Infrastructure`; `Denarius.WebAPI`'s `CategoriesController` only calls use-case interfaces. No layer is skipped. |
| III. Migration Rollback Discipline | PASS | The migration that creates the `Categories` table (`20260813225006_InitialCreate`) has a clean, non-destructive `Down()` (`DropTable`). |
| IV. Test Suite Verification | PASS | The `Category` entity and all five use cases have dedicated xUnit coverage; `CategoriesController` itself is covered by `tests/Denarius.WebAPI.Tests/Categories/CategoriesControllerTests.cs` (added by `/speckit-implement` on 2026-09-21, closing the gap Research originally flagged). |

No violations — Complexity Tracking is not needed.

*Re-checked after Phase 1 design: unchanged — data-model.md and contracts/ describe the shipped
design, they don't introduce anything new to re-gate.*

## Project Structure

### Documentation (this feature)

```text
specs/001-category-management/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md         # Phase 1 output (/speckit-plan command)
├── quickstart.md         # Phase 1 output (/speckit-plan command)
├── contracts/            # Phase 1 output (/speckit-plan command)
└── tasks.md              # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
src/
├── Denarius.Domain/
│   ├── Entities/Category.cs
│   ├── ValueObjects/Color.cs
│   └── Repositories/ICategoryRepository.cs
├── Denarius.Application/
│   ├── IO/Categories/                 # CreateCategoryInput, UpdateCategoryInput,
│   │                                   # ListCategoriesInput, CategoryOutput, CategoryOrderField
│   └── UseCases/Categories/           # Create, Update, Delete, GetById, List
├── Denarius.Infrastructure/
│   ├── Persistence/Configurations/CategoryConfiguration.cs
│   ├── Repositories/CategoryRepository.cs
│   └── Migrations/20260813225006_InitialCreate.cs
└── Denarius.WebAPI/
    └── Controllers/CategoriesController.cs

tests/
├── Denarius.Domain.Tests/Entities/CategoryTests.cs
├── Denarius.Application.Tests/UseCases/Categories/   # one folder per use case
└── Denarius.WebAPI.Tests/                            # no Category-specific tests today (see Research)
```

**Structure Decision**: This is the template's Option 2 (web application: separate
backend/frontend) narrowed to the backend only, using this repo's actual Clean Architecture
layout (`Denarius.Domain` / `Denarius.Application` / `Denarius.Infrastructure` /
`Denarius.WebAPI` under `src/`, mirrored 1:1 under `tests/`) rather than the template's generic
`models/services/api` split.

## Complexity Tracking

Not applicable — the Constitution Check reported no violations.
