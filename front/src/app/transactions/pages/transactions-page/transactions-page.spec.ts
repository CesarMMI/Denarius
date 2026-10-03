import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { LOCALE_ID } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { By } from '@angular/platform-browser';
import { ActivatedRoute, convertToParamMap, Params } from '@angular/router';
import { of, Subject } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { buildCategory } from '../../../categories/testing/category-fixture';
import { TransactionForm } from '../../components/transaction-form/transaction-form';
import { TransactionsFilters } from '../../components/transactions-filters/transactions-filters';
import { buildTransaction } from '../../testing/transaction-fixture';
import { Transaction } from '../../types/transaction';
import { TransactionsPage } from './transactions-page';

registerLocaleData(localePt);

describe('TransactionsPage', () => {
	const baseUrl = `${environment.apiUrl}/transactions`;
	const categoriesUrl = `${environment.apiUrl}/categories`;
	const mercado = buildCategory({ id: 'mercado', name: 'Mercado', color: '#43A047' });
	const salario = buildCategory({ id: 'salario', name: 'Salário', color: '#1E88E5' });
	const feira = buildTransaction({
		id: 't1',
		description: 'Feira',
		categoryId: 'mercado',
		value: -186.42,
		date: '2026-09-24T00:00:00Z',
	});
	const pao = buildTransaction({
		id: 't2',
		description: null,
		categoryId: 'mercado',
		value: -12.5,
		date: '2026-09-24T00:00:00Z',
	});
	const pagamento = buildTransaction({
		id: 't3',
		description: 'Salário',
		categoryId: 'salario',
		value: 8600,
		date: '2026-09-05T00:00:00Z',
	});

	let fixture: ComponentFixture<TransactionsPage>;
	let element: HTMLElement;
	let httpTesting: HttpTestingController;
	let dialog: { open: ReturnType<typeof vi.fn> };
	let snackBar: { open: ReturnType<typeof vi.fn> };
	let snackBarAction: Subject<void>;

	function create(queryParams: Params = {}) {
		dialog = { open: vi.fn() };
		snackBarAction = new Subject<void>();
		snackBar = { open: vi.fn().mockReturnValue({ onAction: () => snackBarAction }) };

		TestBed.configureTestingModule({
			providers: [
				{ provide: LOCALE_ID, useValue: 'pt-BR' },
				{ provide: ActivatedRoute, useValue: { snapshot: { queryParamMap: convertToParamMap(queryParams) } } },
				provideHttpClient(),
				provideHttpClientTesting(),
				{ provide: MatDialog, useValue: dialog },
				{ provide: MatSnackBar, useValue: snackBar },
			],
		});

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

	function column(name: string) {
		return Array.from(element.querySelectorAll(`tr[mat-row] .mat-column-${name}`)).map((cell) =>
			cell.textContent?.trim(),
		);
	}

	function noDataRow() {
		return element.querySelector('td[colspan]')?.textContent?.trim();
	}

	function spinner() {
		return element.querySelector('mat-progress-spinner');
	}

	function button(text: string) {
		return Array.from(element.querySelectorAll('button')).find((b) => b.textContent?.trim().endsWith(text))!;
	}

	function headerButton(label: string) {
		return element.querySelector<HTMLButtonElement>(`app-page-header button[aria-label="${label}"]`);
	}

	/** The menu opens in an overlay, outside the page. */
	async function sortBy(label: string) {
		element.querySelector<HTMLButtonElement>('app-sort-menu button')!.click();
		await fixture.whenStable();
		Array.from(document.querySelectorAll<HTMLButtonElement>('.mat-mdc-menu-panel [mat-menu-item]'))
			.find((item) => item.textContent?.includes(label))!
			.click();
	}

	function rowButton(row: number, label: 'Editar' | 'Excluir') {
		return element.querySelectorAll<HTMLButtonElement>(`tr[mat-row] button[aria-label="${label}"]`)[row];
	}

	function filters(): TransactionsFilters | undefined {
		return fixture.debugElement.query(By.directive(TransactionsFilters))?.componentInstance;
	}

	function dialogReturns(result: unknown) {
		dialog.open.mockReturnValue({ afterClosed: () => of(result) });
	}

	describe('loading', () => {
		beforeEach(() => create());

		it('should request the most recent transactions and all the categories when opened', () => {
			expect(expectList().request.params.toString()).toBe('orderBy=date&asc=false');
			expect(expectCategories().request.params.keys()).toEqual([]);
		});

		it('should show a spinner while loading', () => {
			const req = expectList();
			expectCategories().flush([]);
			fixture.detectChanges();

			expect(spinner()).not.toBeNull();
			req.flush([]);
		});

		it('should list the transactions in the API order with their category', async () => {
			await load([feira, pao, pagamento]);

			expect(column('description')).toEqual(['Feira', '', 'Salário']);
			expect(column('category')).toEqual(['Mercado', 'Mercado', 'Salário']);
			expect(spinner()).toBeNull();
		});

		it('should say when there are no transactions', async () => {
			await load([]);

			expect(noDataRow()).toBe('Nenhuma transação encontrada.');
		});

		it('should show an error and reload both lists from the header', async () => {
			expectCategories().flush(null, { status: 500, statusText: 'Server Error' });
			await flushList([feira]);

			expect(noDataRow()).toBe('Não foi possível carregar as transações.');
			expect(column('description')).toEqual([]);

			headerButton('Recarregar')!.click();
			expectCategories().flush([mercado, salario]);
			await flushList([feira]);

			expect(column('description')).toEqual(['Feira']);
			expect(column('category')).toEqual(['Mercado']);
		});

		it('should not open the form while the categories failed to load', async () => {
			expectCategories().flush(null, { status: 500, statusText: 'Server Error' });
			await flushList([feira]);

			button('Nova transação').click();

			expect(dialog.open).not.toHaveBeenCalled();
			expect(snackBar.open).toHaveBeenCalledWith('Não foi possível carregar as categorias. Tente novamente.', 'Fechar', {
				duration: 5000,
			});
		});
	});

	describe('filters', () => {
		beforeEach(async () => {
			create();
			await load([feira, pao, pagamento]);
		});

		it('should be hidden until shown from the header', async () => {
			expect(filters()).toBeUndefined();

			headerButton('Exibir filtros')!.click();
			await fixture.whenStable();
			expect(filters()!.categories()).toEqual([mercado, salario]);

			headerButton('Ocultar filtros')!.click();
			await fixture.whenStable();
			expect(filters()).toBeUndefined();
		});

		it('should reload with the filters chosen', async () => {
			headerButton('Exibir filtros')!.click();
			await fixture.whenStable();

			filters()!.filters.set({ description: 'fei', type: 'out', categoryId: 'mercado', month: new Date(2026, 8, 1) });

			expect(expectList().request.params.toString()).toBe(
				'description=fei&type=out&categoryId=mercado&dateRef=2026-09-01&orderBy=date&asc=false',
			);
		});
	});

	it('should filter by the category given in the query string, showing the filters', async () => {
		create({ categoryId: 'salario' });

		const req = expectList();
		expect(req.request.params.get('categoryId')).toBe('salario');
		req.flush([pagamento]);
		expectCategories().flush([mercado, salario]);
		await fixture.whenStable();

		const category = Array.from(element.querySelectorAll('mat-form-field')).find(
			(field) => field.querySelector('mat-label')?.textContent === 'Categoria',
		);
		expect(category!.textContent).toContain('Salário');
	});

	describe('sorting', () => {
		beforeEach(async () => {
			create();
			await load([feira, pao, pagamento]);
		});

		it('should show the most recent first', () => {
			expect(headerButton('Ordenar: Mais recentes primeiro')).not.toBeNull();
		});

		it('should reload sorted by the option chosen in the sort menu', async () => {
			await sortBy('Menor valor primeiro');

			const req = expectList();
			expect(req.request.params.toString()).toBe('orderBy=value&asc=true');
			req.flush([pao, feira, pagamento]);
			await fixture.whenStable();
			expect(column('description')).toEqual(['', 'Feira', 'Salário']);
		});
	});

	describe('saving', () => {
		const input = { description: 'Pão', categoryId: 'mercado', value: -12.5, date: '2026-09-24T00:00:00.000Z' };

		beforeEach(async () => {
			create();
			await load([feira, pagamento]);
		});

		it('should create a transaction from the form and reload', async () => {
			dialogReturns(input);
			button('Nova transação').click();

			expect(dialog.open).toHaveBeenCalledWith(TransactionForm, {
				data: { transaction: undefined, categories: [mercado, salario] },
			});
			const req = httpTesting.expectOne(baseUrl);
			expect(req.request.method).toBe('POST');
			expect(req.request.body).toEqual(input);
			req.flush(buildTransaction({ id: 't9', ...input }), { status: 201, statusText: 'Created' });

			expect(snackBar.open).toHaveBeenCalledWith('Transação criada.', undefined, { duration: 3000 });
			await flushList([feira, buildTransaction({ id: 't9', ...input }), pagamento]);
			expect(column('description')).toContain('Pão');
		});

		it('should update the transaction edited and reload', async () => {
			dialogReturns(input);
			rowButton(0, 'Editar').click();

			expect(dialog.open).toHaveBeenCalledWith(TransactionForm, {
				data: { transaction: feira, categories: [mercado, salario] },
			});
			const req = httpTesting.expectOne(`${baseUrl}/${feira.id}`);
			expect(req.request.method).toBe('PUT');
			expect(req.request.body).toEqual(input);
			req.flush({ ...feira, ...input });

			expect(snackBar.open).toHaveBeenCalledWith('Transação salva.', undefined, { duration: 3000 });
			await flushList([{ ...feira, ...input }, pagamento]);
		});

		it('should do nothing when the form is cancelled', () => {
			dialogReturns(undefined);
			button('Nova transação').click();

			TestBed.tick();
			httpTesting.expectNone(baseUrl);
			expect(snackBar.open).not.toHaveBeenCalled();
		});

		it('should show the API error detail and not reload when saving fails', () => {
			dialogReturns({ ...input, categoryId: 'removida' });
			button('Nova transação').click();

			httpTesting
				.expectOne(baseUrl)
				.flush(
					{ status: 404, title: 'Not Found', detail: 'Categoria não encontrada.' },
					{ status: 404, statusText: 'Not Found' },
				);

			expect(snackBar.open).toHaveBeenCalledWith('Categoria não encontrada.', 'Fechar', { duration: 5000 });
			TestBed.tick();
			httpTesting.expectNone(baseUrl);
		});

		it('should show a fallback message when the error has no detail', () => {
			dialogReturns(input);
			rowButton(0, 'Editar').click();

			httpTesting.expectOne(`${baseUrl}/${feira.id}`).flush(null, { status: 500, statusText: 'Server Error' });

			expect(snackBar.open).toHaveBeenCalledWith('Não foi possível salvar a transação.', 'Fechar', { duration: 5000 });
		});
	});

	describe('deleting', () => {
		beforeEach(async () => {
			create();
			await load([feira, pagamento]);
		});

		function deleteFeira() {
			rowButton(0, 'Excluir').click();
			const req = httpTesting.expectOne(`${baseUrl}/${feira.id}`);
			expect(req.request.method).toBe('DELETE');
			return req;
		}

		it('should delete the transaction, reload and offer to undo', async () => {
			deleteFeira().flush(null, { status: 204, statusText: 'No Content' });

			expect(snackBar.open).toHaveBeenCalledWith('Transação excluída.', 'Desfazer', { duration: 5000 });
			await flushList([pagamento]);
			expect(column('description')).toEqual(['Salário']);
		});

		it('should create the transaction again when the deletion is undone', async () => {
			deleteFeira().flush(null, { status: 204, statusText: 'No Content' });
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
			expect(column('description')).toEqual(['Feira', 'Salário']);
		});

		it('should show the API error and keep the list when the deletion fails', () => {
			deleteFeira().flush({ status: 404, detail: 'Transação não encontrada.' }, { status: 404, statusText: 'Not Found' });

			expect(snackBar.open).toHaveBeenCalledWith('Transação não encontrada.', 'Fechar', { duration: 5000 });
			TestBed.tick();
			httpTesting.expectNone(baseUrl);
		});
	});
});
