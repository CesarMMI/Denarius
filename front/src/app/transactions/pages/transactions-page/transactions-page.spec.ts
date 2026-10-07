import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { LOCALE_ID } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDatepicker } from '@angular/material/datepicker';
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

const MERCADO_ID = '3f2a1c4e-0000-4000-8000-0000000000a1';
const SALARIO_ID = '3f2a1c4e-0000-4000-8000-0000000000a2';
const REMOVIDA_ID = '3f2a1c4e-0000-4000-8000-0000000000a3';
const FEIRA_ID = '7b1d2e3f-0000-4000-8000-0000000000b1';
const PAO_ID = '7b1d2e3f-0000-4000-8000-0000000000b2';
const PAGAMENTO_ID = '7b1d2e3f-0000-4000-8000-0000000000b3';
const CRIADA_ID = '7b1d2e3f-0000-4000-8000-0000000000b9';
const RESTAURADA_ID = '7b1d2e3f-0000-4000-8000-0000000000ba';

describe('TransactionsPage', () => {
	const baseUrl = `${environment.apiUrl}/transactions`;
	const categoriesUrl = `${environment.apiUrl}/categories`;
	const mercado = buildCategory({ id: MERCADO_ID, name: 'Mercado', color: '#43A047' });
	const salario = buildCategory({ id: SALARIO_ID, name: 'Salário', color: '#1E88E5' });
	const feira = buildTransaction({
		id: FEIRA_ID,
		description: 'Feira',
		categoryId: MERCADO_ID,
		value: -186.42,
		date: '2026-09-24T00:00:00Z',
	});
	const pao = buildTransaction({
		id: PAO_ID,
		description: null,
		categoryId: MERCADO_ID,
		value: -12.5,
		date: '2026-09-24T00:00:00Z',
	});
	const pagamento = buildTransaction({
		id: PAGAMENTO_ID,
		description: 'Salário',
		categoryId: SALARIO_ID,
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

	function expectNoListRequest() {
		TestBed.tick();
		httpTesting.expectNone((req) => req.method === 'GET' && req.url === baseUrl);
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

		it('should keep the rows in sight while reloading from the header', async () => {
			await load([feira, pagamento]);

			headerButton('Recarregar')!.click();
			fixture.detectChanges();

			expect(column('description')).toEqual(['Feira', 'Salário']);
			expect(spinner()).toBeNull();

			expectCategories().flush([mercado, salario]);
			await flushList([pagamento]);
			expect(column('description')).toEqual(['Salário']);
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

			filters()!.filters.set({ description: 'fei', type: 'out', categoryId: MERCADO_ID, month: new Date(2026, 8, 1) });

			expect(expectList().request.params.toString()).toBe(
				`description=fei&type=out&categoryId=${MERCADO_ID}&dateRef=2026-09-01&orderBy=date&asc=false`,
			);
		});
	});

	describe('filters, when nothing changes', () => {
		beforeEach(async () => {
			create();
			await load([feira, pao, pagamento]);
			headerButton('Exibir filtros')!.click();
			await fixture.whenStable();
		});

		function descriptionInput() {
			return element.querySelector<HTMLInputElement>('app-transactions-filters input[matInput]')!;
		}

		/** Typed in the DOM: a harness would wait on the request the typing may send. */
		function typeDescription(text: string) {
			descriptionInput().value = text;
			descriptionInput().dispatchEvent(new Event('input'));
		}

		function pause() {
			return new Promise((resolve) => setTimeout(resolve, 300));
		}

		function pickMonth(month: Date) {
			const datepicker: MatDatepicker<Date> = fixture.debugElement.query(By.directive(MatDatepicker)).componentInstance;
			datepicker.monthSelected.emit(month);
		}

		async function applyDescription(text: string) {
			typeDescription(text);
			await pause();
			const req = expectList();
			expect(req.request.params.get('description')).toBe(text);
			req.flush([feira]);
			await fixture.whenStable();
		}

		it('should not reload when the description is typed again as the text applied', async () => {
			await applyDescription('fei');

			typeDescription('fe');
			typeDescription('fei');
			await pause();

			expectNoListRequest();
		});

		it('should not reload when leaving the description field after the text applied did not change', async () => {
			await applyDescription('fei');
			typeDescription('fe');
			typeDescription('fei');
			await pause();
			// Answers what the typing may have sent; the test above covers it.
			TestBed.tick();
			httpTesting.match((req) => req.method === 'GET' && req.url === baseUrl).forEach((req) => req.flush([feira]));

			descriptionInput().dispatchEvent(new Event('blur'));

			expectNoListRequest();
		});

		it('should not reload when the month in use is picked again', async () => {
			pickMonth(new Date(2026, 8, 1));
			const req = expectList();
			expect(req.request.params.get('dateRef')).toBe('2026-09-01');
			req.flush([feira]);
			await fixture.whenStable();

			pickMonth(new Date(2026, 8, 1));

			expectNoListRequest();
		});
	});

	it('should filter by the category given in the query string, showing the filters', async () => {
		create({ categoryId: SALARIO_ID });

		const req = expectList();
		expect(req.request.params.get('categoryId')).toBe(SALARIO_ID);
		req.flush([pagamento]);
		expectCategories().flush([mercado, salario]);
		await fixture.whenStable();

		const category = Array.from(element.querySelectorAll('mat-form-field')).find(
			(field) => field.querySelector('mat-label')?.textContent === 'Categoria',
		);
		expect(category!.textContent).toContain('Salário');
	});

	it('should filter by the month given in the query string, showing the filters', async () => {
		create({ month: '2026-09' });

		const req = expectList();
		expect(req.request.params.get('dateRef')).toBe('2026-09-01');
		req.flush([feira, pao, pagamento]);
		expectCategories().flush([mercado, salario]);
		await fixture.whenStable();

		expect(element.querySelector<HTMLInputElement>('app-month-field input')!.value).toBe('09/2026');
	});

	it('should ignore a month in the query string that is not YYYY-MM', async () => {
		create({ month: 'setembro' });

		const req = expectList();
		expect(req.request.params.has('dateRef')).toBe(false);
		req.flush([]);
		expectCategories().flush([mercado, salario]);
		await fixture.whenStable();
		expect(filters()).toBeUndefined();
	});

	describe('category in the query string', () => {
		function categoryField() {
			return Array.from(element.querySelectorAll('mat-form-field')).find(
				(field) => field.querySelector('mat-label')?.textContent === 'Categoria',
			);
		}

		it('should say the transactions failed to load when the API refuses a category that is not a UUID', async () => {
			create({ categoryId: 'nao-e-uuid' });

			const req = expectList();
			expect(req.request.params.get('categoryId')).toBe('nao-e-uuid');
			req.flush(
				{
					type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1',
					title: 'One or more validation errors occurred.',
					status: 400,
					errors: { categoryId: ["The value 'nao-e-uuid' is not valid."] },
				},
				{ status: 400, statusText: 'Bad Request' },
			);
			expectCategories().flush([mercado, salario]);
			await fixture.whenStable();

			expect(noDataRow()).toBe('Não foi possível carregar as transações.');
			expect(filters()).toBeDefined();
			expect(categoryField()!.querySelector('.mat-mdc-select-value')!.textContent!.trim()).toBe('');
		});

		it('should say there are no transactions for a category that does not exist', async () => {
			const unknownId = '3f2a1c4e-0000-4000-8000-0000000000ff';
			create({ categoryId: unknownId });

			const req = expectList();
			expect(req.request.params.get('categoryId')).toBe(unknownId);
			req.flush([]);
			expectCategories().flush([mercado, salario]);
			await fixture.whenStable();

			expect(noDataRow()).toBe('Nenhuma transação encontrada.');
		});

		it('should filter by the category and the month together, showing both', async () => {
			create({ categoryId: SALARIO_ID, month: '2026-09' });

			const req = expectList();
			expect(req.request.params.get('categoryId')).toBe(SALARIO_ID);
			expect(req.request.params.get('dateRef')).toBe('2026-09-01');
			req.flush([pagamento]);
			expectCategories().flush([mercado, salario]);
			await fixture.whenStable();

			expect(filters()).toBeDefined();
			expect(categoryField()!.textContent).toContain('Salário');
			expect(element.querySelector<HTMLInputElement>('app-month-field input')!.value).toBe('09/2026');
		});
	});

	describe('layout', () => {
		beforeEach(async () => {
			create({ month: '2026-09' });
			await load([feira, pagamento]);
		});

		it('should keep the header and the filters outside the scrolling table card', () => {
			const table = element.querySelector<HTMLElement>('mat-card.table')!;

			expect(element.querySelector('app-page-header')).not.toBeNull();
			expect(element.querySelector('mat-card.filters app-transactions-filters')).not.toBeNull();
			expect(table.querySelector('app-page-header')).toBeNull();
			expect(table.querySelector('mat-card.filters')).toBeNull();

			const tableStyle = getComputedStyle(table);
			expect(tableStyle.overflow).toBe('auto');
			expect(tableStyle.flexGrow).toBe('1');
			expect(getComputedStyle(element).maxHeight).toBe('100%');
		});
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

		it('should not reload when the sort in use is chosen again', async () => {
			await sortBy('Mais recentes primeiro');

			expectNoListRequest();
		});
	});

	describe('saving', () => {
		const input = { description: 'Pão', categoryId: MERCADO_ID, value: -12.5, date: '2026-09-24T00:00:00.000Z' };

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
			req.flush(buildTransaction({ id: CRIADA_ID, ...input }), { status: 201, statusText: 'Created' });

			expect(snackBar.open).toHaveBeenCalledWith('Transação criada.', undefined, { duration: 3000 });
			await flushList([feira, buildTransaction({ id: CRIADA_ID, ...input }), pagamento]);
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
			dialogReturns({ ...input, categoryId: REMOVIDA_ID });
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

			httpTesting.expectOne(`${baseUrl}/${feira.id}`).error(new ProgressEvent('error'));

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
			req.flush({ ...feira, id: RESTAURADA_ID }, { status: 201, statusText: 'Created' });

			expect(snackBar.open).toHaveBeenCalledWith('Transação restaurada.', undefined, { duration: 3000 });
			await flushList([{ ...feira, id: RESTAURADA_ID }, pagamento]);
			expect(column('description')).toEqual(['Feira', 'Salário']);
		});

		it('should show a fallback message and keep the list when the deletion fails without a detail', () => {
			deleteFeira().error(new ProgressEvent('error'));

			expect(snackBar.open).toHaveBeenCalledWith('Não foi possível excluir a transação.', 'Fechar', { duration: 5000 });
			TestBed.tick();
			httpTesting.expectNone((req) => req.method === 'GET' && req.url === baseUrl);
		});

		it.each([
			[
				'the API error detail',
				(req: TestRequest) =>
					req.flush(
						{ status: 404, title: 'Not Found', detail: 'Categoria não encontrada.' },
						{ status: 404, statusText: 'Not Found' },
					),
				'Categoria não encontrada.',
			],
			[
				'a fallback message',
				(req: TestRequest) => req.error(new ProgressEvent('error')),
				'Não foi possível salvar a transação.',
			],
		])('should show %s and not reload when undoing fails', async (_, fail, message) => {
			deleteFeira().flush(null, { status: 204, statusText: 'No Content' });
			await flushList([pagamento]);

			snackBarAction.next();
			const req = httpTesting.expectOne(baseUrl);
			expect(req.request.method).toBe('POST');
			fail(req);

			expect(snackBar.open).toHaveBeenLastCalledWith(message, 'Fechar', { duration: 5000 });
			TestBed.tick();
			httpTesting.expectNone((req) => req.method === 'GET' && req.url === baseUrl);
		});

		it('should show the API error and keep the list when the deletion fails', () => {
			deleteFeira().flush({ status: 404, detail: 'Transação não encontrada.' }, { status: 404, statusText: 'Not Found' });

			expect(snackBar.open).toHaveBeenCalledWith('Transação não encontrada.', 'Fechar', { duration: 5000 });
			TestBed.tick();
			httpTesting.expectNone(baseUrl);
		});
	});

	describe('deleting, while in progress', () => {
		const noContent = { status: 204, statusText: 'No Content' };

		beforeEach(async () => {
			create();
			await load([feira, pagamento]);
		});

		function deleteRequests(id: string): TestRequest[] {
			TestBed.tick();
			return httpTesting.match((req) => req.method === 'DELETE' && req.url === `${baseUrl}/${id}`);
		}

		function rowSpinner(row: number) {
			return rowButton(row, 'Excluir').querySelector('mat-progress-spinner');
		}

		function isDisabled(button: HTMLButtonElement) {
			return button.getAttribute('aria-disabled') === 'true';
		}

		function progressBar() {
			return element.querySelector('mat-card.table mat-progress-bar');
		}

		/** The list takes the response in a later task; `whenStable()` would wait on the reload it may send. */
		function settle() {
			return new Promise((resolve) => setTimeout(resolve));
		}

		it('should not send another DELETE when "Excluir" is clicked again before the response', async () => {
			rowButton(0, 'Excluir').click();
			fixture.detectChanges();
			rowButton(0, 'Excluir').click();

			const requests = deleteRequests(feira.id);
			expect(requests).toHaveLength(1);

			requests[0].flush(null, noContent);
			await flushList([pagamento]);
		});

		it('should not send another DELETE after the deletion is accepted and before the list reloads', async () => {
			rowButton(0, 'Excluir').click();
			deleteRequests(feira.id)[0].flush(null, noContent);
			const reload = expectList();
			fixture.detectChanges();

			rowButton(0, 'Excluir').click();

			expect(deleteRequests(feira.id)).toHaveLength(0);
			expect(snackBar.open).toHaveBeenCalledExactlyOnceWith('Transação excluída.', 'Desfazer', { duration: 5000 });
			reload.flush([pagamento]);
			await fixture.whenStable();
			expect(column('description')).toEqual(['Salário']);
		});

		it('should disable "Excluir" and show a spinner in the row while it is being deleted', async () => {
			rowButton(0, 'Excluir').click();
			const request = deleteRequests(feira.id)[0];
			fixture.detectChanges();

			expect(isDisabled(rowButton(0, 'Excluir'))).toBe(true);
			expect(rowSpinner(0)).not.toBeNull();

			request.flush(null, noContent);
			await flushList([pagamento]);
		});

		it('should keep "Excluir" of the other rows enabled, without a spinner, while a row is being deleted', async () => {
			rowButton(0, 'Excluir').click();
			const feiraRequest = deleteRequests(feira.id)[0];
			fixture.detectChanges();

			expect(isDisabled(rowButton(1, 'Excluir'))).toBe(false);
			expect(rowSpinner(1)).toBeNull();

			rowButton(1, 'Excluir').click();
			const pagamentoRequests = deleteRequests(pagamento.id);
			expect(pagamentoRequests).toHaveLength(1);

			feiraRequest.flush(null, noContent);
			pagamentoRequests[0].flush(
				{ status: 404, detail: 'Transação não encontrada.' },
				{ status: 404, statusText: 'Not Found' },
			);
			await flushList([pagamento]);
		});

		it('should enable "Excluir" again when the deletion is refused', async () => {
			rowButton(0, 'Excluir').click();
			deleteRequests(feira.id)[0].error(new ProgressEvent('error'));
			await fixture.whenStable();

			expect(isDisabled(rowButton(0, 'Excluir'))).toBe(false);
			expect(rowSpinner(0)).toBeNull();

			rowButton(0, 'Excluir').click();
			const again = deleteRequests(feira.id);
			expect(again).toHaveLength(1);
			again[0].flush(null, noContent);
			await flushList([pagamento]);
		});

		it('should disable "Editar" while the row is being deleted', async () => {
			dialogReturns(undefined);
			rowButton(0, 'Excluir').click();
			const request = deleteRequests(feira.id)[0];
			fixture.detectChanges();

			expect(isDisabled(rowButton(0, 'Editar'))).toBe(true);
			rowButton(0, 'Editar').click();
			expect(dialog.open).not.toHaveBeenCalled();

			request.flush(null, noContent);
			const reload = expectList();
			fixture.detectChanges();

			expect(isDisabled(rowButton(0, 'Editar'))).toBe(true);
			rowButton(0, 'Editar').click();
			expect(dialog.open).not.toHaveBeenCalled();
			expect(isDisabled(rowButton(1, 'Editar'))).toBe(false);

			reload.flush([pagamento]);
			await fixture.whenStable();
		});

		describe('restoring', () => {
			const restored = { ...feira, id: RESTAURADA_ID };

			async function undoDeletion() {
				rowButton(0, 'Excluir').click();
				deleteRequests(feira.id)[0].flush(null, noContent);
				await flushList([pagamento]);
				expect(progressBar()).toBeNull();

				snackBarAction.next();
				return httpTesting.expectOne((req) => req.method === 'POST' && req.url === baseUrl);
			}

			it('should show that the restoration is in progress', async () => {
				const request = await undoDeletion();
				fixture.detectChanges();

				expect(progressBar()).not.toBeNull();
				expect(progressBar()!.getAttribute('aria-label')).toBe('Restaurando a transação');

				request.flush(restored, { status: 201, statusText: 'Created' });
				await flushList([restored, pagamento]);
			});

			it('should hide the progress once the restoration is accepted', async () => {
				const request = await undoDeletion();

				request.flush(restored, { status: 201, statusText: 'Created' });
				const reload = expectList();
				fixture.detectChanges();
				expect(progressBar()).toBeNull();

				reload.flush([restored, pagamento]);
				await fixture.whenStable();
				expect(column('description')).toEqual(['Feira', 'Salário']);
			});

			it('should hide the progress once the restoration is refused', async () => {
				const request = await undoDeletion();

				request.error(new ProgressEvent('error'));
				await fixture.whenStable();

				expect(progressBar()).toBeNull();
				expectNoListRequest();
			});
		});

		describe('when the list is already loading', () => {
			it('should reload again when a deletion is accepted during a reload', async () => {
				rowButton(0, 'Excluir').click();
				deleteRequests(feira.id)[0].flush(null, noContent);
				const firstReload = expectList();

				rowButton(1, 'Excluir').click();
				deleteRequests(pagamento.id)[0].flush(null, noContent);

				// Answered before the API saw the payment go.
				firstReload.flush([pagamento]);
				await settle();
				const secondReload = expectList();
				fixture.detectChanges();

				expect(column('description')).toEqual(['Salário']);
				expect(isDisabled(rowButton(0, 'Excluir'))).toBe(true);
				rowButton(0, 'Excluir').click();
				expect(deleteRequests(pagamento.id)).toHaveLength(0);

				secondReload.flush([]);
				await fixture.whenStable();
				expect(noDataRow()).toBe('Nenhuma transação encontrada.');
				expectNoListRequest();
			});

			it('should reload again when a deletion is accepted while the filters load the list', async () => {
				headerButton('Exibir filtros')!.click();
				await fixture.whenStable();

				rowButton(0, 'Excluir').click();
				const request = deleteRequests(feira.id)[0];
				filters()!.filters.set({ description: '', type: 'out', categoryId: '', month: null });
				const filtered = expectList();

				request.flush(null, noContent);
				filtered.flush([feira]);
				await settle();
				const reload = expectList();
				expect(reload.request.params.get('type')).toBe('out');

				reload.flush([]);
				await fixture.whenStable();
				expect(noDataRow()).toBe('Nenhuma transação encontrada.');
				expectNoListRequest();
			});
		});
	});
});
