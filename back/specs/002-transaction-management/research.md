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

## List filtering and sorting (User Story 3, added 2026-09-24)

Until 2026-09-24 the list had no filter, sort, or pagination: `ListTransactionsUseCase` took an
ignored `object?` input and returned every transaction in whatever order the database produced.
User Story 3 replaced that with the decisions below.

### Where filtering and sorting happen

- **Decision**: `ListTransactionsUseCase` loads every transaction via the unchanged
  `ITransactionRepository.GetAllAsync()` and applies all filters and the sort in memory.
  `ITransactionRepository` is not changed.
- **Rationale**: Matches the established `ListCategoriesUseCase` pattern (which already loads
  every transaction per request for its aggregates), keeps all the new logic in
  `Denarius.Application` where `Denarius.Application.Tests` covers it (Constitution Principle
  IV — there is no Infrastructure test project to cover repository query logic), and costs no
  more than the previous unfiltered list at this feature's stated scale.
- **Alternatives considered**: Pushing the filters into the repository as an EF Core query
  (`Where`/`OrderBy` translated to SQL) — better once volumes grow, but it would move the logic
  into an untested layer and widen `ITransactionRepository` with filter parameters the Domain
  layer would have to express without Application's enums. Deferred until the scale requires it.

### Description search

- **Decision**: Partial, case-insensitive match (`OrdinalIgnoreCase`) on the trimmed search
  text; blank/whitespace search text means no filter; transactions with no description never
  match.
- **Rationale**: Case-insensitive is what the front-end's filter sheet already does locally, and
  transaction descriptions are free text where the user rarely remembers the casing.
- **Alternatives considered**: Mirroring Category's name search exactly — rejected because that
  one runs as SQL `Contains` in PostgreSQL and is therefore case-sensitive; not changed here, but
  noted as a known inconsistency between the two lists.

### Month filter (`dateRef`)

- **Decision**: Same semantics as `ListCategoriesUseCase`: any date within a month selects that
  full calendar month, from its first moment through its last tick, inclusive.
- **Rationale**: One meaning of "month" across the API; the front-end already sends
  `YYYY-MM-01` for Categories and can reuse that.
- **Alternatives considered**: An explicit `from`/`to` date range — more flexible, but no
  current screen needs it, and it would diverge from Categories.

### Type filter

- **Decision**: `TransactionType` enum `All` (default), `In` (value > 0), `Out` (value < 0),
  bound case-insensitively from the query string.
- **Rationale**: "In"/"Out" follow the user-facing wording ("Entradas"/"Saídas") and the
  front-end's existing `all`/`in`/`out` values, which bind to it directly. Zero is impossible
  (the entity rejects it), so the two signs cover every transaction.
- **Alternatives considered**: A nullable `bool?` like Category's `withTransaction` — rejected
  because a three-state choice reads more clearly as an enum with an explicit `All`.

### Sorting

- **Decision**: `TransactionOrderField` enum `Date` (default), `Description`, `Value`,
  `CategoryName`, with `asc` defaulting to `false` (date descending, most recent first).
  `Value` sorts by the signed value. `CategoryName` loads categories via
  `ICategoryRepository.GetAllAsync(null)` — only when that sort is requested — and sorts by a
  CategoryId → name lookup.
- **Rationale**: Most-recent-first is the natural default for a transaction history and matches
  what the front-end mock already shows. Loading category names only for that one sort avoids
  an extra query on every other request.
- **Alternatives considered**: Defaulting `asc` to `true` for consistency with Categories —
  rejected because it would list the oldest transaction first. Adding a `Category` navigation
  property plus a SQL join, or adding `categoryName` to `TransactionOutput` — both larger
  changes (entity/mapping, or response shape) not needed to satisfy the sort.

### Invalid query parameters

- **Decision**: No custom validation — an unknown `type`/`orderBy` value or a malformed
  `categoryId` fails model binding, and `[ApiController]` returns `400` `ValidationProblem`
  before the use case runs.
- **Rationale**: Framework behavior already gives a clear, consistent error; Categories relies on
  the same thing for `orderBy`.
- **Alternatives considered**: Silently falling back to defaults — rejected because it would hide
  client bugs.

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
