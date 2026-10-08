---
name: write-front-tests
description: Use when writing, fixing or extending unit tests (`*.spec.ts`) in the Denarius Angular front — services, pages, tables, filters, dialog forms, shared components/directives, utils — or when a front test hangs, flakes or fails in a confusing way (`ResourceValueError`, `whenStable()` never resolving, a select or filter change not reaching the model). Also use when the user asks in Portuguese for "testes do front", "cobrir X com testes", "escrever o spec de Y", "teste travando".
---

# Write Front Tests

## Overview

Angular 21, standalone, **zoneless**, Vitest through `@angular/build:unit-test` with jsdom. `describe`/`it`/`expect`/`vi` are globals (no import). There is no `fakeAsync` (it needs zone.js): time passes for real, and the app settles with `await fixture.whenStable()` / `TestBed.tick()`.

Every spec sits next to its file and has a model in the codebase — find the closest one and copy its setup and helpers; they are the house style:

| What you test | Setup | Model spec |
|---|---|---|
| Pure function / static class | plain `describe`, `it.each` for tables of cases | `shared/date-utils/date-utils.spec.ts` |
| Service with `HttpClient` | `HttpTestingController`, `verify()` after each | `categories/services/categories.service.spec.ts` |
| Service with an `httpResource` | `TestBed.tick()` then `expectOne`, settle with `ApplicationRef.whenStable()` | `categories/services/colors.service.spec.ts` |
| Table taking a `Resource` | `resourceFromSnapshots(signal(...))`, no HTTP | `categories/components/categories-table/categories-table.spec.ts` |
| Component with `model`/inputs, directive | a `Host` component binding it | `shared/sort-menu`, `shared/month-field`, `shared/mat-chip-color` specs |
| Filters (Signal Forms + Material fields) | `Host` with `[(filters)]` + component harnesses | `transactions/components/transactions-filters/transactions-filters.spec.ts` |
| Dialog form | fake `MatDialogRef` + `MAT_DIALOG_DATA` in a `render()` helper | `transactions/components/transaction-form/transaction-form.spec.ts` |
| Page | real `HttpTestingController`, fake `MatDialog` and `MatSnackBar` | `categories/pages/categories-page/categories-page.spec.ts`, `transactions/pages/transactions-page/transactions-page.spec.ts` |
| App shell / router | `provideRouter` with blank components | `app.spec.ts` |

Read `references/recipes.md` for the skeleton of each kind before writing one from scratch.

## How the tests read

- **Test behavior through the DOM**, the way the user sees it: visible text, `aria-label`s, the requests sent, what the dialog closes with, what the snackbar says. Reach into a component instance only for what the DOM can't drive (a `MonthField` value, a filters `model` in a page spec).
- **Names say the behavior**, in English: `should list the categories in the API order`, `should say when the categories fail to load`, `should only offer "%s" while the filter is set, and empty it`. Group with `describe('loading' | 'filters' | 'saving' | 'deleting' | 'creating' | 'editing')`.
- **UI strings are asserted exactly** in pt-BR (`'Nenhuma categoria encontrada.'`, `'Categoria criada.'`), so a typo or wrong gender fails.
- **Data comes from fixtures**: `buildCategory({ id: 'mercado', name: 'Mercado', ... })` from the feature's `testing/` folder, with readable ids. Create the fixture file when a feature lacks one.
- **Small helpers inside the `describe`**: `column(name)`, `noDataRow()`, `rowButton(row, 'Editar' | 'Excluir')`, `headerButton(label)`, `expectList()`, `flushList(items)`, `dialogReturns(result)`, `render(data)`. Reuse the names the existing specs use.
- Specs that render money or dates call `registerLocaleData(localePt)` at the top and provide `{ provide: LOCALE_ID, useValue: 'pt-BR' }`.
- Keep a test to one behavior and assert its outcome; don't snapshot whole templates or assert Material internals.

## Settling the app (zoneless)

- After changing an input, a signal or clicking: `await fixture.whenStable()`.
- `httpResource` requests and effects only run after `TestBed.tick()`. Put it inside an `expectList()` helper so every `expectOne` is preceded by a tick, and pair it with an `expectNoListRequest()` that ticks and asserts no reload.
- **`whenStable()` waits for pending `httpResource` requests.** To see a loading state, flush nothing and call `fixture.detectChanges()` instead; flush the request at the end so `verify()` passes.
- Timers are real: for a 300 ms debounce, `await new Promise((r) => setTimeout(r, 300))`.
- Things rendered in an overlay (menu items, select panels, dialogs) are outside the fixture — query `document`, or use a harness.

## Pitfalls

Each of these cost an investigation; the last one hangs the test without an error.

- **`Resource.value()` throws `ResourceValueError` while the resource is in error.** Components read it only after `hasValue()`; always cover the error state so a missing guard fails a test.
- **Tables take a `Resource`: don't use HTTP for them.** `resourceFromSnapshots(signal<ResourceSnapshot<T[] | undefined>>({ status: 'resolved', value: [...] }))` and switch with `.set({ status: 'loading' | 'error' | 'resolved', ... })`.
- **`mat-select` bound with `[formField]` goes through its ControlValueAccessor**: `triggerEventHandler('valueChange')` does not change the form. Use `MatSelectHarness.clickOptions({ text })`. For `MonthField`, `By.directive(MonthField)` then `componentInstance.value.set(new Date(...))`.
- **Debounced text filters** (`debounce(path.x!, 300)`) update the model only after the pause, or at once on blur (`markAsTouched` flushes). Test both: `MatInputHarness.setValue` + 300 ms; `setValue` + `.blur()`.
- **`expectOne`/`expectNone` with a string compare it with the URL *with* its query string.** For a request with params (the lists always send `orderBy`/`asc`, the reports `month`), use a predicate on `req.url`, or `expectNone` never fails. And call `expectNone` before awaiting stability: after `await fixture.whenStable()`, a pending reload hangs the test and the assertion never runs.
- **In page specs, never drive an action through a harness when it triggers an `httpResource` request**: the harness's `whenStable()` waits on the request and hangs forever. Change filters through the filters component's `model` (`filters()!.filters.set({...})`), then `TestBed.tick()` and `expectOne`.

## What to cover

- **Service**: `list()` params — default, empty filters skipped, sort as `orderBy`/`asc`, all filters together (exact `params.toString()`); each mutation's method, URL, body and response.
- **Table**: rows in API order; formatted money/dates (pt-BR) and `.negative` only below zero; loading spinner, error text, empty text; `edit`/`delete` emit the right row; disabled actions and their tooltip (`injector.get(MatTooltip).message`).
- **Filters**: each field updates the model; text after the pause and on blur; each clear button appears only while set and empties only its filter (`it.each`); clearing a select doesn't open it.
- **Form**: title for new/edit; prefilled values; invalid input doesn't `close` and shows the `mat-error`; valid submit closes with exactly the API input (conversions like sign, decimal comma, UTC dates, blank → `null`); cancel closes without a result.
- **Page**: first request with the default sort; spinner, rows, error + **Recarregar**; filters hidden/shown and reloading with exact params; sort menu reloading; create/update/cancel; API `detail` vs. fallback message on errors, without reloading; delete + **Desfazer** recreating; query params the page reads.

## Run

From `front/`:
```bash
npx ng test --watch=false --include='src/app/categories/**/*.spec.ts'   # while iterating
npx ng test --watch=false                                                # before finishing
npm run lint
npx prettier --write <changed specs>
```
`afterEach(() => httpTesting.verify())` fails a test that left a request unanswered — flush it rather than dropping the `verify()`.

## Common Mistakes

- `await fixture.whenStable()` with an `httpResource` request pending → the test hangs until Vitest's timeout. Use `detectChanges()` for loading states and `TestBed.tick()` + `expectOne` before awaiting.
- Using a harness in a page spec for an action that reloads the list → same hang.
- Forgetting `TestBed.tick()` before `expectOne` → "Expected one matching request, found none".
- Triggering `valueChange` on a `mat-select` and wondering why the model didn't change.
- Asserting on `component.something()` protected state instead of the DOM; mocking the service in page specs (the page specs go through `HttpTestingController` so the real params are checked).
- Changing the app code just to make it testable — if a test seems to need that, look again at how the model spec handles it first.
