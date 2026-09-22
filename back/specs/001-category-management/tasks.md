---

description: "Task list for the Category Management feature"
---

# Tasks: Category Management

**Input**: Design documents from `/specs/001-category-management/`

**Prerequisites**: plan.md (present), spec.md (present), research.md (present),
data-model.md (present), contracts/ (present)

**Status**: This is a **retroactive** task list — Category Management is already implemented.
`[x]` marks a task already satisfied by existing code/tests (with its file path, so the mapping
from requirement to implementation is traceable); `[ ]` marks the two genuinely outstanding
items, both already flagged in `research.md`. `/speckit-implement` run against this file should
only act on the unchecked tasks.

**Tests**: Test tasks below reflect the automated tests that already exist per story; no new
test tasks were added beyond the one explicit coverage gap (T032), which `research.md` already
flagged rather than a blanket "add more tests" addition.

**Organization**: Tasks are grouped by user story, per `spec.md`'s priorities (P1/P2/P3).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Different files, no dependency on an incomplete task
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- File paths are exact, relative to the repository root

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization. Shared across every backend feature, not created for
Category specifically — included here only so this list matches the required phase structure.

- [x] T001 Confirm the four-project Clean Architecture solution exists per `Denarius.slnx`
      (`Denarius.Domain`, `Denarius.Application`, `Denarius.Infrastructure`, `Denarius.WebAPI`,
      with matching `*.Tests` projects) — pre-existing.
- [x] T002 [P] Confirm EF Core + `Npgsql.EntityFrameworkCore.PostgreSQL` (in
      `src/Denarius.Infrastructure/Denarius.Infrastructure.csproj`) and xUnit + NSubstitute /
      `Microsoft.AspNetCore.TestHost` (in the three `tests/*.csproj`) package references are in
      place — pre-existing.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The entity, persistence, and controller scaffolding every user story below depends
on.

**⚠️ CRITICAL**: No user story can work without this phase.

- [x] T003 [P] `Category` entity — `Name`: "required; trimmed; 1-100 characters after trimming;
      throws DomainException otherwise" — in `src/Denarius.Domain/Entities/Category.cs`.
- [x] T004 [P] `Color` value object — "normalized to `#RRGGBB` uppercase; accepts input with or
      without a leading `#`, and 3-digit shorthand; anything not matching `^#[0-9A-F]{6}$` after
      normalization throws DomainException" — in `src/Denarius.Domain/ValueObjects/Color.cs`.
- [x] T005 `ICategoryRepository` (extends `IRepository<Category>`, adds
      `GetAllAsync(string? name)`) in `src/Denarius.Domain/Repositories/ICategoryRepository.cs`
      (depends on T003).
- [x] T006 [P] `CategoryConfiguration` EF mapping — `Name` `character varying(100)` not null,
      `Color` `character varying(7)` not null via value converter — in
      `src/Denarius.Infrastructure/Persistence/Configurations/CategoryConfiguration.cs`
      (depends on T003, T004).
- [x] T007 [P] `CategoryRepository.GetAllAsync(string? name)` (`Name.Contains(name)` filter) in
      `src/Denarius.Infrastructure/Repositories/CategoryRepository.cs` (depends on T005).
- [x] T008 `Categories` table migration with a clean, reversible `Down()` (`DropTable`) in
      `src/Denarius.Infrastructure/Migrations/20260813225006_InitialCreate.cs` (depends on
      T006) — satisfies Constitution Principle III.
- [x] T009 Restrict-on-delete FK `Transactions.CategoryId → Categories.Id` in
      `src/Denarius.Infrastructure/Persistence/Configurations/TransactionConfiguration.cs` and
      `src/Denarius.Infrastructure/Migrations/20260814132713_AddTransaction.cs` (depends on
      T008) — database-level backstop for FR-009.
- [x] T010 Register `ICategoryRepository → CategoryRepository` in
      `src/Denarius.Infrastructure/DependencyInjection.cs` (depends on T007).
- [x] T011 Scaffold `CategoriesController` with `GlobalExceptionHandler` mapping
      `NotFoundException → 404`, `DomainException`/`AppException → 400` in
      `src/Denarius.WebAPI/Controllers/CategoriesController.cs` and
      `src/Denarius.WebAPI/Middleware/GlobalExceptionHandler.cs` (depends on T010).

**Checkpoint**: Foundation ready — all three user stories build on this.

---

## Phase 3: User Story 1 - Build and maintain a category list (Priority: P1) 🎯 MVP

**Goal**: Create, view, rename/recolor, and delete categories; block deleting a category that
still has transactions.

**Independent Test**: Create a category, confirm it appears with the chosen name/color, edit it,
delete it — no transaction data required.

### Tests for User Story 1 (already exist)

- [x] T012 [P] [US1] `Category` validation/trim/update tests in
      `tests/Denarius.Domain.Tests/Entities/CategoryTests.cs`.
- [x] T013 [P] [US1] `CreateCategoryUseCase` tests in
      `tests/Denarius.Application.Tests/UseCases/Categories/Create/CreateCategoryUseCaseTests.cs`.
- [x] T014 [P] [US1] `UpdateCategoryUseCase` tests in
      `tests/Denarius.Application.Tests/UseCases/Categories/Update/UpdateCategoryUseCaseTests.cs`.
- [x] T015 [P] [US1] `DeleteCategoryUseCase` tests — not-found and has-transactions guard — in
      `tests/Denarius.Application.Tests/UseCases/Categories/Delete/DeleteCategoryUseCaseTests.cs`.
- [x] T016 [P] [US1] `GetCategoryByIdUseCase` tests in
      `tests/Denarius.Application.Tests/UseCases/Categories/GetById/GetCategoryByIdUseCaseTests.cs`.

### Implementation for User Story 1

- [x] T017 [P] [US1] `CreateCategoryInput`/`UpdateCategoryInput` DTOs (`name`, `color`) in
      `src/Denarius.Application/IO/Categories/CreateCategoryInput.cs` and
      `UpdateCategoryInput.cs`.
- [x] T018 [P] [US1] `CategoryOutput` DTO in
      `src/Denarius.Application/IO/Categories/CategoryOutput.cs`.
- [x] T019 [US1] `ICreateCategoryUseCase`/`CreateCategoryUseCase` in
      `src/Denarius.Application/UseCases/Categories/Create/` (depends on T017, T018).
- [x] T020 [US1] `IUpdateCategoryUseCase`/`UpdateCategoryUseCase` — 404 via `NotFoundException`
      when the category doesn't exist — in `src/Denarius.Application/UseCases/Categories/Update/`
      (depends on T017, T018).
- [x] T021 [US1] `IDeleteCategoryUseCase`/`DeleteCategoryUseCase` — "MUST refuse to delete a
      category that has one or more transactions associated with it" via
      `ITransactionRepository.ExistsByCategoryIdAsync` — in
      `src/Denarius.Application/UseCases/Categories/Delete/` (depends on T018).
- [x] T022 [US1] `IGetCategoryByIdUseCase`/`GetCategoryByIdUseCase` — 404 when missing — in
      `src/Denarius.Application/UseCases/Categories/GetById/` (depends on T018).
- [x] T023 [US1] Wire `POST`, `PUT {id}`, `DELETE {id}`, `GET {id}` actions in
      `src/Denarius.WebAPI/Controllers/CategoriesController.cs` (depends on T019-T022).

**Checkpoint**: User Story 1 fully functional and independently testable.

---

## Phase 4: User Story 2 - See how much each category is used (Priority: P2)

**Goal**: Each category reports its transaction count and balance, optionally scoped to one
calendar month.

**Independent Test**: Record a known set of transactions against a category; confirm the list
reports the correct count/balance for it and zero/zero for an unused one.

### Tests for User Story 2 (already exist)

- [x] T024 [US2] `ListCategoriesUseCase` aggregation and month-scoping tests
      (`Execute_CalculatesTransactionCountAndBalancePerCategory`,
      `Execute_WithDateRef_OnlyConsidersTransactionsWithinTheMonth`) in
      `tests/Denarius.Application.Tests/UseCases/Categories/List/ListCategoriesUseCaseTests.cs`.

### Implementation for User Story 2

- [x] T025 [US2] `TransactionCount`/`Balance` fields on `CategoryOutput` in
      `src/Denarius.Application/IO/Categories/CategoryOutput.cs`.
- [x] T026 [US2] `ListCategoriesInput.DateRef` + `IListCategoriesUseCase`/`ListCategoriesUseCase`
      aggregation (`GroupBy(t => t.CategoryId)`, month range derived from `dateRef`) in
      `src/Denarius.Application/IO/Categories/ListCategoriesInput.cs` and
      `src/Denarius.Application/UseCases/Categories/List/ListCategoriesUseCase.cs` (depends on
      T025, T007).
- [x] T027 [US2] Wire `GET /api/categories` with the `dateRef` query parameter in
      `src/Denarius.WebAPI/Controllers/CategoriesController.cs` (depends on T026).

**Checkpoint**: User Stories 1 and 2 both functional independently.

---

## Phase 5: User Story 3 - Narrow down a long category list (Priority: P3)

**Goal**: Search by name, filter by usage, sort by name/count/balance.

**Independent Test**: Create several categories with varying names/usage; confirm search, the
in-use/not-in-use filter, and each sort option each narrow or reorder the list on their own.

### Tests for User Story 3 (already exist)

- [x] T028 [US3] `ListCategoriesUseCase` tests for the name filter, the `withTransaction` filter,
      and each `orderBy`/`asc` combination in
      `tests/Denarius.Application.Tests/UseCases/Categories/List/ListCategoriesUseCaseTests.cs`.

### Implementation for User Story 3

- [x] T029 [US3] `CategoryOrderField` enum (`Name`, `TransactionCount`, `Balance`) in
      `src/Denarius.Application/IO/Categories/CategoryOrderField.cs`.
- [x] T030 [US3] `withTransaction` filter and `orderBy`/`asc` sort switch in
      `src/Denarius.Application/UseCases/Categories/List/ListCategoriesUseCase.cs` (depends on
      T026, T029).
- [x] T031 [US3] Wire `name`, `withTransaction`, `orderBy`, `asc` query parameters on
      `GET /api/categories` in `src/Denarius.WebAPI/Controllers/CategoriesController.cs`
      (depends on T030).

**Checkpoint**: All three user stories functional independently.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T032 [P] Add `CategoriesController` integration tests via `WebApplicationFactory` —
      covering `POST` (201 + `Location` header), `GET`/`GET {id}`, `PUT {id}`, `DELETE {id}`, and
      the 404/400 mappings — in a new `tests/Denarius.WebAPI.Tests/Categories/` folder, following
      the existing pattern in `tests/Denarius.WebAPI.Tests/Cors/` and `.../Middleware/`. Closes
      the gap flagged in `research.md` → Test coverage gap; not required for the feature to work,
      only for the controller layer to be regression-tested.
- [ ] T033 Run the `quickstart.md` validation end-to-end (filtered `dotnet test` runs, then the
      manual curl smoke test) and record the result.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — pre-existing.
- **Foundational (Phase 2)**: Depends on Setup — blocks all user stories. Pre-existing.
- **User Stories (Phase 3-5)**: All depend on Foundational. Built in priority order (P1 → P2 →
  P3) historically; each remains independently testable today.
- **Polish (Phase 6)**: Depends on the user stories it covers. Still open.

### User Story Dependencies

- **User Story 1 (P1)**: No dependency on US2/US3.
- **User Story 2 (P2)**: Builds on the `List` plumbing US1 doesn't touch (US1 never calls
  `ListCategoriesUseCase`); independently testable without US3's filters/sort.
- **User Story 3 (P3)**: Extends the same `ListCategoriesUseCase` as US2 (T026 → T030); depends
  on US2's `ListCategoriesInput`/`ListCategoriesUseCase` existing, but its own filter/sort
  behavior is independently testable.

### Parallel Opportunities

- T003/T004 (Foundational): different files, no shared dependency.
- T006/T007 (Foundational): different files, both depend only on T003-T005.
- T012-T016 (US1 tests): five different files.
- T017/T018 (US1 DTOs): different files.
- T032 (Polish) has no dependency on T033 and vice versa.

---

## Parallel Example: User Story 1

```bash
# The five US1 test files have no dependency on each other:
Task: "Category validation/trim/update tests in tests/Denarius.Domain.Tests/Entities/CategoryTests.cs"
Task: "CreateCategoryUseCase tests in tests/Denarius.Application.Tests/UseCases/Categories/Create/CreateCategoryUseCaseTests.cs"
Task: "UpdateCategoryUseCase tests in tests/Denarius.Application.Tests/UseCases/Categories/Update/UpdateCategoryUseCaseTests.cs"
Task: "DeleteCategoryUseCase tests in tests/Denarius.Application.Tests/UseCases/Categories/Delete/DeleteCategoryUseCaseTests.cs"
Task: "GetCategoryByIdUseCase tests in tests/Denarius.Application.Tests/UseCases/Categories/GetById/GetCategoryByIdUseCaseTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

This is how the feature was actually delivered: Setup + Foundational, then US1 (CRUD) shipped as
a usable increment before usage statistics (US2) or search/sort (US3) existed.

### Incremental Delivery (as it happened)

1. Setup + Foundational → `Category`/`Color`, persistence, empty controller.
2. US1 → basic category CRUD with the delete guard — deployable on its own.
3. US2 → transaction count/balance joined in (`feat(api): add category transaction count,
   balance, filters and sorting`).
4. US3 → search, in-use filter, and sorting added in the same change as US2.

### What's Left

Only Phase 6: the `CategoriesController` HTTP-layer test gap (T032) and a recorded quickstart
run (T033). Everything else is shipped and covered.

---

## Notes

- `[x]` = already implemented and (where applicable) tested; `[ ]` = genuinely outstanding.
- File paths are exact — this list doubles as a requirement-to-code traceability map for
  `spec.md`'s FR-001…FR-015.
- If `/speckit-implement` runs against this file, it should only touch T032 and T033.
