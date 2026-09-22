<!--
Sync Impact Report
Version change: N/A (unratified template scaffold) → 1.0.0
Rationale for bump: Initial ratification of concrete governance content. There is no prior
  ratified version to diff against, so the starting version is set to 1.0.0 rather than computed
  via the MAJOR/MINOR/PATCH rules (those apply to amendments of an already-ratified document).

Modified principles: none (initial ratification — no prior ratified version existed)

Core Principles defined:
  - I. Public API Compatibility (new)
  - II. Service Boundary Adherence (new)
  - III. Migration Rollback Discipline (new)
  - IV. Test Suite Verification (new)
  Note: the template offers 5 principle slots; only 4 are defined here, matching the 4 distinct
  directives supplied in the user input for this command. The 5th slot was omitted rather than
  padded with an invented principle.

Sections added:
  - Architectural Constraints (fills the template's second section slot; derived from repo
    structure — .csproj project references and the Infrastructure/Migrations folder)
  - Development Workflow (fills the template's third section slot; operationalizes Principles
    I, III and IV as pull-request checklist items)
  - Governance (amendment procedure, semantic versioning policy, compliance review expectations)

Sections removed: none

Deferred / TODO placeholders: none

Templates under .specify/templates/ are not modified by this command (constitution updates are
scoped to this file only). A manual pass is recommended to confirm they still align with the
principles above:
  - plan-template.md
  - spec-template.md
  - tasks-template.md
  - checklist-template.md
-->

# Denarius Backend Constitution

## Core Principles

### I. Public API Compatibility
Public contracts exposed by `Denarius.WebAPI` — controller routes, request/response DTOs,
status codes, and error contracts — MUST remain backward compatible for existing consumers.
Breaking changes (removed or renamed endpoints, altered request/response shapes, changed
status-code semantics) MUST NOT ship without an explicit versioning strategy and a documented
migration path for consumers. Additive, backward-compatible changes (new optional fields, new
endpoints) do not require a version bump.

**Rationale**: The WebAPI is the sole integration point for the Denarius front-end and any
external clients. An uncoordinated breaking change silently breaks consumers who have no
visibility into server-side changes ahead of a deploy.

### II. Service Boundary Adherence
The existing layering — `Denarius.Domain` → `Denarius.Application` → `Denarius.Infrastructure`
→ `Denarius.WebAPI` — MUST be respected for every change. `Denarius.Domain` MUST NOT take a
dependency on any other project. `Denarius.Application` MAY depend only on `Denarius.Domain`
and MUST express infrastructure needs (persistence, external I/O) as interfaces rather than
concrete implementations. `Denarius.Infrastructure` implements those interfaces and MUST NOT be
referenced by `Denarius.Domain` or `Denarius.Application`. `Denarius.WebAPI` composes the other
layers via dependency injection and MUST NOT contain business rules that belong in `Domain` or
`Application`. Code MUST be placed in the layer matching its responsibility; shortcuts that skip
a layer (for example, a controller querying `Infrastructure` directly) are prohibited.

**Rationale**: The layered boundaries keep business logic independently testable and keep
infrastructure swappable. Skipping them reintroduces the tangled dependencies this architecture
exists to prevent.

### III. Migration Rollback Discipline
Every Entity Framework Core migration added under `Denarius.Infrastructure/Migrations` MUST
ship with a working, verified rollback path: a correct `Down()` method that reverses the
matching `Up()`, or — when a clean `Down()` cannot fully reverse the change (for example, a
destructive data transformation) — an explicit, documented manual rollback procedure alongside
the migration. A migration MUST NOT be merged while its rollback path is unverified.

**Rationale**: Denarius persists financial data. A migration without a reviewed rollback turns a
bad deploy into a data-loss incident instead of a quick recovery.

### IV. Test Suite Verification
Before a change is considered complete, the repository's established unit and integration test
suites — `Denarius.Domain.Tests`, `Denarius.Application.Tests`, and `Denarius.WebAPI.Tests`,
run via `dotnet test` — MUST be executed and MUST pass. New behavior requires corresponding
coverage in the matching test project; existing tests MUST NOT be deleted, skipped, or weakened
to force a pass.

**Rationale**: These suites are the project's only automated guardrail against regressions
across the layered architecture and the public API surface. Bypassing them defeats their
purpose.

## Architectural Constraints

Denarius Backend targets .NET 10 (`net10.0`) and follows Clean Architecture across four
projects with a strict, one-directional dependency graph:

- `Denarius.Domain` — entities, value objects, and repository interfaces. No project or
  third-party dependencies.
- `Denarius.Application` — use cases and IO contracts. Depends only on `Denarius.Domain`.
- `Denarius.Infrastructure` — EF Core persistence (PostgreSQL via Npgsql), repository
  implementations, and database migrations. Depends on `Denarius.Domain` and
  `Denarius.Application`.
- `Denarius.WebAPI` — controllers, middleware, CORS policy, and the composition root. Depends
  on `Denarius.Application` and `Denarius.Infrastructure`.

Persistence is Entity Framework Core, code-first, against PostgreSQL; schema changes are
expressed as migrations under `Denarius.Infrastructure/Migrations`.

Each source project has a matching xUnit test project (`Denarius.Domain.Tests`,
`Denarius.Application.Tests`, `Denarius.WebAPI.Tests`); `Denarius.WebAPI.Tests` additionally
uses `Microsoft.AspNetCore.TestHost` for integration-style tests against the composed
application.

## Development Workflow

Before opening a pull request:

- Run `dotnet test` and confirm all three test suites pass; add or update tests in the matching
  project for any behavior change (Principle IV).
- If the change touches `Denarius.WebAPI/Controllers` or its DTOs, state explicitly in the PR
  description whether it is additive or breaking, and how a breaking change is versioned
  (Principle I).
- If the change adds a file under `Denarius.Infrastructure/Migrations`, confirm the `Down()`
  method (or the documented manual rollback) was executed and verified locally before
  requesting review (Principle III).

A pull request that skips a layer defined in Architectural Constraints — for example, a
controller referencing `Denarius.Infrastructure` types directly, or `Denarius.Domain` taking on
a new dependency — MUST be rejected in review and redirected to the correct layer (Principle
II).

## Governance

This constitution supersedes all other backend development practices, informal conventions, and
prior PR precedent. Where another document or past practice conflicts with a principle here,
this constitution governs.

Amendments are made by editing this file directly, MUST document their rationale, and MUST
update the version number according to semantic versioning:

- **MAJOR**: Backward-incompatible governance changes — removing a principle, or redefining one
  so that it permits what was previously prohibited.
- **MINOR**: Adding a new principle, or materially expanding an existing one.
- **PATCH**: Wording clarifications, typo fixes, or non-semantic refinements that do not change
  what is required.

Every pull request MUST be reviewed for compliance with the principles above before merge. A
reviewer who finds a violation MUST block the merge until it is resolved or an explicit,
documented exception is agreed with the change's author in the PR description.

**Version**: 1.0.0 | **Ratified**: 2026-09-21 | **Last Amended**: 2026-09-21
