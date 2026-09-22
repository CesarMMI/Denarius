# Phase 0 Research: Category Management

This is a retroactive plan, so "research" means confirming the decisions already embedded in
the shipped code and tests, rather than exploring options for code not yet written. Each entry
below resolves what would otherwise be a `NEEDS CLARIFICATION` in Technical Context.

## Persistence technology

- **Decision**: PostgreSQL via EF Core code-first migrations, using the project-wide
  `DenariusDbContext`.
- **Rationale**: Already established for the whole backend (constitution's Architectural
  Constraints). `Category` has no requirement — schema flexibility, graph queries, and so on —
  that would justify a different store.
- **Alternatives considered**: None specific to `Category`; this was inherited from the
  project-wide stack, not decided per feature.

## Usage aggregation strategy (transaction count & balance per category)

- **Decision**: Computed in the Application layer (`ListCategoriesUseCase`), by loading all
  categories and all transactions through their repositories and aggregating in-memory with
  LINQ (`GroupBy(t => t.CategoryId)`), rather than pushing a `GROUP BY`/join into the repository
  or database.
- **Rationale**: Keeps SQL-shaping logic out of `Domain`/`Application` (Constitution Principle
  II) and keeps the use case trivially testable with plain in-memory collections and NSubstitute
  doubles, as seen in `ListCategoriesUseCaseTests`.
- **Alternatives considered**: A SQL-side aggregate (a repository method returning pre-grouped
  counts/sums) would scale better but would widen the repository contract and move logic into
  `Infrastructure`. Implicitly rejected in favor of simplicity at the project's current data
  scale (see Scale/Scope in plan.md).
- **Flagged trade-off**: `ITransactionRepository.GetAllAsync()` fetches the entire
  `Transactions` table on every category-list request, with no date-range push-down even when
  `dateRef` narrows the result to one month. Fine today; worth revisiting if transaction volume
  grows substantially.

## Validation placement

- **Decision**: Validation lives inside the `Category` constructor/`Update` method and the
  `Color` value-object constructor (Domain layer), throwing `DomainException` on invalid input.
- **Rationale**: Keeps invalid `Category`/`Color` instances unrepresentable, consistent with
  every other entity/value object in `Denarius.Domain`. No external validation library (e.g.,
  FluentValidation) is used anywhere in the codebase.
- **Alternatives considered**: Data-annotation attributes on the input DTOs
  (`CreateCategoryInput`/`UpdateCategoryInput`) at the WebAPI boundary — would run before a
  `Category`/`Color` even exists, but was implicitly rejected in favor of one validation source
  of truth in `Domain`.

## Delete protection for in-use categories

- **Decision**: Enforced twice — an explicit `ITransactionRepository.ExistsByCategoryIdAsync`
  check in `DeleteCategoryUseCase` (raises `DomainException`, mapped to `400`), backed by a
  database-level foreign key (`Transactions.CategoryId → Categories.Id`,
  `ON DELETE RESTRICT`) as a safety net.
- **Rationale**: The application-level check produces the user-facing explanation the spec
  requires (FR-009); the database constraint guarantees referential integrity even if a future
  code path bypasses the use case.
- **Alternatives considered**: Relying on the database constraint alone — rejected because a raw
  foreign-key-violation exception would surface as an unhelpful `500` instead of a clean `400`
  with an explanation.

## Error-to-HTTP-status mapping

- **Decision**: A single `GlobalExceptionHandler` (`IExceptionHandler`) maps
  `NotFoundException → 404`, `DomainException`/`AppException → 400`, anything else → `500`, all
  rendered as RFC 7807 `ProblemDetails`.
- **Rationale**: One mapping shared by every controller, including `CategoriesController`, keeps
  error shape consistent across the whole public API (Constitution Principle I).
- **Alternatives considered**: Per-controller try/catch — rejected; the existing shared handler
  already covers this feature's needs with no changes.

## Test coverage gap (flagged, not a blocking finding)

- **Observation**: `Denarius.Domain.Tests` and `Denarius.Application.Tests` fully cover the
  entity and all five use cases; `Denarius.WebAPI.Tests` has no test that exercises
  `CategoriesController` itself — routing, query-parameter binding for `List`, and the `201`
  `Location` header on `Create` are untested above the use-case layer.
- **Impact**: Low. The controller has no branching logic of its own; it only delegates. But a
  routing or binding regression (e.g., a query parameter renamed) would not be caught by any
  test that exists today.
- **Suggested follow-up**: A small `CategoriesController` integration test (via
  `WebApplicationFactory`, matching the pattern already used for `Cors`/`Middleware` in
  `Denarius.WebAPI.Tests`). Out of scope for this plan since it changes no behavior — a
  candidate task for `/speckit-tasks` if the team wants to close the gap.

**Output**: All Technical Context items above are resolved; no `NEEDS CLARIFICATION` remain.
