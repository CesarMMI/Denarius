<!--
Sync Impact Report
Version change: N/A (unratified template scaffold) → 1.0.0
Rationale for bump: Initial ratification of concrete governance content. There is no prior
  ratified version to diff against, so the starting version is 1.0.0 rather than one computed
  via the MAJOR/MINOR/PATCH rules (those apply to amendments of an already-ratified document).

Modified principles: none (initial ratification — no prior ratified version existed)

Core Principles defined:
  - I. Public API Compatibility (new)
  - II. Service Boundary Adherence (new)
  - III. Migration Rollback Discipline (new)
  - IV. Test Suite Verification (new)
  Note: the template offers 5 principle slots; only 4 are defined, matching the 4 directives in
  the user input. The 5th slot was omitted rather than padded with an invented principle.
  The same 4 directives already govern back/.specify/memory/constitution.md (v1.0.0); this
  document applies them to the Angular front and defers to the backend constitution wherever
  a change crosses into back/.

Sections added:
  - Architectural Constraints (template's second section slot; derived from front/src/app,
    angular.json and package.json)
  - Development Workflow (template's third section slot; turns Principles I–IV into
    pre-PR checks)
  - Governance (amendment procedure, semantic versioning policy, compliance review)

Sections removed: none

Deferred / TODO placeholders: none

Templates under .specify/templates/ are not modified by this command. A manual pass is
recommended to confirm they still align with the principles above:
  - plan-template.md (Constitution Check gate)
  - spec-template.md
  - tasks-template.md
  - checklist-template.md
-->

# Denarius Front Constitution

## Core Principles

### I. Public API Compatibility
The front MUST consume the Denarius WebAPI exactly as its current contract defines it — the
routes, query parameters, request/response shapes and status codes served by `back/`, as
documented under `back/specs/*/contracts`. The TypeScript types under each feature's `types/`
folder MUST mirror that contract. A front feature MUST NOT depend on an API change that does not
exist yet; when one is genuinely needed, it MUST be called out in the spec, explicitly approved
by the project owner, and delivered in `back/` under the backend constitution's Principle I
(additive by default, versioned when breaking) before the front relies on it.

The front's own public surface — its URL routes, which users bookmark and share — MUST stay
stable: renaming or removing a route requires a redirect from the old path.

**Rationale**: The WebAPI is the single integration point between the two halves of Denarius.
A front that quietly assumes a different contract breaks at runtime, not at build time, and a
broken bookmark is a regression the user sees first.

### II. Service Boundary Adherence
Work for a front feature MUST stay in `front/`; it MUST NOT modify `back/` unless the change was
explicitly approved (Principle I). Inside `front/src/app`, the existing boundaries MUST be
respected:

- Each feature lives in its own folder (`categories/`, `transactions/`, …) with the
  `pages/`, `components/`, `services/`, `types/` and `testing/` split those features already use.
- Endpoint URLs, query parameters and `HttpClient` calls MUST live only in the feature's
  `services/*.service.ts`. Pages own the `httpResource` state and orchestrate dialogs and
  snackbars; components under `components/` (filters, table, form) are presentational and
  MUST NOT perform HTTP.
- A feature MUST NOT import another feature's internals. Code needed by more than one feature
  moves to `shared/`.
- New features MUST follow the established pattern (page + filters + table + form dialog +
  service + types + routes, lazily routed from `app.routes.ts`) instead of inventing a new one.

**Rationale**: These boundaries keep each feature testable in isolation, keep the HTTP surface in
one auditable place per feature, and keep the codebase small and predictable.

### III. Migration Rollback Discipline
The front holds no database and MUST NOT introduce one. Every database migration a feature
requires MUST be delivered in `back/` (`Denarius.Infrastructure/Migrations`) and MUST include a
rollback plan — a verified `Down()` or a documented manual procedure — as required by the
backend constitution's Principle III. A front feature whose plan depends on a schema change MUST
reference that migration and its rollback plan in `plan.md`, and MUST remain deployable against
the schema both before and after the rollback is applied.

**Rationale**: Denarius stores financial data. A front that only works against the migrated
schema turns a backend rollback into a broken UI; planning for both states keeps recovery quick.

### IV. Test Suite Verification
Before a change is considered complete, the front's established unit and integration suites —
the Vitest specs run by `ng test` (`npm test`), covering services, pages, filters, tables, forms
and shared components — MUST be executed and MUST pass. When a change also touches `back/`, the
backend suites (`dotnet test`) MUST be executed and pass as well. New behavior requires matching
`*.spec.ts` coverage next to the code it tests; existing tests MUST NOT be deleted, skipped or
weakened to force a pass.

**Rationale**: These suites are the only automated guard against regressions in the UI and in its
integration with the API. Bypassing them defeats their purpose.

## Architectural Constraints

The front is an Angular 21 single-page application (standalone components, signals, signal forms,
`httpResource`), styled with Angular Material and the CDK, and tested with Vitest through
`@angular/build:unit-test`. The API base URL comes from `src/environments/`.

- Angular Material is the UI foundation: prefer its components, tokens and theming over custom
  HTML, CSS or TypeScript, and keep custom code to the minimum a feature needs.
- The target is desktop browsers; responsive or mobile layouts are out of scope unless a spec
  says otherwise.
- The production build MUST stay within the budgets in `angular.json` (initial bundle warning at
  700 kB, error at 1 MB; component styles warning at 4 kB, error at 8 kB).
- Code MUST pass `ng lint` (angular-eslint) and follow the repository's Prettier formatting.

## Development Workflow

Before opening a pull request:

- Run `npm test` and confirm every suite passes; add or update the `*.spec.ts` for any behavior
  change (Principle IV). If `back/` was touched, run `dotnet test` too.
- Run `npm run lint` and `npm run build`, and confirm no budget error is reported.
- If the change consumes a new or altered endpoint, link the backend contract it relies on and
  confirm it is already available (Principle I).
- If the change depends on a schema change, link the backend migration and its rollback plan
  (Principle III).

A pull request that breaks a boundary from Principle II — HTTP outside a feature service, a
feature importing another feature's internals, or unapproved changes in `back/` — MUST be
rejected in review and redirected to the right place.

## Governance

This constitution supersedes other front-end practices, informal conventions and prior PR
precedent. Where a change spans both halves of the repository, the backend constitution
(`back/.specify/memory/constitution.md`) governs the `back/` part and this one governs the
`front/` part; neither may be used to waive the other.

Amendments are made by editing this file directly, MUST document their rationale, and MUST
update the version number according to semantic versioning:

- **MAJOR**: Backward-incompatible governance changes — removing a principle, or redefining one
  so that it permits what was previously prohibited.
- **MINOR**: Adding a new principle, or materially expanding an existing one.
- **PATCH**: Wording clarifications, typo fixes, or non-semantic refinements that do not change
  what is required.

Every pull request MUST be reviewed for compliance with the principles above before merge. A
reviewer who finds a violation MUST block the merge until it is resolved, or until an explicit,
documented exception is agreed in the PR description.

**Version**: 1.0.0 | **Ratified**: 2026-10-03 | **Last Amended**: 2026-10-03
