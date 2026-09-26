import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { LOCALE_ID } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { By } from '@angular/platform-browser';
import { ActivatedRoute, convertToParamMap, Params } from '@angular/router';
import { Subject } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { buildCategory } from '../../../categories/testing/category-fixture';
import { BottomSheetService } from '../../../shared/bottom-sheet/services/bottom-sheet.service';
import { MonthPickerSheet } from '../../../shared/month-picker-sheet/month-picker-sheet';
import { formatSignedMoney } from '../../../shared/utils/format-currency';
import { buildTransaction } from '../../testing/transaction-fixture';
import { Transaction } from '../../types/transaction';
import { DEFAULT_TRANSACTION_FILTERS } from '../../types/transaction-filters';
import { TRANSACTION_SORT_OPTIONS } from '../../types/transaction-sort';
import { TransactionFiltersSheet } from '../transaction-filters-sheet/transaction-filters-sheet';
import { TransactionFormSheet } from '../transaction-form-sheet/transaction-form-sheet';
import { TransactionsPage } from './transactions-page';

registerLocaleData(localePt);

describe('TransactionsPage', () => {
	const baseUrl = `${environment.apiUrl}/Transactions`;
	const categoriesUrl = `${environment.apiUrl}/Categories`;
	const mercado = buildCategory({ id: 'mercado', name: 'Mercado', color: '#43A047' });
	const salario = buildCategory({ id: 'salario', name: 'Salário', color: '#1E88E5' });
	const feira = buildTransaction({ id: 't1', description: 'Feira', categoryId: 'mercado', value: -186.42, date: '2026-09-24T00:00:00Z' });
	const pao = buildTransaction({ id: 't2', description: null, categoryId: 'mercado', value: -12.5, date: '2026-09-24T00:00:00Z' });
	const pagamento = buildTransaction({ id: 't3', description: 'Salário', categoryId: 'salario', value: 8600, date: '2026-09-05T00:00:00Z' });

	let fixture: ComponentFixture<TransactionsPage>;
	let element: HTMLElement;
	let httpTesting: HttpTestingController;
	let bottomSheet: { open: ReturnType<typeof vi.fn> };
	let snackBar: { open: ReturnType<typeof vi.fn> };
	let snackBarAction: Subject<void>;

	async function create(queryParams: Params = {}) {
		bottomSheet = { open: vi.fn() };
		snackBarAction = new Subject<void>();
		snackBar = { open: vi.fn().mockReturnValue({ onAction: () => snackBarAction }) };

		await TestBed.configureTestingModule({
			imports: [TransactionsPage],
			providers: [
				{ provide: LOCALE_ID, useValue: 'pt-BR' },
				{ provide: ActivatedRoute, useValue: { snapshot: { queryParamMap: convertToParamMap(queryParams) } } },
				provideHttpClient(),
				provideHttpClientTesting(),
				{ provide: BottomSheetService, useValue: bottomSheet },
				{ provide: MatSnackBar, useValue: snackBar },
			],
		}).compileComponents();

		httpTesting = TestBed.inject(HttpTestingController);
		fixture = TestBed.createComponent(TransactionsPage);
		element = fixture.nativeElement;
	}

	afterEach(() => httpTesting.verify());

	function expectList(): TestRequest {
		TestBed.tick();
		return httpTesting.expectOne((req) => req.method === 'GET' && req.url === baseUrl);
	}

	function expectCategories(): TestRequest {
		TestBed.tick();
		return httpTesting.expectOne((req) => req.method === 'GET' && req.url === categoriesUrl);
	}

	async function flushList(transactions: Transaction[]) {
		expectList().flush(transactions);
		await fixture.whenStable();
	}

	/** Answers the first load: the transactions and the categories used for names, colors, filters and the form. */
	async function load(transactions: Transaction[]) {
		expectCategories().flush([mercado, salario]);
		await flushList(transactions);
	}

	function groupLabels() {
		return Array.from(element.querySelectorAll('.group-header .label')).map((n) => n.textContent?.trim());
	}

	function groupTotals() {
		return Array.from(element.querySelectorAll('.group-header .total')).map((n) => n.textContent?.trim());
	}

	function renderedDescriptions() {
		return Array.from(element.querySelectorAll('app-transaction-row .description')).map((n) => n.textContent?.trim());
	}

	function header() {
		return fixture.debugElement.query(By.css('app-page-header')).componentInstance;
	}

	function emitFrom(selector: string, event: string, payload?: unknown) {
		fixture.debugElement.query(By.css(selector)).triggerEventHandler(event, payload);
	}

	/** Returns the options of the last sheet opened for the given component and answers it through its callback. */
	function lastSheet(component: unknown) {
		const call = bottomSheet.open.mock.calls.filter(([c]) => c === component).at(-1);
		expect(call).toBeDefined();
		const options = call![1];
		const sheet = { close: vi.fn() };
		return {
			data: options.data,
			answer: (result: unknown) => {
				options.callback(result, sheet);
				return sheet;
			},
		};
	}

	describe('loading', () => {
		beforeEach(() => create());

		it('should request the most recent transactions and all categories when opened', () => {
			const { params } = expectList().request;
			expect(params.keys()).toEqual(['orderBy', 'asc']);
			expect(params.get('orderBy')).toBe('Date');
			expect(params.get('asc')).toBe('false');

			const categories = expectCategories().request.params;
			expect(categories.has('dateRef')).toBe(false);
			expect(categories.get('orderBy')).toBe('Name');
		});

		it('should show a progress bar while loading', () => {
			const req = expectList();
			expectCategories().flush([]);
			fixture.detectChanges();

			expect(element.querySelector('mat-progress-bar')).not.toBeNull();
			expect(element.querySelector('app-empty-state')).toBeNull();
			req.flush([]);
		});

		it('should group the transactions by day in the API order', async () => {
			await load([feira, pao, pagamento]);

			expect(groupLabels()).toEqual(['24 de setembro', '5 de setembro']);
			expect(groupTotals()).toEqual([`${formatSignedMoney(-198.92)} no dia`, `${formatSignedMoney(8600)} no dia`]);
			expect(renderedDescriptions()).toEqual(['Feira', 'Sem descrição', 'Salário']);
			expect(element.querySelector('mat-progress-bar')).toBeNull();
		});

		it('should show the category name and color of each transaction', async () => {
			await load([feira, pagamento]);

			const metas = Array.from(element.querySelectorAll('app-transaction-row .meta')).map((n) => n.textContent?.trim());
			expect(metas).toEqual(['Mercado', 'Salário']);
			expect(element.querySelector<HTMLElement>('app-transaction-row .dot')!.style.background).toBe('rgb(67, 160, 71)');
		});

		it('should mute transactions without description', async () => {
			await load([pao]);

			expect(element.querySelector('app-transaction-row .description')!.classList).toContain('muted');
		});

		it('should show the empty state when there are no transactions', async () => {
			await load([]);

			expect(element.querySelector('app-empty-state h2')?.textContent).toBe('Nenhuma transação encontrada');
		});

		it('should open the create form from the empty state', async () => {
			await load([]);

			element.querySelector<HTMLButtonElement>('app-empty-state button')!.click();

			expect(lastSheet(TransactionFormSheet).data).toEqual({ transaction: undefined, categories: [mercado, salario] });
		});

		it('should show an error state and retry', async () => {
			expectCategories().flush([mercado, salario]);
			expectList().flush(null, { status: 500, statusText: 'Server Error' });
			await fixture.whenStable();

			expect(element.querySelector('app-empty-state h2')?.textContent).toBe('Não foi possível carregar as transações');

			element.querySelector<HTMLButtonElement>('app-empty-state button')!.click();
			await flushList([feira]);

			expect(renderedDescriptions()).toEqual(['Feira']);
		});
	});

	describe('category from the route', () => {
		beforeEach(() => create({ categoryId: 'salario' }));

		it('should filter by the category given in the query string and show its chip', async () => {
			const req = expectList();
			expect(req.request.params.get('categoryId')).toBe('salario');
			req.flush([pagamento]);
			expectCategories().flush([mercado, salario]);
			await fixture.whenStable();

			expect(header().chips()).toEqual(['Salário']);
			expect(header().filtersActive()).toBe(true);
		});
	});

	describe('sorting', () => {
		beforeEach(async () => {
			await create();
			await load([feira, pao, pagamento]);
		});

		function applySort(index: number) {
			emitFrom('app-page-header', 'openFilters');
			lastSheet(TransactionFiltersSheet).answer({ filters: DEFAULT_TRANSACTION_FILTERS, sort: TRANSACTION_SORT_OPTIONS[index] });
		}

		it('should reload with the chosen sort', () => {
			applySort(3);

			const { params } = expectList().request;
			expect(params.get('orderBy')).toBe('Value');
			expect(params.get('asc')).toBe('true');
		});

		it('should keep day groups in ascending order when sorted by oldest', async () => {
			applySort(1);
			await flushList([pagamento, feira, pao]);

			expect(groupLabels()).toEqual(['5 de setembro', '24 de setembro']);
		});

		it('should show a single group in the API order when not sorted by date', async () => {
			applySort(2);
			await flushList([pagamento, pao, feira]);

			expect(groupLabels()).toEqual(['Maior valor']);
			expect(groupTotals()).toEqual([`${formatSignedMoney(8401.08)} no total`]);
			expect(renderedDescriptions()).toEqual(['Salário', 'Sem descrição', 'Feira']);
		});

		it('should not add a chip for the sort', async () => {
			applySort(6);
			await flushList([]);

			expect(header().chips()).toEqual([]);
			expect(header().filtersActive()).toBe(false);
		});
	});

	describe('filters', () => {
		beforeEach(async () => {
			await create();
			await load([feira, pao, pagamento]);
		});

		it('should open the filters sheet with the current state and the categories', () => {
			emitFrom('app-page-header', 'openFilters');

			expect(lastSheet(TransactionFiltersSheet).data).toEqual({
				filters: DEFAULT_TRANSACTION_FILTERS,
				sort: TRANSACTION_SORT_OPTIONS[0],
				monthRef: null,
				categories: [mercado, salario],
			});
		});

		it('should reload with the applied filters and show chips', async () => {
			emitFrom('app-page-header', 'openFilters');
			const sheet = lastSheet(TransactionFiltersSheet).answer({
				filters: { description: 'fei', type: 'Out', categoryId: 'mercado' },
				sort: TRANSACTION_SORT_OPTIONS[0],
			});

			expect(sheet.close).toHaveBeenCalled();
			const req = expectList();
			expect(req.request.params.get('description')).toBe('fei');
			expect(req.request.params.get('type')).toBe('Out');
			expect(req.request.params.get('categoryId')).toBe('mercado');
			req.flush([feira]);
			await fixture.whenStable();

			expect(header().chips()).toEqual(['"fei"', 'Saídas', 'Mercado']);
			expect(header().filtersActive()).toBe(true);
		});

		it('should reload without a filter when its chip is removed', async () => {
			emitFrom('app-page-header', 'openFilters');
			lastSheet(TransactionFiltersSheet).answer({
				filters: { description: 'fei', type: 'In', categoryId: 'all' },
				sort: TRANSACTION_SORT_OPTIONS[0],
			});
			await flushList([]);
			expect(header().chips()).toEqual(['"fei"', 'Entradas']);

			emitFrom('app-page-header', 'removeChip', 1);

			const { params } = expectList().request;
			expect(params.get('description')).toBe('fei');
			expect(params.has('type')).toBe(false);
		});

		it('should reload for the month picked and send it as dateRef', async () => {
			emitFrom('app-page-header', 'openMonth');
			const sheet = lastSheet(MonthPickerSheet).answer({ month: 8, year: 2026 });

			expect(sheet.close).toHaveBeenCalled();
			const req = expectList();
			expect(req.request.params.get('dateRef')).toBe('2026-09-01');
			req.flush([]);
			await fixture.whenStable();
			expect(header().monthLabel()).toBe('setembro 2026');
		});

		it('should drop dateRef when the month is cleared', async () => {
			emitFrom('app-page-header', 'openMonth');
			lastSheet(MonthPickerSheet).answer({ month: 8, year: 2026 });
			await flushList([]);

			emitFrom('app-page-header', 'openMonth');
			expect(lastSheet(MonthPickerSheet).data).toEqual({ month: 8, year: 2026, clearable: true });
			lastSheet(MonthPickerSheet).answer(null);

			const req = expectList();
			expect(req.request.params.has('dateRef')).toBe(false);
			req.flush([]);
			await fixture.whenStable();
			expect(header().monthLabel()).toBe('Todos os meses');
		});
	});

	describe('saving', () => {
		const input = { description: 'Pão', categoryId: 'mercado', value: -12.5, date: '2026-09-24T00:00:00.000Z' };

		beforeEach(async () => {
			await create();
			await load([feira, pagamento]);
		});

		it('should create a transaction and reload the list', async () => {
			emitFrom('app-page-header', 'action');
			const sheet = lastSheet(TransactionFormSheet).answer({ type: 'save', result: { id: undefined, ...input } });

			expect(sheet.close).toHaveBeenCalled();
			const req = httpTesting.expectOne(baseUrl);
			expect(req.request.method).toBe('POST');
			expect(req.request.body).toEqual(input);
			req.flush(buildTransaction({ id: 't9', ...input }), { status: 201, statusText: 'Created' });

			expect(snackBar.open).toHaveBeenCalledWith('Transação criada.', undefined, { duration: 3000 });
			await flushList([feira, buildTransaction({ id: 't9', ...input }), pagamento]);
			expect(renderedDescriptions()).toContain('Pão');
		});

		it('should open the edit form when a transaction is clicked and update it', async () => {
			element.querySelector<HTMLButtonElement>('app-transaction-row button')!.click();
			expect(lastSheet(TransactionFormSheet).data).toEqual({ transaction: feira, categories: [mercado, salario] });

			lastSheet(TransactionFormSheet).answer({ type: 'save', result: { id: feira.id, ...input } });

			const req = httpTesting.expectOne(`${baseUrl}/${feira.id}`);
			expect(req.request.method).toBe('PUT');
			expect(req.request.body).toEqual(input);
			req.flush({ ...feira, ...input });

			expect(snackBar.open).toHaveBeenCalledWith('Transação salva.', undefined, { duration: 3000 });
			await flushList([{ ...feira, ...input }, pagamento]);
		});

		it('should show the API error detail and not reload when saving fails', () => {
			emitFrom('app-page-header', 'action');
			lastSheet(TransactionFormSheet).answer({ type: 'save', result: { ...input, categoryId: 'removida' } });

			httpTesting
				.expectOne(baseUrl)
				.flush({ status: 404, title: 'Not Found', detail: 'Categoria não encontrada.' }, { status: 404, statusText: 'Not Found' });

			expect(snackBar.open).toHaveBeenCalledWith('Categoria não encontrada.', 'Fechar', { duration: 5000 });
			TestBed.tick();
			httpTesting.expectNone(baseUrl);
		});

		it('should show a fallback message when the error has no detail', () => {
			emitFrom('app-transaction-list', 'openTransaction', feira.id);
			lastSheet(TransactionFormSheet).answer({ type: 'save', result: { id: feira.id, ...input } });

			httpTesting.expectOne(`${baseUrl}/${feira.id}`).flush(null, { status: 500, statusText: 'Server Error' });

			expect(snackBar.open).toHaveBeenCalledWith('Não foi possível salvar a transação.', 'Fechar', { duration: 5000 });
		});
	});

	describe('deleting', () => {
		beforeEach(async () => {
			await create();
			await load([feira, pagamento]);
		});

		function deleteFeira() {
			emitFrom('app-transaction-list', 'openTransaction', feira.id);
			const sheet = lastSheet(TransactionFormSheet).answer({ type: 'delete', id: feira.id });
			expect(sheet.close).toHaveBeenCalled();
		}

		it('should delete the transaction, reload and offer to undo', async () => {
			deleteFeira();

			const req = httpTesting.expectOne(`${baseUrl}/${feira.id}`);
			expect(req.request.method).toBe('DELETE');
			req.flush(null, { status: 204, statusText: 'No Content' });

			expect(snackBar.open).toHaveBeenCalledWith('Transação excluída.', 'Desfazer', { duration: 5000 });
			await flushList([pagamento]);
			expect(renderedDescriptions()).toEqual(['Salário']);
		});

		it('should recreate the transaction when the deletion is undone', async () => {
			deleteFeira();
			httpTesting.expectOne(`${baseUrl}/${feira.id}`).flush(null, { status: 204, statusText: 'No Content' });
			await flushList([pagamento]);

			snackBarAction.next();

			const req = httpTesting.expectOne(baseUrl);
			expect(req.request.method).toBe('POST');
			expect(req.request.body).toEqual({
				description: feira.description,
				categoryId: feira.categoryId,
				value: feira.value,
				date: feira.date,
			});
			req.flush({ ...feira, id: 't10' }, { status: 201, statusText: 'Created' });

			expect(snackBar.open).toHaveBeenCalledWith('Transação restaurada.', undefined, { duration: 3000 });
			await flushList([{ ...feira, id: 't10' }, pagamento]);
			expect(renderedDescriptions()).toEqual(['Feira', 'Salário']);
		});

		it('should show the API error and keep the list when the deletion fails', () => {
			deleteFeira();

			httpTesting
				.expectOne(`${baseUrl}/${feira.id}`)
				.flush({ status: 404, detail: 'Transação não encontrada.' }, { status: 404, statusText: 'Not Found' });

			expect(snackBar.open).toHaveBeenCalledWith('Transação não encontrada.', 'Fechar', { duration: 5000 });
			TestBed.tick();
			httpTesting.expectNone(baseUrl);
		});
	});
});
