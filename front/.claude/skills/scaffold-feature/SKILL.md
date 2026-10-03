---
name: scaffold-feature
description: Use when adding a new feature/screen to the Denarius Angular front (e.g. Accounts, Budgets, Goals) that lists an API resource in a table with filters, sort and a create/edit dialog — the page + filters + table + form + service + types + routes + sidenav link set that `categories/` and `transactions/` follow. Also use when adding one of those pieces to an existing feature (e.g. "add filters to the X page", "add a form dialog for Y"), or when the user asks in Portuguese for "nova tela", "nova página", "CRUD de X no front", "listagem de X".
---

# Scaffold Feature

## Overview

The front is Angular 21 + Material 3, standalone, zoneless, tested with Vitest + jsdom. Every feature is a lazy-loaded folder with the same files, and `categories/` is the reference the user fixed and asked to replicate; `transactions/` is the same pattern plus a second resource (categories) feeding the table, filters and form. Copy their shape — don't invent a new one.

The user wants the front **as simple as possible** and **leaning on Angular Material**: Material components and `--mat-sys-*` tokens before any custom HTML/CSS/TS, desktop layout only (no responsive/mobile code), fewer files and less indirection. If something looks like it needs a custom component, look for the Material equivalent first (`*matNoDataRow`, `mat-menu`, `matTooltip`, `MatDialog`, `MatSnackBar`...).

Colors come only from `--mat-sys-*` tokens — never a hex value in a feature's SCSS — so they follow the app's palette in light and dark mode. The theme lives in `src/material.scss` (`mat.theme` plus the global `mat.*-overrides`) using `src/material-theme.scss`, which `ng generate @angular/material:theme-color` generated: regenerate it with the CLI rather than editing it. A look that should change for every table, card or button goes in those overrides, not in the feature.

Read before starting (in `src/app/`):
- `categories/pages/categories-page/*`, `categories/components/*/*`, `categories/services/categories.service.ts`, `categories/types/*`, `categories/testing/category-fixture.ts`, `categories/categories.routes.ts`
- `transactions/` counterparts when the feature shows data from another feature (lookups by id, selects fed by another list)
- `app.routes.ts`, `app.ts` (sidenav links), `shared/` (`page-header`, `sort-menu`, `month-field`, `mat-chip-color`, `date-utils`)

## Before Starting

1. **Get the API contract first — it can block everything else.** If the user gave the contract in the request (e.g. the backend lives on another branch), use it as is. Otherwise open `../back/src/Denarius.WebAPI/Controllers/{Feature}Controller.cs` and `../back/src/Denarius.Application/IO/{Feature}/*.cs` (Output, Create/Update inputs, List input, `*OrderField` enum): they give the fields, the list query params, the sort fields and the error behavior.
   - **Never edit `back/`** in a front task — the user reverted that once.
   - If the endpoint or a needed param doesn't exist and no contract was given, stop before writing any front code. Tell the user what's missing, say what the existing API already covers, and propose a contract (routes, fields, list params, sort fields) for them to build with the backend's own `scaffold-entity` / `add-use-case` skills, run from `back/`.
2. **Propose, then confirm** with the user, deriving defaults from the contract:
   - table columns (and which are money, counts, dates)
   - filters (one per list query param; a `dateRef` param becomes the shared month field)
   - sort options (one pair per `*OrderField` value, labelled by what comes first) and the default sort — by name A–Z when there is a name
   - form fields and validation (max lengths come from the backend validation/EF config)
   - delete rules (e.g. `canDelete` disables the button with a tooltip explaining why) and whether undo makes sense
   - sidenav label (pt-BR) and Material Symbols icon; the new link goes last
   
   When something doesn't fit the pattern (a computed column, a month that identifies the record rather than filters it, data only partly in the API), say so in the proposal instead of forcing it into the template.
3. All user-facing text is **pt-BR** (mind gender agreement: "Categoria criada." / "Transação salva." / "Orçamento criado."); code, identifiers and comments are English.

## Files

Names below use `categories` / `category` / `Category` as placeholders — plural folder, singular entity.

```
src/app/{plural}/
├── {plural}.routes.ts                       export const {plural}Routes: Routes (loadComponent of the page)
├── types/{singular}.ts                      interface {Entity} (the API output) + type {Entity}Input = Pick<...>
├── types/{singular}-filters.ts              interface {Entity}Filters, every field optional
├── services/{plural}.service.ts (+ spec)    class {Plural}Service
├── testing/{singular}-fixture.ts            build{Entity}(overrides)
├── pages/{plural}-page/                     class {Plural}Page        app-{plural}-page
├── components/{plural}-filters/             class {Plural}Filters     app-{plural}-filters
├── components/{plural}-table/               class {Plural}Table       app-{plural}-table
└── components/{singular}-form/              class {Entity}Form        app-{singular}-form
```

Each component folder has `.ts`, `.html`, `.scss`, `.spec.ts`. Classes have no `Component` suffix; use `templateUrl`/`styleUrl` (not inline). Formatting is Prettier (tabs, single quotes, width 120) — run it rather than hand-format.

### types

```ts
export interface Category { id: string; name: string; /* ...API output, dates as string */ createdAt: string; updatedAt: string; }
export type CategoryInput = Pick<Category, 'name' | 'color'>;
```

```ts
/** A filter left out or empty is not applied. */
export interface CategoryFilters {
	name?: string;
	withTransaction?: boolean | '';  // '' means "Todas" in a select
	/** The first day of the month. */
	month?: Date | null;
}
```

A field with a fixed set of values (a type, a status) gets its pt-BR labels next to the type, used by table, filters and form:

```ts
export type AccountType = 'checking' | 'savings' | 'wallet';
/** The name of each account type, in the order the selects offer them. */
export const accountTypeLabels: Record<AccountType, string> = { checking: 'Conta corrente', savings: 'Poupança', wallet: 'Carteira' };
export const accountTypes = Object.keys(accountTypeLabels) as AccountType[];
```

### service

`providedIn: 'root'`, `baseUrl = \`${environment.apiUrl}/{plural}\``. `list()` does **not** call HTTP — it returns `{ url, params }` for the page's `httpResource`, skipping empty filters so the API applies its defaults. Month → `dateRef` via `DateUtils.toDateKey`; sort → `orderBy` (the API's camelCase field) + `asc`. `create`/`update`/`delete` return `HttpClient` observables. Copy `categories.service.ts`.

### page

Copy `categories-page.*`. The pieces:
- `filters = signal<{Entity}Filters>({...every key at its empty value})`, `filtersVisible = signal(false)`
- `sort = signal<Sort>(default)`, `sortOptions: SortOption[]` — labels like `'Nome (A–Z)'`, `'Maior saldo primeiro'`
- `{plural} = httpResource<{Entity}[]>(() => this.service.list(this.filters(), this.sort()))` — the resource reloads by itself when filters or sort change
- `openForm(entity?)` opens `MatDialog` with the form and, on a result, calls `save(create|update, message)`
- `delete(entity)` deletes, reloads, and offers **Desfazer** in the snackbar, which re-creates it (the API has no undelete)
- `save()` shows the success snackbar (3000 ms) and reloads; `showError()` shows `error.error?.detail ?? fallback` (5000 ms, `'Fechar'`) — the API's ProblemDetails `detail` is already pt-BR
- template: `<app-page-header text>` projecting reload, show/hide filters, `<app-sort-menu>`, and the `matButton="filled"` "Nova X" button; then `@if (filtersVisible())` a `mat-card.filters` with the filters; then `mat-card.table` with the table
- scss: identical to `categories-page.scss` (the table card scrolls inside the page)

If the feature needs another feature's list (like transactions needs categories), add a second `httpResource` from that feature's service, pass it to the table as a `Resource`, and pass a `computed` plain array (`hasValue() ? value() : []`) to filters and form.

### filters

Signal Forms over a `model`: `filters = model.required<{Entity}Filters>()`, `form = form(this.filters, (path) => debounce(path.{text}!, 300))` — every change reloads the list, so text fields wait for a pause in typing. Bind with `[formField]="form.x!"`. Each `mat-form-field` uses `subscriptSizing="dynamic"`; text fields get a `search` icon prefix; selects start with a `value=""` "Todos/Todas" option; a set filter shows a `matSuffix matIconButton` close button that calls one `clear(event, filter)` method (`event.stopPropagation()` so the select doesn't open). Here `transactions-filters.ts` is the model, not `categories-filters.ts`, which still has one method per filter. Month: `<app-month-field [formField]="form.month!" clearable />`. scss: a grid with `minmax(200px, 3fr)` for the text field and `minmax(200px, 2fr)` for the others.

### table

`{plural} = input.required<Resource<{Entity}[] | undefined>>()`, `edit`/`delete` outputs, `columns` array, and `rows = computed(() => r.hasValue() ? r.value() : [])` — **`value()` throws while the resource is in error**, so always guard with `hasValue()` (keep the resource in a local variable so the type guard narrows). Template: `mat-table` with one `matColumnDef` per column; numbers and money `align="end"` on both `th` and `td`; money `currency: 'BRL'` with `[class.negative]="x < 0"` (global class — there is no `.positive`); API dates `date: 'shortDate' : 'UTC'`; a category (the only thing with a color) as `<mat-chip [appMatChipColor]>`, other values as plain text. Rows in `matCellDef` are untyped, so indexing a `Record` with them fails strict template checking (TS7053) — look labels up in a small method (`typeLabel(account)`). Then an `actions` column with `matIconButton` + `matTooltip` for Editar/Excluir; and a `*matNoDataRow` showing a spinner while loading, "Não foi possível carregar os/as X." on error, "Nenhum(a) X encontrado(a)." when empty. A delete that the API would refuse (`canDelete`) uses `[disabled]` + `disabledInteractive` + a tooltip saying why, guarding the click.

### form

A `MatDialog` component with **Reactive Forms** (not Signal Forms — keep it like the existing forms): `data = inject(MAT_DIALOG_DATA)` (the entity, or `undefined` to create; a `{Entity}FormData` interface only when it needs more, like `TransactionFormData`), `dialogRef: MatDialogRef<{Entity}Form, {Entity}Input>`, a `FormGroup` of `nonNullable` controls prefilled from the entity, and `submit()` that closes with the `{Entity}Input` only when valid. Template: `mat-dialog-title` "Nova X"/"Editar X", `<form id="{singular}-form">` inside `mat-dialog-content`, `mat-error` per validated field, a `{len}/{max}` `mat-hint` on text with a max length (plus `Validators.maxLength`), and `mat-dialog-actions` with "Cancelar" (`mat-dialog-close`) and a `matButton="filled" type="submit" form="{singular}-form"` "Salvar". scss: `form { display: flex; flex-direction: column; width: 400px; }`. Money is typed as text so it takes a decimal comma, like `transaction-form.ts`: `matTextPrefix` "R$", a `Validators.pattern` with up to two decimals, prefilled with `toFixed(2).replace('.', ',')`, parsed back with `replace(',', '.')`.

### wiring

- `app.routes.ts`: `{ path: '{plural}', loadChildren: () => import('./{plural}/{plural}.routes').then((m) => m.{plural}Routes) }` — always lazy, the initial bundle is near its budget.
- `app.ts`: add `{ route: '/{plural}', label: '...', icon: '...' }` at the end of `links`.
- `app.spec.ts`: it asserts the exact list of links, so add the new one there.

## Testing

Write a spec for every file above, modeled on the matching `categories/` or `transactions/` spec. Use the **write-front-tests** skill for them — it has the setup for each kind of spec, what each should cover, and the pitfalls that hang or break tests silently (resources in error, debounced filters, harnesses with a pending `httpResource`).

## Verify

From `front/`:
```bash
npx prettier --write src/app/{plural} src/app/app.routes.ts src/app/app.ts src/app/app.spec.ts
npx ng test --watch=false
npm run lint
npx ng build
```
`ng build` warns at 700 kB initial (it's ~630 kB). A lazy feature shouldn't move the initial total; if it does, something was imported eagerly — find it before touching the budget.

## Checklist

| File | Notes |
|---|---|
| `types/{singular}.ts` | `{Entity}` + `{Entity}Input = Pick<...>` |
| `types/{singular}-filters.ts` | all optional, empty means not applied |
| `services/{plural}.service.ts` + spec | `list()` returns `{ url, params }` |
| `testing/{singular}-fixture.ts` | `build{Entity}(overrides)` |
| `components/{plural}-filters/*` | Signal Forms, debounce text, clear buttons |
| `components/{plural}-table/*` | `Resource` input, `hasValue()` guard, no-data row |
| `components/{singular}-form/*` | dialog, Reactive Forms, closes with the input |
| `pages/{plural}-page/*` | `httpResource`, header actions, snackbars, undo |
| `{plural}.routes.ts` | lazy page |
| `app.routes.ts`, `app.ts`, `app.spec.ts` | lazy route + sidenav link (last) + its test |

## Common Mistakes

- Editing `back/` to make the front work — propose the API change and wait.
- Reading `resource.value()` without `hasValue()` — it throws when the request fails, and the page breaks instead of showing the error row.
- Subscribing to `list()` in the page or keeping list state by hand — the `httpResource` driven by `filters()`/`sort()` already reloads.
- Building custom cards, lists, empty states or CSS where a Material component or `--mat-sys-*` token exists; adding responsive/mobile styles.
- Sending empty filters to the API (`name=`) — skip them in `list()`.
- Writing UI text in English or with the wrong gender; inventing an error message when the API's `detail` is available.
- Importing the feature eagerly (e.g. its types or service from `app.ts`), which grows the initial bundle.
