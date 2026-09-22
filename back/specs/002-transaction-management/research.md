# Phase 0 Research: Transaction Management

This is a retroactive plan, so "research" means confirming the decisions already embedded in
the shipped code and tests, rather than exploring options for code not yet written. Each entry
below resolves what would otherwise be a `NEEDS CLARIFICATION` in Technical Context.

## Persistence technology

- **Decision**: PostgreSQL via EF Core code-first migrations, using the project-wide
  `DenariusDbContext` — the same store [001-category-management](../001-category-management/research.md)
  uses.
- **Rationale**: Inherited from the project-wide stack, not decided per feature. `Transaction`
  has no requirement that would justify a different store.
- **Alternatives considered**: None specific to `Transaction`.

## List scope (no filter, sort, or pagination)

- **Decision**: `ListTransactionsUseCase` takes no input beyond an ignored `null`/`object?`
  parameter, calls `ITransactionRepository.GetAllAsync()` with no arguments, and returns every
  transaction mapped to `TransactionOutput` with no ordering, filtering, or pagination applied.
  `TransactionsController.List()` takes no query parameters at all.
- **Rationale**: This is simply the current, unextended state of the feature — no product
  requirement for search/filter/sort has been implemented for transactions yet, unlike the
  richer `ListCategoriesUseCase` (name search, in-use filter, three sort fields).
- **Alternatives considered**: N/A — this documents current behavior rather than a deliberate
  design trade-off; there is no evidence in the code or tests of a rejected alternative here.
- **Flagged gap**: Unlike Category, there is no way to narrow the transaction list by category,
  date range, or description via the API today. Any future work adding that would extend this
  feature, not fix a defect in it (the spec's Assumptions section documents this as current
  scope, not a bug).

## Validation placement

- **Decision**: Field-level validation (`Description` trim/length, `Date` not-default, `Value`
  not-zero, `CategoryId` not-empty) lives inside the `Transaction` constructor/`Update` method
  (Domain layer), throwing `DomainException` on invalid input — same pattern as `Category`/`Color`.
  Cross-entity validation (the referenced `Category` must exist) lives one layer up, in
  `CreateTransactionUseCase`/`UpdateTransactionUseCase` (Application layer), because `Transaction`
  itself has no dependency on `ICategoryRepository` and must not (Constitution Principle II —
  `Domain` cannot depend on repository behavior beyond its own interface).
- **Rationale**: Keeps invalid `Transaction` instances unrepresentable for its own fields, while
  keeping the cross-entity existence check where a repository call is actually possible.
- **Alternatives considered**: Checking category existence inside `Transaction` itself — rejected
  because it would require `Domain` to depend on `ICategoryRepository`, violating Principle II.

## Referential integrity with Category

- **Decision**: Enforced twice, in the same shape as Category's own delete-guard research: an
  explicit `ICategoryRepository.GetByIdAsync` existence check in `CreateTransactionUseCase` and
  `UpdateTransactionUseCase` (raises `NotFoundException`, mapped to `404`), backed by a
  database-level foreign key (`Transactions.CategoryId → Categories.Id`, `ON DELETE RESTRICT`)
  as a safety net.
- **Rationale**: The application-level check produces a clean, user-facing `404` when a caller
  references a category that doesn't exist; the database constraint guarantees referential
  integrity even if a future code path bypasses the use case. Note the direction here is the
  mirror image of Category's own guard: Category blocks *deleting* a row that Transactions
  reference; Transaction blocks *creating/updating* a row that references a nonexistent Category.
- **Alternatives considered**: Relying on the database constraint alone — rejected for the same
  reason as in Category: a raw foreign-key-violation exception would surface as an unhelpful
  `500` instead of a clean `404` with an explanation.

## Error-to-HTTP-status mapping

- **Decision**: The same single `GlobalExceptionHandler` (`IExceptionHandler`) used by every
  controller maps `NotFoundException → 404`, `DomainException`/`AppException → 400`, anything
  else → `500`, all rendered as RFC 7807 `ProblemDetails`. No Transaction-specific exception
  handling exists or is needed.
- **Rationale**: One mapping shared across the whole public API (Constitution Principle I).
- **Alternatives considered**: Per-controller try/catch — rejected; unnecessary given the shared
  handler already covers this feature's needs.

## Test coverage gap (flagged, then closed)

- **Observation** (as of the original `/speckit-plan` run): `Denarius.Domain.Tests` (13 tests in
  `TransactionTests.cs`) and `Denarius.Application.Tests` (13 tests across the five
  `UseCases/Transactions/*` test files) fully covered the entity and all five use cases.
  `Denarius.WebAPI.Tests` had no test that exercised `TransactionsController` itself — routing,
  request/response model binding, and the `201`/`Location` header on `Create` were untested above
  the use-case layer.
- **Impact**: Low. The controller has no branching logic of its own; it only delegates. But a
  routing or binding regression (e.g., a field renamed) would not have been caught by any test
  that existed at the time.
- **Resolution**: Closed by `/speckit-implement` on 2026-09-22 — added
  `tests/Denarius.WebAPI.Tests/Transactions/TransactionsControllerTests.cs` (11 tests), following
  the same hand-built `HostBuilder`/`TestServer` pattern already used for
  `Cors`/`Middleware`/`Categories` in `Denarius.WebAPI.Tests` (registers `TransactionsController`
  via `AddApplicationPart` so no database is required), with hand-written fakes for the five use
  case interfaces. This mirrors exactly how
  [001-category-management](../001-category-management/research.md) closed its own equivalent
  gap.

**Output**: All Technical Context items above are resolved; no `NEEDS CLARIFICATION` remain.
