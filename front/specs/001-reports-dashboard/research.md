# Phase 0 Research: Reports Dashboard

## Chart library and how it is added

- **Decision**: `npx ng add ng2-charts`, which installs `ng2-charts` and `chart.js` and wires
  `provideCharts(withDefaultRegisterables())`. ng2-charts 11.x requires Angular 22; `ng add` picks the
  newest version whose peer dependencies match the installed Angular, so the project gets 10.0.0
  (peer `@angular/core >=21`). Nothing is installed by hand.
- **Rationale**: The request names the library and asks for its schematic.
- **Alternatives considered**: Installing `ng2-charts@11` — fails the peer dependencies of Angular 21.

## Where Chart.js is provided

- **Decision**: Keep the schematic's provider but move it from `app.config.ts` to the `providers` of
  the reports route (`reports.routes.ts`), which `app.routes.ts` already loads lazily. The provider
  also sets Chart.js's default font to the app's Roboto.
- **Rationale**: `withDefaultRegisterables()` imports every Chart.js controller, element, scale and
  plugin (~200 kB). Provided in `app.config.ts`, all of it joins the initial bundle (630 kB today, budget
  warning at 700 kB) and is downloaded by users who never open the reports — the kind of regression the
  budget exists to catch. A route-level provider gives the reports page the same configuration, and
  Chart.js loads with the route.
- **Alternatives considered**: Leaving the provider in `app.config.ts` — passes `ng add` verbatim but
  breaks the budget rule; registering only the needed Chart.js pieces — smaller, but departs from the
  request's `withDefaultRegisterables()` for a chunk that is already lazy.

## Who loads each block's data

- **Decision**: `ReportsPage` owns five `httpResource`s, one per report, driven by its `month` signal.
  Each block component receives its own `Resource` as an input and emits `retry`; the page calls
  `reload()` on that resource alone.
- **Rationale**: The request asks each card to fetch its own data "so that loading and error are
  independent"; the front constitution (Principle II) puts `httpResource` state in pages and keeps
  `components/` presentational, without HTTP. One resource per block keeps every block independent —
  its own request, loading, error and retry — and keeps the boundary. It is also the shape the tables
  already use (`TransactionsTable` takes a `Resource`).
- **Alternatives considered**: An `httpResource` inside each card — what the request literally says,
  but a constitution violation with nothing gained over one resource per block in the page.

## Loading, empty and error states

- **Decision**: A `report-card` component — `mat-card` with `mat-card-title`, then either a
  `mat-progress-spinner` (loading with nothing to show), the error text with a "Tentar novamente"
  `matButton` (error), the empty text (loaded, nothing to show), or the block's content passed as an
  `ng-template`. A reload of a block that has data keeps it on screen at reduced opacity instead of
  replacing it with the spinner. Empty texts: "Sem movimentações neste mês." for the summary and the list
  (the request's text), "Sem despesas neste mês." for the doughnut, "Sem despesas neste mês nem no
  anterior." for the line, and "Sem movimentações neste período." for the bars, which cover twelve months.
- **Rationale**: Five blocks share one state behavior; the template input makes the content render only
  when there is data, so a chart is never created over an empty or failed report. Angular's resource
  clears `value()` when the request changes (a new month → spinner) and keeps it on `reload()` (→ dimmed
  content), checked in `@angular/core`'s resource implementation. A Material spinner instead of a
  hand-made skeleton follows the project's "Material first" rule. The doughnut and the line are about
  expenses, so "no movement" would be false for a month with income only.
- **Alternatives considered**: Repeating the states in each block — five copies of the same markup and
  styles; `@if` around `<ng-content>` — projected content is created even when hidden, so the chart would
  be created over missing data.

## Layout

- **Decision**: The page host takes `height: 100%` of `mat-sidenav-content` (the sidenav container is
  `fullscreen`, so that is the viewport minus the content padding) — the project's equivalent of
  `100dvh`. Below the page header, a CSS grid (`gap: 1rem`) with `grid-template-areas`:

  ```
  "summary    summary  summary  summary"
  "categories bars     bars     transactions"
  "categories line     line     transactions"
  ```

  with columns `repeat(3, minmax(0, 1fr)) minmax(18rem, 1.2fr)` and rows `auto minmax(0, 1fr)
minmax(0, 1fr)`. The five summary cards share the first row; the doughnut and the transactions are tall
  blocks on each side; the two time series take the wide middle. Below 1200 px wide or 600 px tall, a
  media query switches to one column (`summary`, `categories`, `bars`, `line`, `transactions`), gives
  the blocks fixed heights and lets the page scroll.

- **Rationale**: The request's areas, adjusted as it allows: four blocks side by side in the second row
  would leave each time series a quarter of the width, too narrow for twelve months of paired bars or 31
  days of lines; stacking the two series in a double-width column gives them room while the doughnut and
  the list use the full height.
- **Alternatives considered**: The literal four-column second row — cramped series; `100dvh` on the
  page — ignores the header, the padding and the sidenav layout and would overflow.

## Colors

- **Decision**: Every color is a `--mat-sys-*` token. Expense: `error`, in the charts and the list
  (the global `.negative` class already uses it); the summary cards show expenses as negative values
  without color. Income: `tertiary`, the theme's green. Change indicators: "good" in `tertiary`, "bad"
  in `error`, always with ↑/↓ and the percentage in text. The doughnut uses each category's own color,
  "Outras" `outline` (the neutral), and a gap in the card's surface color between slices. Grid lines use
  `surface-container-high`, chart text uses `on-surface`. The canvas can't read CSS variables, so
  `ChartThemeService` resolves these tokens to concrete colors through a probe element, again whenever
  `prefers-color-scheme` changes, and the charts recompute their options from it.
- **Rationale**: The first version had income in `outline`, the pair that passed the dataviz CVD check;
  the owner then chose the theme's green, and its seed was desaturated (`#4CAF50` → `#71A96C`) so it no
  longer outweighs the red in dark mode, where `error` is a pastel tone 80. Green against red fails the
  deuteranopia check (ΔE 7.0 light / 2.8 dark), so the legend, the tooltip and the fixed order of the
  bars carry the identity too. The red in the summary cards drew too much attention, so the owner
  removed it there. Reading the tokens keeps the charts on the theme, in light and dark.
- **Alternatives considered**: Income in `outline` — the validated first version, replaced by the
  owner's green; hex values for a blue income — breaks the "tokens only" rule and drifts from the theme.

## Five slices at most in the doughnut

- **Decision**: `expenses-by-category-chart` folds the API's items to five: past five, the four largest
  keep their slices and the rest — including the API's own "Outras" — add up to a fifth, "Outras", whose
  share is recomputed from the month's `total`. The legend, the tooltip and the `aria-label` use the
  folded list.
- **Rationale**: Asked by the owner before finishing. The API sorts the categories largest first and
  folds past eight with its "Outras" last, so the first four items are always named categories. Folding
  in the front keeps the backend as it is (front tasks don't change `back/`).
- **Alternatives considered**: Lowering the API's limit from eight to five — a backend change for a
  presentation choice.

## Latest transactions and "Ver todas"

- **Decision**: `transactions-list` shows the first ten of the month's transactions (the API sends them
  newest first) and keeps counting all of them in its title. Below the table, a `matButton` link "Ver
  todas" opens `/transactions?month=YYYY-MM` (new `month` input, from the page). The transactions page
  reads `month` from its query string as it already reads `categoryId` (`DateUtils.fromMonthKey`, which
  ignores anything but a valid `YYYY-MM`) and opens with the filters shown on that month.
- **Rationale**: Asked by the owner before finishing. The same link-with-query-string pattern as the
  categories table's transaction count; a link (not router state) survives a reload and can be shared.
- **Alternatives considered**: An API `limit` — a backend change that would lose the month's count.

## Balance line over the bars (dropped)

- **Decision**: None — the bars stay as they were.
- **Rationale**: A line with each month's balance, in the text color over the bars, was built and
  tried; the owner found that it cluttered the chart and asked to remove it.

## Formatting

- **Decision**: Templates use `CurrencyPipe` (`'BRL'`), `DatePipe` (`'dd/MM'` in `UTC` for API dates,
  the same as the transactions table) and `PercentPipe`, under the app's `LOCALE_ID` (`pt-BR`). Chart
  callbacks (tooltips, axes) use `Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })`,
  compact on the value axes. Months are spelled out where they are named — "vs. agosto", legends and
  tooltips such as "setembro de 2026" — and abbreviated only on the bar axis: "set.", with the year on a
  second line under the first month and every January, so the labels need no tilt.
- **Rationale**: The request's pt-BR rules, with the formatting the app already uses.

## Month selector

- **Decision**: The shared `app-month-field` in the page header, not clearable, starting on the current
  month in São Paulo (`DateUtils.currentMonth()`, from `Intl.DateTimeFormat` with `timeZone:
'America/Sao_Paulo'`). The service sends it as `month=YYYY-MM` (`DateUtils.toMonthKey`).
- **Rationale**: Reuses the app's month picker; it shows `MM/yyyy`, which the owner set for every month
  field (commit `40b8e1d`), so the dashboard spells months out in its own content instead. Starting on
  São Paulo's month keeps the first view in line with the API's "current month".
- **Alternatives considered**: Omitting `month` on the first load and letting the API choose — the field
  would show nothing.

## Chart forms and marks

- **Decision**: Following the dataviz method: a KPI row of stat tiles (small label, large value, delta)
  for the summary; a doughnut (as requested; at most eight slices, the API folds the tail into "Outras");
  grouped columns for income vs. expense (bars at most 24 px, 4 px rounded tops, one tooltip per month
  listing both series); and lines (2 px, no point markers except on hover) for the cumulative
  comparison, the current month in the accent (red) and the previous month in the neutral, with an
  index-mode tooltip. Every chart uses Chart.js's own legend and tooltip — the doughnut's labels carry
  each category's share ("Mercado (23,08%)") and its tooltip adds the amount. Grid lines are hairlines,
  axes recessive, `maintainAspectRatio: false` so the chart fills its card. Each canvas gets an
  `aria-label` with the values it draws.
- **Rationale**: Thin marks and quiet chrome, color doing one job per chart, tooltips that never gate a
  value (legend, axes and the `aria-label` carry it too). The native legend instead of an HTML list keeps
  the front's own code to a minimum (the owner asked for it during implementation).
- **Alternatives considered**: An HTML legend listing amount and share beside the doughnut — built first,
  replaced by the native legend: about sixty lines of template, styles and tests for what the legend and
  the tooltip already show.

## Validation findings (rendered in Chrome, 2026-10-03)

- The native doughnut legend needs height: in a one-row block it cut off the last three of eight
  categories, so the doughnut keeps the tall `categories` area (rows 2–3) of the layout above.
- Long single words in a description kept the transactions table from shrinking and pushed the amount out
  of the card at 1366 px; the description column now takes the remaining width and truncates with an
  ellipsis, and the side table uses 8 px cell padding.
- Two-line card titles ("Projeção de despesas") misaligned the summary row; the cards' labels use the
  small title style, so the value is the loud part of each tile.
- At 1366×768 and 1440×900, the page's scroll height equals its client height (no page scroll); at
  1024 px wide the blocks stack and the page scrolls.

## Testing approach

- **Decision**: Model specs per the `write-front-tests` skill: the service with `HttpTestingController`;
  the block components with `resourceFromSnapshots`; the page with `HttpTestingController` and fake
  `MatDialog`-free providers; `app.spec.ts` gains the new link. The chart components are tested through
  the inputs they give to the chart (`type`, `data`, `options`, tooltip callbacks): specs replace
  ng2-charts' `BaseChartDirective` with a fake directive that has the same selector and inputs.
- **Rationale**: jsdom has no canvas; the fake keeps the tests about what the components decide (data,
  colors, formatting), not about Chart.js drawing.
