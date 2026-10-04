# Implementation Plan: Reports Dashboard

**Branch**: `001-reports-dashboard` | **Date**: 2026-10-03 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-reports-dashboard/spec.md`

## Summary

A new lazily loaded `reports/` feature with the folders the other features use. `ReportsService` has
one method per backend report, each returning the `{ url, params }` request that an `httpResource`
takes. `ReportsPage` owns the month signal and the five `httpResource`s, and lays the blocks out in a
CSS grid that fills the height of the sidenav content. Each block is a presentational component that
receives its own `Resource` and emits `retry`: `summary-cards`, `expenses-by-category-chart`,
`income-vs-expense-chart`, `cumulative-comparison-chart` and `transactions-list`. A small `report-card`
gives them the same card, title and loading / empty / error-with-retry states. Charts use ng2-charts
(Chart.js), added with `ng add ng2-charts` and provided on the lazy route, so the initial bundle stays
put. Chart colors come from the Material theme tokens, resolved for the color scheme in use: expense
`error`, income `tertiary` (the theme's green), "Outras" `outline` — see research → Colors.

## Technical Context

**Language/Version**: TypeScript 5.9 / Angular 21.2 (standalone, zoneless, signals)

**Primary Dependencies**: Angular Material / CDK 21.2; **ng2-charts 10.0.0** + **chart.js 4.x**,
added by `ng add ng2-charts` — the latest ng2-charts compatible with Angular 21, since 11.x requires
Angular 22

**Storage**: N/A (reads the backend reports API)

**Testing**: Vitest + jsdom through `@angular/build:unit-test`; `HttpTestingController` for the service
and the page; `resourceFromSnapshots` for the block components; a fake `canvas[baseChart]` directive
in the specs, so Chart.js never draws in jsdom

**Target Platform**: Desktop browsers (the narrow-screen collapse is the one responsive behavior the
spec asks for)

**Project Type**: web — frontend half of a two-project web application; this plan covers `front/`

**Performance Goals**: The initial bundle stays at its current size (~630 kB, budget warning at
700 kB): Chart.js only loads with the reports route (SC-005)

**Constraints**: `.specify/memory/constitution.md` (see Constitution Check); Material components and
`--mat-sys-*` tokens first; component styles under the 4 kB budget; `ng lint` and Prettier

**Scale/Scope**: One route, one page, six presentational components, one API service, one chart-theme
service, the types for five reports

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

| Principle                          | Status | Evidence                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| ---------------------------------- | ------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| I. Public API Compatibility        | PASS   | Consumes `GET /api/reports/{summary,expensesByCategory,incomeVsExpense,cumulativeExpenses,transactions}` exactly as `back/specs/003-financial-reports/contracts/reports-api.yaml` defines them; `types/report.ts` mirrors that contract. The API change was requested by the owner in this same task and is delivered in `back/` first, under the backend constitution (additive). The new `/reports` route renames nothing.                                                                                                                                                      |
| II. Service Boundary Adherence     | PASS   | Everything lives in `front/src/app/reports/` with `pages/ components/ services/ types/ testing/`. URLs and params only in `services/reports.service.ts`; `ReportsPage` owns the five `httpResource`s; the block components are presentational (a `Resource` input and a `retry` output, no HTTP). Shared pieces come from `shared/` (`page-header`, `month-field`, `date-utils`); no other feature's internals are imported. The feature is read-only, so of the established pattern it uses page + components + service + types + routes and has no filters, form or CRUD table. |
| III. Migration Rollback Discipline | PASS   | No schema dependency: the backend's only migration (`AddTransactionDateIndex`, with a verified `Down()`) adds an index, and the page works the same before and after it.                                                                                                                                                                                                                                                                                                                                                                                                          |
| IV. Test Suite Verification        | PASS   | A `*.spec.ts` next to every new file (service, page, six components, theme service, `DateUtils` additions) and the updated `app.spec.ts`; `npm test`, `npm run lint` and `npm run build` must pass, and `dotnet test` for the `back/` half.                                                                                                                                                                                                                                                                                                                                       |
| Architectural constraints          | PASS   | Material cards, spinner, buttons, table and icons, `--mat-sys-*` tokens for every color (resolved to concrete colors for the canvas). The single-column collapse is in scope because the spec asks for it (FR-011). Chart.js stays out of the initial bundle (see research → Where Chart.js is provided).                                                                                                                                                                                                                                                                         |

No violations — Complexity Tracking is not needed.

_Re-checked after Phase 1 design: unchanged._

## Project Structure

### Documentation (this feature)

```text
specs/001-reports-dashboard/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
│   └── reports-ui.md
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
src/app/
├── app.routes.ts                     # + lazy /reports
├── app.ts / app.spec.ts              # + "Relatórios" sidenav link (last)
├── shared/date-utils/                # + toMonthKey, currentMonth (São Paulo)
└── reports/
    ├── reports.routes.ts             # page + provideCharts(withDefaultRegisterables())
    ├── types/report.ts               # the five report shapes
    ├── services/
    │   ├── reports.service.ts        # one request per endpoint
    │   └── chart-theme.service.ts    # theme colors for the canvas, per color scheme
    ├── testing/
    │   ├── report-fixtures.ts        # build* fixtures
    │   └── fake-chart.ts             # stands in for canvas[baseChart] in specs
    ├── pages/reports-page/           # month + 5 httpResources + bento grid
    └── components/
        ├── report-card/              # card, title, loading/empty/error states
        ├── summary-cards/
        ├── expenses-by-category-chart/
        ├── income-vs-expense-chart/
        ├── cumulative-comparison-chart/
        └── transactions-list/
```

**Structure Decision**: A feature folder like `categories/` and `transactions/`, lazily routed from
`app.routes.ts`. Specs sit next to their files.

## Complexity Tracking

Not applicable — the Constitution Check reported no violations.
