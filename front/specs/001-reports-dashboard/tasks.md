---
description: 'Task list for the Reports Dashboard feature'
---

# Tasks: Reports Dashboard

**Input**: Design documents from `/specs/001-reports-dashboard/`

**Prerequisites**: plan.md (present), spec.md (present), research.md (present),
data-model.md (present), contracts/ (present)

**Tests**: Required by the constitution (Principle IV: a `*.spec.ts` next to every new file) and by
the request ("lint e testes existentes passando"). Within each story the spec is written with the code
it covers, modeled on the `write-front-tests` skill.

**Organization**: Tasks are grouped by user story, in `spec.md`'s priority order (P1–P6).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Different files, no dependency on an incomplete task
- **[Story]**: Which user story this task belongs to (US1–US6)
- File paths are exact, relative to `front/`

## Phase 1: Setup (Shared Infrastructure)

- [x] T001 Run `npx ng test --watch=false`, `npm run lint` and `npm run build` on the untouched front and
      record the baseline (tests, lint, "Initial total").
- [x] T002 Run `npx ng add ng2-charts --skip-confirmation`; check that `package.json` gains `ng2-charts`
      (10.x, the Angular 21 line) and `chart.js` (4.x) and what the schematic added to
      `src/app/app.config.ts`.

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story can be completed before this phase.

- [x] T003 Move `provideCharts(withDefaultRegisterables())` from `src/app/app.config.ts` to the
      `providers` of the page route in `src/app/reports/reports.routes.ts`, with Chart.js's default font
      set to Roboto; lazy-load it from `src/app/app.routes.ts` (`path: 'reports'`, `loadChildren`).
- [x] T004 [P] Types for the five reports — `MonthlySummary`, `PreviousMonthSummary`,
      `ExpensesByCategory`, `CategoryExpense`, `IncomeVsExpense`, `CumulativeExpenseComparison`,
      `AccumulatedExpense`, `MonthlyTransaction` (`type: 'in' | 'out'`) — in
      `src/app/reports/types/report.ts`.
- [x] T005 [P] `DateUtils.toMonthKey(date)` → `YYYY-MM` and `DateUtils.currentMonth()` → day 1 of the
      current month in America/Sao_Paulo, with tests (including 02:00 UTC on October 1 still being
      September) in `src/app/shared/date-utils/date-utils.ts` and `date-utils.spec.ts`.
- [x] T006 `ReportsService` — `summary`, `expensesByCategory`, `incomeVsExpense(month, months?)`,
      `cumulativeExpenses`, `transactions`, each returning `{ url, params }` with `month=YYYY-MM` only when
      a month is given — and its spec in `src/app/reports/services/reports.service.ts` and
      `reports.service.spec.ts` (depends on T005).
- [x] T007 [P] `ChartThemeService` — `colors()` resolving `error`, `outline`, `outline-variant`,
      `on-surface-variant` and `surface-container-low` through a probe element, recomputed when
      `prefers-color-scheme` changes — and its spec in `src/app/reports/services/chart-theme.service.ts`
      and `chart-theme.service.spec.ts`.
- [x] T008 [P] `report-card` — `mat-card` with title (and optional subtitle), spinner while loading
      without data, dimmed content while reloading with data, error text + "Tentar novamente" (`retry`),
      empty text, otherwise the content `ng-template` — and its spec in
      `src/app/reports/components/report-card/`.
- [x] T009 [P] Test helpers: `build*` fixtures for the five reports in
      `src/app/reports/testing/report-fixtures.ts` and a fake `canvas[baseChart]` directive (inputs
      `type`, `data`, `options`) in `src/app/reports/testing/fake-chart.ts`.

**Checkpoint**: The route, data access and the card shell exist.

---

## Phase 3: User Story 1 - See how the current month is going (Priority: P1) 🎯 MVP

**Goal**: "Relatórios" in the menu opens `/reports` on the current month with the five summary cards.

**Independent Test**: quickstart.md step 2.1.

- [x] T010 [US1] `summary-cards` — five `mat-card`s (Saldo, Receitas, Despesas, Taxa de poupança,
      Projeção) with BRL values, expenses and negative balances in red, changes as ↑/↓ + percent +
      "vs. {mês anterior por extenso}" in the good (`tertiary`) or bad (`error`) color by metric, "Sem
      comparação com {mês}" for a null change, "—" for a null savings rate; otherwise one `report-card`
      with the summary's state ("Sem movimentações neste mês." when income and expense are 0) — and its
      spec, in `src/app/reports/components/summary-cards/`.
- [x] T011 [US1] `ReportsPage` — page header "Relatórios" with "Recarregar" and the month field, the five
      `httpResource`s on `month()`, the bento grid (areas, `height: 100%`, single-column media query) with
      `summary-cards` wired — and its spec (first requests with the São Paulo month, spinner, cards, error
      and retry of one block, reload of all) in `src/app/reports/pages/reports-page/`.
- [x] T012 [US1] Add `{ route: '/reports', label: 'Relatórios', icon: 'insights' }` last in `links` in
      `src/app/app.ts`, and the new link to the expectations of `src/app/app.spec.ts`.

**Checkpoint**: The page opens from the menu with the summary — the MVP.

---

## Phase 4: User Story 2 - Look at another month (Priority: P2)

**Goal**: The month field reloads every block; a block's retry reloads only that block.

**Independent Test**: quickstart.md step 2.4 and 2.6.

- [x] T013 [US2] Page spec: picking a month through the month field's model sends `month=YYYY-MM` to the
      five endpoints; `retry` from one block re-requests only its endpoint — in
      `src/app/reports/pages/reports-page/reports-page.spec.ts` (extends T011 as each block is wired).

---

## Phase 5: User Story 3 - See where the money went (Priority: P3)

- [x] T014 [US3] `expenses-by-category-chart` — `report-card` "Despesas por categoria" (subtitle: the
      month's total in BRL), a doughnut with the categories' colors, "Outras" in `outline`, a 2 px gap in the
      card surface color, Chart.js's own legend with each share ("Mercado (23,08%)"), tooltip adding the
      amount in BRL, `aria-label`; "Sem despesas neste mês." when empty — and its spec (data, colors,
      legend, tooltip, states, retry) in
      `src/app/reports/components/expenses-by-category-chart/`; wire it in the page and its spec.

---

## Phase 6: User Story 4 - Compare income and expenses over the months (Priority: P4)

- [x] T015 [US4] `income-vs-expense-chart` — `report-card` "Receitas vs. despesas" (subtitle "12 meses até
      {mês}"), grouped columns (Receitas `outline`, Despesas `error`, at most 24 px, 4 px rounded tops),
      untilted x labels ("set.", the year below the first month and every January), compact BRL y axis,
      index tooltip with the month spelled out and
      both values in BRL, `aria-label`; "Sem movimentações neste período." when every month is zero — and its
      spec in `src/app/reports/components/income-vs-expense-chart/`; wire it in the page and its spec.

---

## Phase 7: User Story 5 - Follow the spending pace (Priority: P5)

- [x] T016 [US5] `cumulative-comparison-chart` — `report-card` "Despesas acumuladas", lines over days 1…max
      of both months (current month `error`, previous `outline`, 2 px, no markers), legend with both months
      spelled out (from the `month` input), compact BRL y axis, index tooltip "Dia N" with both values in
      BRL, `aria-label`; "Sem despesas neste mês nem no anterior." when both series are zero — and its spec
      in `src/app/reports/components/cumulative-comparison-chart/`; wire it in the page and its spec.

---

## Phase 8: User Story 6 - Check the details (Priority: P6)

- [x] T017 [US6] `transactions-list` — `report-card` "Transações do mês" (subtitle: how many), a
      `mat-table` with sticky header and its own scroll: Data (`dd/MM`, UTC), Descrição (with the category
      below), Valor (BRL, `-` and `.negative` for `out`, end-aligned); "Sem movimentações neste mês." when
      empty — and its spec in `src/app/reports/components/transactions-list/`; wire it in the page and its
      spec.

---

## Phase 9: Polish & Cross-Cutting Concerns

- [x] T018 `npx prettier --write` on the changed files; `npx ng test --watch=false`, `npm run lint`,
      `npm run build` — all pass (27 spec files, 199 tests), no budget warning; "Initial total" 633.45 kB
      against 630.52 kB in T001, from the esbuild re-split of shared Angular/Material chunks, with no chart
      or reports code in the initial graph (checked with `--stats-json`).
- [x] T019 quickstart.md step 2 against the backend's scratch database: render the page (desktop and
      narrow) and check the numbers, colors, states and layout; fix what the screenshots show. Rendered in
      headless Chrome through the DevTools protocol at 1440×900 (light and dark), 1366×768 and 1024 px
      wide, for October (current), September (eight categories) and March (no movement): numbers match the
      backend quickstart, no page scroll on desktop, single column below 1200 px. Fixed: the doughnut back
      in the tall area (its legend cut off categories in a one-row block), the transactions table
      overflowing at 1366 px (description now truncates), two-line summary labels (small label style),
      tilted axis labels.
- [x] T020 [P] Mark this task list complete and record what the validation found (research.md →
      Validation findings). "Initial total": 630.52 kB before, 633.45 kB after (no budget warning; no
      Chart.js, ng2-charts or reports code in the initial chunks — 800.33 kB with the schematic's
      provider left in `app.config.ts`).

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)** → stories in priority order → **Polish**.
- US1 creates the page; US2 extends the page spec; US3–US6 each add one block to the page (template and
  spec), so they touch `reports-page.*` one at a time even though their components are independent.
- T004, T005, T007, T008 and T009 touch different files and can be written together.

## Parallel Example: Foundational

```bash
Task: "Types in src/app/reports/types/report.ts"
Task: "DateUtils.toMonthKey/currentMonth in src/app/shared/date-utils/"
Task: "ChartThemeService in src/app/reports/services/chart-theme.service.ts"
Task: "report-card in src/app/reports/components/report-card/"
Task: "Fixtures and fake chart in src/app/reports/testing/"
```

## Implementation Strategy

MVP = Setup + Foundational + US1 (the page with the summary cards from the menu). Then each story adds a
block, testable on its own, and Polish runs the full suites, the build budget and the visual check.

## Phase 10: Convergence

The owner's adjustments before finishing (spec.md → Clarifications, Session 2026-10-03). A balance line
over the bars was built and dropped at the owner's request; the bars are unchanged.

- [x] T021 Fold the doughnut to at most five slices — the four largest and "Outras" with the rest, its
      share recomputed from `total`, in the neutral color (`ChartColors.neutral`, `outline`) — with specs in
      `src/app/reports/components/expenses-by-category-chart/` and `src/app/reports/services/chart-theme.service.*`
      per FR-004, US3/AC2 (partial)
- [x] T022 Add `DateUtils.fromMonthKey` and read `?month=YYYY-MM` in `TransactionsPage`, opening the filters on
      that month, with specs in `src/app/shared/date-utils/` and `src/app/transactions/pages/transactions-page/`
      per FR-013, US6/AC3 (missing)
- [x] T023 List the ten newest transactions under a title with the month's count, and link "Ver todas" to
      `/transactions?month=YYYY-MM` (new `month` input, wired in the page), with specs in
      `src/app/reports/components/transactions-list/` and the page per FR-007, US6/AC1, US6/AC3 (partial)
- [x] T024 Cover the owner's "Mês anterior" / "Próximo mês" — `DateUtils.previousMonth`/`nextMonth` and one
      request per block from the page header — in `date-utils.spec.ts` and `reports-page.spec.ts` per FR-002,
      US2/AC3, Constitution IV (missing)
- [x] T025 Align the specs that drifted from the owner's later changes, asserting the new behavior: menu
      order (Relatórios first), expenses as negative values without red in the summary cards, no card
      subtitles, `count: 5` value ticks, the doughnut's 4 px gap, chart colors (`tertiary` income, `primary`
      and `on-primary` lines, `on-surface` text) per Constitution IV (contradicts)
- [x] T026 `npx prettier --write` on the changed files; `npx ng test --watch=false` (27 files, 223 tests),
      `npm run lint` and `npm run build` pass — "Initial total" 633.08 kB, no budget warning (polish)
