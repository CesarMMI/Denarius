# Spec recipes

Skeletons distilled from the existing specs. `{Entity}`/`{plural}` are placeholders (`Category`/`categories`). Open the model spec named in each section for the full version.

## Contents
- [Service with HttpClient](#service-with-httpclient)
- [Service with an httpResource](#service-with-an-httpresource)
- [Table taking a Resource](#table-taking-a-resource)
- [Component or directive through a Host](#component-or-directive-through-a-host)
- [Filters](#filters)
- [Dialog form](#dialog-form)
- [Page](#page)
- [App shell / router](#app-shell--router)

## Service with HttpClient

Model: `categories/services/categories.service.spec.ts`.

```ts
describe('{Plural}Service', () => {
	const baseUrl = `${environment.apiUrl}/{plural}`;
	let service: {Plural}Service;
	let httpTesting: HttpTestingController;

	beforeEach(() => {
		TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
		service = TestBed.inject({Plural}Service);
		httpTesting = TestBed.inject(HttpTestingController);
	});

	afterEach(() => httpTesting.verify());

	describe('list', () => {
		it('should target the {plural} endpoint without params by default', () => {
			const { url, params } = service.list();
			expect(url).toBe(baseUrl);
			expect(params.keys()).toEqual([]);
		});
		// empty filters skipped; sort → 'orderBy=x&asc=false'; every filter together → exact params.toString()
	});

	it('create should POST the input', () => {
		let response: unknown;
		service.create(input).subscribe((value) => (response = value));
		const req = httpTesting.expectOne(baseUrl);
		expect(req.request.method).toBe('POST');
		expect(req.request.body).toEqual(input);
		req.flush(entity, { status: 201, statusText: 'Created' });
		expect(response).toEqual(entity);
	});
	// update → PUT `${baseUrl}/${id}`; delete → DELETE, flush(null, { status: 204, ... }), assert `complete`
});
```

## Service with an httpResource

Model: `categories/services/colors.service.spec.ts`. The resource issues its request on `TestBed.tick()`; after flushing, settle with `TestBed.inject(ApplicationRef).whenStable()`.

```ts
beforeEach(() => {
	TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
	service = TestBed.inject(ColorsService);
	httpTesting = TestBed.inject(HttpTestingController);
	TestBed.tick();
});

async function respond(body: string[] | 'error') {
	const req = httpTesting.expectOne(URL);
	if (body === 'error') req.flush('', { status: 404, statusText: 'Not Found' });
	else req.flush(body);
	await TestBed.inject(ApplicationRef).whenStable();
}
// reload: service.reload(); TestBed.tick(); await respond(...)
```

## Table taking a Resource

Model: `categories/components/categories-table/categories-table.spec.ts` (two resources: `transactions-table.spec.ts`).

```ts
registerLocaleData(localePt);

describe('{Plural}Table', () => {
	let fixture: ComponentFixture<{Plural}Table>;
	let element: HTMLElement;
	let items: WritableSignal<ResourceSnapshot<{Entity}[] | undefined>>;

	beforeEach(async () => {
		TestBed.configureTestingModule({ providers: [{ provide: LOCALE_ID, useValue: 'pt-BR' }, provideRouter([])] });
		items = signal({ status: 'resolved', value: [a, b, c] });
		fixture = TestBed.createComponent({Plural}Table);
		element = fixture.nativeElement;
		fixture.componentRef.setInput('{plural}', resourceFromSnapshots(items));
		await fixture.whenStable();
	});

	function column(name: string) {
		return Array.from(element.querySelectorAll(`tr[mat-row] .mat-column-${name}`)).map((cell) => cell.textContent?.trim());
	}
	function noDataRow() {
		return element.querySelector('td[colspan]')?.textContent?.trim();
	}
	function rowButton(row: number, label: 'Editar' | 'Excluir') {
		return element.querySelectorAll<HTMLButtonElement>(`tr[mat-row] button[aria-label="${label}"]`)[row];
	}

	it('should show a spinner while loading', async () => {
		items.set({ status: 'loading', value: undefined });
		await fixture.whenStable();
		expect(element.querySelector('td[colspan] mat-progress-spinner')).not.toBeNull();
	});

	it('should say when the {plural} fail to load', async () => {
		items.set({ status: 'error', error: new Error('Server Error') });
		await fixture.whenStable();
		expect(column('name')).toEqual([]);
		expect(noDataRow()).toBe('Não foi possível carregar as ...');
	});

	it('should emit the row to edit or delete', () => {
		const edit = vi.fn();
		fixture.componentInstance.edit.subscribe(edit);
		rowButton(2, 'Editar').click();
		expect(edit).toHaveBeenCalledWith(c);
	});
});
```

Chip colors: `fixture.debugElement.queryAll(By.directive(MatChipColor)).map((chip) => chip.injector.get(MatChipColor).color())`. Tooltips: `debugElement.queryAll(By.directive(MatTooltip)).find((el) => el.nativeElement === button)!.injector.get(MatTooltip).message`. Disabled-but-interactive buttons: `getAttribute('aria-disabled')`.

## Component or directive through a Host

Models: `shared/sort-menu/sort-menu.spec.ts`, `shared/month-field/month-field.spec.ts`, `shared/mat-chip-color/mat-chip-color.spec.ts`, `shared/page-header/page-header.spec.ts`.

A `Host` binds the component as a parent would, so two-way `model`s, attribute inputs and content projection are tested for real; assert on the host's signals.

```ts
@Component({
	imports: [SortMenu],
	template: `<app-sort-menu [options]="options" [(sort)]="sort" />`,
})
class Host {
	readonly options: SortOption[] = [/* ... */];
	readonly sort = signal<Sort>({ active: 'name', direction: 'asc' });
}

/** The menu opens in an overlay, outside the host. */
function items() {
	return Array.from(document.querySelectorAll<HTMLButtonElement>('.mat-mdc-menu-panel [mat-menu-item]'));
}
```

Spy on a Material child through its instance: `vi.spyOn(fixture.debugElement.query(By.directive(MatDatepicker)).componentInstance, 'open').mockImplementation(() => undefined)`.

## Filters

Model: `transactions/components/transactions-filters/transactions-filters.spec.ts`.

```ts
@Component({
	imports: [{Plural}Filters],
	template: `<app-{plural}-filters [(filters)]="filters" />`,
})
class Host {
	readonly filters = signal<{Entity}Filters>({ name: '', type: '', month: null });
}

beforeEach(async () => {
	fixture = TestBed.createComponent(Host);
	host = fixture.componentInstance;
	loader = TestbedHarnessEnvironment.loader(fixture);
	await fixture.whenStable();
});

function field(label: string) {
	return loader.getHarness(MatFormFieldHarness.with({ floatingLabelText: label }));
}
async function select(label: string) {
	return (await (await field(label)).getControl(MatSelectHarness))!;
}
async function input(label: string) {
	return (await (await field(label)).getControl(MatInputHarness))!;
}
function clearButton(label: string) {
	return loader.getHarnessOrNull(MatButtonHarness.with({ selector: `[aria-label="${label}"]` }));
}

it('should filter by the name after a pause in the typing', async () => {
	await (await input('Nome')).setValue('mer');
	expect(host.filters().name).toBe('');
	await new Promise((resolve) => setTimeout(resolve, 300));
	expect(host.filters().name).toBe('mer');
});

it.each([
	['Limpar nome', 'name'],
	['Limpar tipo', 'type'],
] as const)('should only offer "%s" while the filter is set, and empty it', async (label, filter) => {
	expect(await clearButton(label)).toBeNull();
	const filters: {Entity}Filters = { name: 'mer', type: 'x', month: null };
	host.filters.set(filters);
	await fixture.whenStable();
	await (await clearButton(label))!.click();
	expect(host.filters()).toEqual({ ...filters, [filter]: '' });
	expect(await clearButton(label)).toBeNull();
});
```

Month: `fixture.debugElement.query(By.directive(MonthField)).componentInstance.value.set(new Date(2026, 8, 1)); await fixture.whenStable();`.

## Dialog form

Models: `categories/components/category-form/category-form.spec.ts`, `transactions/components/transaction-form/transaction-form.spec.ts`.

```ts
let close: ReturnType<typeof vi.fn>;

async function render(data: {Entity} | undefined) {
	close = vi.fn();
	TestBed.configureTestingModule({
		providers: [
			{ provide: MatDialogRef, useValue: { close } },
			{ provide: MAT_DIALOG_DATA, useValue: data },
		],
	});
	fixture = TestBed.createComponent({Entity}Form);
	element = fixture.nativeElement;
	await fixture.whenStable();
}

function input(name: string) {
	return element.querySelector<HTMLInputElement>(`input[formControlName="${name}"]`)!;
}
async function type(name: string, value: string) {
	input(name).value = value;
	input(name).dispatchEvent(new Event('input'));
	await fixture.whenStable();
}
async function save() {
	element.querySelector('form')!.dispatchEvent(new Event('submit'));
	await fixture.whenStable();
}

describe('creating', () => {
	beforeEach(() => render(undefined));
	it('should not save without a name', async () => {
		await save();
		expect(close).not.toHaveBeenCalled();
		expect(element.querySelector('mat-error')?.textContent).toBe('Informe um nome');
	});
});
// editing: render(entity) in the test or a beforeEach; title 'Editar ...'; prefilled; close called with the exact input
// cancel: click button[mat-dialog-close]; expect(close).toHaveBeenCalledOnce(); expect(close.mock.calls[0][0]).toBeFalsy()
```

A form that loads something itself (like the category colors from `data/default-colors.json`) also gets `provideHttpClient(), provideHttpClientTesting()` and, after creating the component, `TestBed.tick()` + `expectOne(...).flush(...)`.

## Page

Models: `categories/pages/categories-page/categories-page.spec.ts`, `transactions/pages/transactions-page/transactions-page.spec.ts` (second resource, query params through a fake `ActivatedRoute`).

```ts
registerLocaleData(localePt);

let dialog: { open: ReturnType<typeof vi.fn> };
let snackBar: { open: ReturnType<typeof vi.fn> };
let snackBarAction: Subject<void>;

beforeEach(() => {
	dialog = { open: vi.fn() };
	snackBarAction = new Subject<void>();
	snackBar = { open: vi.fn().mockReturnValue({ onAction: () => snackBarAction }) };
	TestBed.configureTestingModule({
		providers: [
			{ provide: LOCALE_ID, useValue: 'pt-BR' },
			provideRouter([]),
			provideHttpClient(),
			provideHttpClientTesting(),
			{ provide: MatDialog, useValue: dialog },
			{ provide: MatSnackBar, useValue: snackBar },
		],
	});
	httpTesting = TestBed.inject(HttpTestingController);
	fixture = TestBed.createComponent({Plural}Page);
	element = fixture.nativeElement;
});

afterEach(() => httpTesting.verify());

function expectList(): TestRequest {
	TestBed.tick();
	return httpTesting.expectOne((req) => req.method === 'GET' && req.url === baseUrl);
}
function expectNoListRequest() {
	TestBed.tick();
	httpTesting.expectNone((req) => req.method === 'GET' && req.url === baseUrl);
}
async function flushList(items: {Entity}[]) {
	expectList().flush(items);
	await fixture.whenStable();
}
function headerButton(label: string) {
	return element.querySelector<HTMLButtonElement>(`app-page-header button[aria-label="${label}"]`);
}
function filters(): {Plural}Filters | undefined {
	return fixture.debugElement.query(By.directive({Plural}Filters))?.componentInstance;
}
function dialogReturns(result: unknown) {
	dialog.open.mockReturnValue({ afterClosed: () => of(result) });
}

it('should show a spinner while loading', () => {
	const req = expectList();
	fixture.detectChanges(); // not whenStable(): it would wait for the pending request
	expect(element.querySelector('mat-progress-spinner')).not.toBeNull();
	req.flush([]);
});

it('should reload with the filters chosen', async () => {
	await flushList([a, b]);
	headerButton('Exibir filtros')!.click();
	await fixture.whenStable();
	filters()!.filters.set({ name: 'mer', month: new Date(2026, 8, 1) }); // not a harness: it would hang
	expect(expectList().request.params.toString()).toBe('name=mer&dateRef=2026-09-01&orderBy=name&asc=true');
});

it('should show the API error detail and not reload when saving fails', () => {
	dialogReturns(input);
	button('Nova ...').click();
	httpTesting.expectOne(baseUrl).flush({ status: 409, detail: 'Já existe ...' }, { status: 409, statusText: 'Conflict' });
	expect(snackBar.open).toHaveBeenCalledWith('Já existe ...', 'Fechar', { duration: 5000 });
	expectNoListRequest();
});
// undo: after the DELETE and its reload, snackBarAction.next(), then expect the POST that recreates it
// sort: click 'app-sort-menu button', await whenStable(), click the item in document '.mat-mdc-menu-panel [mat-menu-item]'
```

## App shell / router

Model: `app.spec.ts`. Register each route with a blank component so links resolve and `routerLinkActive` works; navigate with `TestBed.inject(Router).navigateByUrl(...)` then `await fixture.whenStable()`; assert `href`, text and `aria-current="page"`. A new sidenav link means updating the `links()` expectations there.

Real routes — model: `app.routes.spec.ts`. To check redirects and lazy routes as declared, provide `provideRouter(routes)` (no blank components), `await router.navigateByUrl(...)` and assert the result and `router.url`. Every page the navigation lazy-loads gets a side-effect import at the top of the spec, with the reason in a comment:

```ts
import { routes } from './app.routes';
// Loads the page and its Material dependencies at import time, as the other specs do,
// so the test only times the navigation.
import './transactions/pages/transactions-page/transactions-page';
```

Without it, the first `loadComponent` imports the page and its dependencies inside the test and, under the load of the full suite, can exceed the 5 s timeout (`front/bugs/teste-de-rota-raiz-intermitente/`).
