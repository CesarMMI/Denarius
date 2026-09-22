---

description: "Task list for the Transaction Management feature"
---

# Tasks: Transaction Management

**Input**: Design documents from `/specs/002-transaction-management/`

**Prerequisites**: plan.md (present), spec.md (present), research.md (present),
data-model.md (present), contracts/ (present)

**Status**: This is a **retroactive** task list — Transaction Management is already implemented.
`[x]` marks a task already satisfied by existing code/tests (with its file path, so the mapping
from requirement to implementation is traceable). `/speckit-implement` ran on 2026-09-22 and
closed the two items that were still outstanding (T025, T026) — all 26 tasks are now `[x]`.

**Tests**: Test tasks below reflect the automated tests that already exist per story; no new
test tasks were added beyond the one explicit coverage gap (T025), which `research.md` already
flagged rather than a blanket "add more tests" addition.

**Organization**: Tasks are grouped by user story, per `spec.md`'s priorities (P1/P2).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Different files, no dependency on an incomplete task
- **[Story]**: Which user story this task belongs to (US1, US2)
- File paths are exact, relative to the repository root

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization. Shared across every backend feature, not created for
Transaction specifically — included here only so this list matches the required phase structure.

- [x] T001 Confirm the four-project Clean Architecture solution exists per `Denarius.slnx`
      (`Denarius.Domain`, `Denarius.Application`, `Denarius.Infrastructure`, `Denarius.WebAPI`,
      with matching `*.Tests` projects) — pre-existing, shared with
      [001-category-management](../001-category-management/tasks.md) T001.
- [x] T002 [P] Confirm EF Core + `Npgsql.EntityFrameworkCore.PostgreSQL` (in
      `src/Denarius.Infrastructure/Denarius.Infrastructure.csproj`) and xUnit + NSubstitute /
      `Microsoft.AspNetCore.TestHost` (in the three `tests/*.csproj`) package references are in
      place — pre-existing, shared with 001-category-management T002.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The entity, persistence, and controller scaffolding the user story below depends on.

**⚠️ CRITICAL**: No user story can work without this phase.

- [x] T003 [P] `Transaction` entity — `Description`: "optional; trimmed; blank/whitespace
      normalizes to null; up to 255 characters after trimming, else throws DomainException";
      `Date`: "required; `default(DateTime)` throws DomainException"; `Value`: "required; nonzero
      decimal, positive or negative; `0` throws DomainException"; `CategoryId`: "required;
      `Guid.Empty` throws DomainException" — in `src/Denarius.Domain/Entities/Transaction.cs`.
- [x] T004 [P] `ITransactionRepository` (extends `IRepository<Transaction>`, adds
      `GetAllAsync()` and `ExistsByCategoryIdAsync(Guid categoryId)`) in
      `src/Denarius.Domain/Repositories/ITransactionRepository.cs` (depends on T003).
- [x] T005 [P] `TransactionConfiguration` EF mapping — `Description`
      `character varying(255)` nullable, `Date` `timestamp with time zone` not null, `Value`
      `numeric(18,2)` not null, `CategoryId` `uuid` not null with FK
      `FK_Transactions_Categories_CategoryId` `ON DELETE RESTRICT` and an index on `CategoryId`
      — in `src/Denarius.Infrastructure/Persistence/Configurations/TransactionConfiguration.cs`
      (depends on T003; FK target depends on
      [001-category-management](../001-category-management/tasks.md) T003/T006).
- [x] T006 [P] `TransactionRepository.GetAllAsync()` and
      `TransactionRepository.ExistsByCategoryIdAsync(Guid)` in
      `src/Denarius.Infrastructure/Repositories/TransactionRepository.cs` (depends on T004).
- [x] T007 `Transactions` table migration with a clean, reversible `Down()` (`DropTable`),
      including the `Categories` FK and `IX_Transactions_CategoryId` index, in
      `src/Denarius.Infrastructure/Migrations/20260814132713_AddTransaction.cs` (depends on
      T005) — satisfies Constitution Principle III.
- [x] T008 Register `ITransactionRepository → TransactionRepository` in
      `src/Denarius.Infrastructure/DependencyInjection.cs` (depends on T006).
- [x] T009 Scaffold `TransactionsController` reusing the shared `GlobalExceptionHandler` mapping
      `NotFoundException → 404`, `DomainException`/`AppException → 400` in
      `src/Denarius.WebAPI/Controllers/TransactionsController.cs` (depends on T008; handler
      shared with 001-category-management T011).

**Checkpoint**: Foundation ready — the user story below builds on this.

---

## Phase 3: User Story 1 - Record and maintain individual transactions (Priority: P1) 🎯 MVP

**Goal**: Create, view, edit, and delete transactions, each referencing an existing category.

**Independent Test**: Create a transaction against an existing category, confirm it appears with
the chosen date/value/category/description, edit it, delete it — no other transactions required.

### Tests for User Story 1 (already exist)

- [x] T010 [P] [US1] `Transaction` validation/trim/update tests (description
      normalization/length, date/value/categoryId rejection, negative-value acceptance) in
      `tests/Denarius.Domain.Tests/Entities/TransactionTests.cs`.
- [x] T011 [P] [US1] `CreateTransactionUseCase` tests — including the category-not-found guard —
      in
      `tests/Denarius.Application.Tests/UseCases/Transactions/Create/CreateTransactionUseCaseTests.cs`.
- [x] T012 [P] [US1] `UpdateTransactionUseCase` tests — including the transaction-not-found and
      category-not-found guards — in
      `tests/Denarius.Application.Tests/UseCases/Transactions/Update/UpdateTransactionUseCaseTests.cs`.
- [x] T013 [P] [US1] `DeleteTransactionUseCase` tests — not-found guard, unconditional delete
      otherwise — in
      `tests/Denarius.Application.Tests/UseCases/Transactions/Delete/DeleteTransactionUseCaseTests.cs`.
- [x] T014 [P] [US1] `GetTransactionByIdUseCase` tests in
      `tests/Denarius.Application.Tests/UseCases/Transactions/GetById/GetTransactionByIdUseCaseTests.cs`.

### Implementation for User Story 1

- [x] T015 [P] [US1] `CreateTransactionInput`/`UpdateTransactionInput` DTOs (`description`,
      `date`, `value`, `categoryId`) in
      `src/Denarius.Application/IO/Transactions/CreateTransactionInput.cs` and
      `UpdateTransactionInput.cs`.
- [x] T016 [P] [US1] `TransactionOutput` DTO in
      `src/Denarius.Application/IO/Transactions/TransactionOutput.cs`.
- [x] T017 [US1] `ICreateTransactionUseCase`/`CreateTransactionUseCase` — verifies the referenced
      `Category` exists via `ICategoryRepository.GetByIdAsync` before constructing the
      `Transaction` — in `src/Denarius.Application/UseCases/Transactions/Create/` (depends on
      T015, T016).
- [x] T018 [US1] `IUpdateTransactionUseCase`/`UpdateTransactionUseCase` — 404 via
      `NotFoundException` when the transaction doesn't exist, and again when the referenced
      category doesn't exist — in `src/Denarius.Application/UseCases/Transactions/Update/`
      (depends on T015, T016).
- [x] T019 [US1] `IDeleteTransactionUseCase`/`DeleteTransactionUseCase` — 404 when missing,
      otherwise deletes unconditionally (no dependent-data guard, unlike
      [001-category-management](../001-category-management/tasks.md) T021's delete-guard) — in
      `src/Denarius.Application/UseCases/Transactions/Delete/` (depends on T016).
- [x] T020 [US1] `IGetTransactionByIdUseCase`/`GetTransactionByIdUseCase` — 404 when missing —
      in `src/Denarius.Application/UseCases/Transactions/GetById/` (depends on T016).
- [x] T021 [US1] Wire `POST`, `PUT {id}`, `DELETE {id}`, `GET {id}` actions in
      `src/Denarius.WebAPI/Controllers/TransactionsController.cs` (depends on T017-T020).

**Checkpoint**: User Story 1 fully functional and independently testable.

---

## Phase 4: User Story 2 - Review recorded transactions (Priority: P2)

**Goal**: View every recorded transaction in one list.

**Independent Test**: Record several transactions; confirm every one appears when the
transaction list is viewed, and that the list is empty when none have been recorded.

### Tests for User Story 2 (already exist)

- [x] T022 [US2] `ListTransactionsUseCase` tests
      (`Execute_TransactionsExist_ReturnsAllTransactionsMappedToOutput`,
      `Execute_NoTransactions_ReturnsEmpty`) in
      `tests/Denarius.Application.Tests/UseCases/Transactions/List/ListTransactionsUseCaseTests.cs`.

### Implementation for User Story 2

- [x] T023 [US2] `IListTransactionsUseCase`/`ListTransactionsUseCase` — calls
      `ITransactionRepository.GetAllAsync()` with no filter/sort/pagination and maps every result
      to `TransactionOutput` — in
      `src/Denarius.Application/UseCases/Transactions/List/ListTransactionsUseCase.cs` (depends
      on T016, T006).
- [x] T024 [US2] Wire `GET /api/transactions` (no query parameters) in
      `src/Denarius.WebAPI/Controllers/TransactionsController.cs` (depends on T023).

**Checkpoint**: Both user stories functional independently.

---

## Phase 5: Polish & Cross-Cutting Concerns

- [x] T025 [P] Add `TransactionsController` integration tests covering `POST` (201 + `Location`
      header), `GET`/`GET {id}`, `PUT {id}`, `DELETE {id}`, and the 404/400 mappings (missing
      transaction, missing category, zero value) — in
      `tests/Denarius.WebAPI.Tests/Transactions/TransactionsControllerTests.cs` (11 tests).
      Follows the existing hand-built `HostBuilder`/`TestServer` pattern from
      `tests/Denarius.WebAPI.Tests/Categories/CategoriesControllerTests.cs` (registers the
      controller via `AddApplicationPart` instead of the full `Program` composition root, so no
      database is needed) with hand-written fakes for the five use case interfaces — this test
      project uses no mocking library. Closes the gap flagged in `research.md` → Test coverage
      gap.
- [x] T026 Ran the `quickstart.md` validation end-to-end on 2026-09-22 against .NET SDK
      10.0.401: `dotnet test` — all three suites, 101/101 passing (Domain.Tests 35,
      Application.Tests 34, WebAPI.Tests 32 including the 11 new T025 tests) — then the full
      manual curl smoke test (steps 1-10) against `dotnet run --project src/Denarius.WebAPI` on
      the local dev database, including the description-blanking, delete, not-found,
      zero-value/missing-date validation, and unknown-category referential steps. Every response
      matched `quickstart.md`'s documented expectations; the category/transaction created for
      the manual run were deleted afterward.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — pre-existing, shared.
- **Foundational (Phase 2)**: Depends on Setup — blocks the user story. Pre-existing; also
  depends on [001-category-management](../001-category-management/tasks.md)'s `Category`
  entity/table existing (T003, T006 there) for the FK target.
- **User Stories (Phase 3-4)**: Both depend on Foundational. Built in priority order (P1 → P2)
  historically; each remains independently testable today.
- **Polish (Phase 5)**: Depends on the user stories it covers. Complete.

### User Story Dependencies

- **User Story 1 (P1)**: No dependency on US2. Depends on
  [001-category-management](../001-category-management/tasks.md) US1 (a category must already
  exist to attach a transaction to).
- **User Story 2 (P2)**: Builds on the `List` plumbing US1 doesn't touch (US1 never calls
  `ListTransactionsUseCase`); independently testable on its own.

### Parallel Opportunities

- T003/T004/T005/T006 (Foundational): different files, T004 depends only on T003, T005/T006
  depend only on T003-T005 respectively.
- T010-T014 (US1 tests): five different files.
- T015/T016 (US1 DTOs): different files.
- T025 has no dependency on T026 and vice versa (same as
  001-category-management's T032/T033 relationship).

---

## Parallel Example: User Story 1

```bash
# The five US1 test files have no dependency on each other:
Task: "Transaction validation/trim/update tests in tests/Denarius.Domain.Tests/Entities/TransactionTests.cs"
Task: "CreateTransactionUseCase tests in tests/Denarius.Application.Tests/UseCases/Transactions/Create/CreateTransactionUseCaseTests.cs"
Task: "UpdateTransactionUseCase tests in tests/Denarius.Application.Tests/UseCases/Transactions/Update/UpdateTransactionUseCaseTests.cs"
Task: "DeleteTransactionUseCase tests in tests/Denarius.Application.Tests/UseCases/Transactions/Delete/DeleteTransactionUseCaseTests.cs"
Task: "GetTransactionByIdUseCase tests in tests/Denarius.Application.Tests/UseCases/Transactions/GetById/GetTransactionByIdUseCaseTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

This is how the feature was actually delivered: Setup + Foundational, then US1 (CRUD) shipped as
a usable increment before the full list view (US2) existed.

### Incremental Delivery (as it happened)

1. Setup + Foundational → `Transaction`, persistence, empty controller (shipped alongside
   [001-category-management](../001-category-management/tasks.md)'s own foundation, same
   commit range).
2. US1 → transaction CRUD with the category-existence guard — deployable on its own.
3. US2 → unfiltered list view added in the same change.
4. Polish (T025/T026) → closed by `/speckit-implement` on 2026-09-22, mirroring how
   001-category-management closed the equivalent gap.

### What's Left

Nothing. Phase 5 (the `TransactionsController` HTTP-layer test gap and a recorded quickstart
run) was closed by `/speckit-implement` on 2026-09-22.

---

## Notes

- `[x]` = implemented and (where applicable) tested; all 26 tasks are now complete.
- File paths are exact — this list doubles as a requirement-to-code traceability map for
  `spec.md`'s FR-001…FR-012.
