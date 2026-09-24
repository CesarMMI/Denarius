import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { LOCALE_ID } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { By } from '@angular/platform-browser';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { BottomSheetService } from '../../../shared/bottom-sheet/services/bottom-sheet.service';
import { ConfirmDeleteDialog } from '../../../shared/confirm-delete-dialog/confirm-delete-dialog';
import { MonthPickerSheet } from '../../../shared/month-picker-sheet/month-picker-sheet';
import { buildCategory } from '../../testing/category-fixture';
import { Category } from '../../types/category';
import { CATEGORY_SORT_OPTIONS } from '../../types/category-sort';
import { CategoryBlockedDialog } from '../category-blocked-dialog/category-blocked-dialog';
import { CategoryFiltersSheet } from '../category-filters-sheet/category-filters-sheet';
import { CategoryFormSheet } from '../category-form-sheet/category-form-sheet';
import { CategoryMenuSheet } from '../category-menu-sheet/category-menu-sheet';
import { CategoriesPage } from './categories-page';

registerLocaleData(localePt);

describe('CategoriesPage', () => {
	const baseUrl = `${environment.apiUrl}/Categories`;
	const mercado = buildCategory({ id: 'mercado', name: 'Mercado', transactionCount: 14, balance: -1842.55 });
	const educacao = buildCategory({ id: 'educacao', name: 'Educação', transactionCount: 0 });

	let fixture: ComponentFixture<CategoriesPage>;
	let element: HTMLElement;
	let httpTesting: HttpTestingController;
	let bottomSheet: { open: ReturnType<typeof vi.fn> };
	let dialog: { open: ReturnType<typeof vi.fn> };
	let snackBar: { open: ReturnType<typeof vi.fn> };
	let navigate: ReturnType<typeof vi.spyOn>;

	beforeEach(async () => {
		bottomSheet = { open: vi.fn() };
		dialog = { open: vi.fn() };
		snackBar = { open: vi.fn() };

		await TestBed.configureTestingModule({
			imports: [CategoriesPage],
			providers: [
				{ provide: LOCALE_ID, useValue: 'pt-BR' },
				provideRouter([]),
				provideHttpClient(),
				provideHttpClientTesting(),
				{ provide: BottomSheetService, useValue: bottomSheet },
				{ provide: MatDialog, useValue: dialog },
				{ provide: MatSnackBar, useValue: snackBar },
			],
		}).compileComponents();

		httpTesting = TestBed.inject(HttpTestingController);
		navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
		fixture = TestBed.createComponent(CategoriesPage);
		element = fixture.nativeElement;
	});

	afterEach(() => httpTesting.verify());

	function expectList(): TestRequest {
		TestBed.tick();
		return httpTesting.expectOne((req) => req.method === 'GET' && req.url === baseUrl);
	}

	async function flushList(categories: Category[]) {
		expectList().flush(categories);
		await fixture.whenStable();
	}

	function renderedNames() {
		return Array.from(element.querySelectorAll('.card .name')).map((n) => n.textContent?.trim());
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

	function dialogReturns(result: unknown) {
		dialog.open.mockReturnValue({ afterClosed: () => of(result) });
	}

	describe('loading', () => {
		it('should request the categories sorted by name when opened', () => {
			const { params } = expectList().request;

			expect(params.get('orderBy')).toBe('Name');
			expect(params.get('asc')).toBe('true');
			expect(params.has('name')).toBe(false);
			expect(params.has('withTransaction')).toBe(false);
			expect(params.has('dateRef')).toBe(false);
		});

		it('should show a progress bar while loading', () => {
			const req = expectList();
			fixture.detectChanges();

			expect(element.querySelector('mat-progress-bar')).not.toBeNull();
			expect(element.querySelector('app-empty-state')).toBeNull();
			req.flush([]);
		});

		it('should render the categories returned by the API', async () => {
			await flushList([mercado, educacao]);

			expect(renderedNames()).toEqual(['Mercado', 'Educação']);
			expect(element.querySelector('mat-progress-bar')).toBeNull();
		});

		it('should show the empty state when there are no categories', async () => {
			await flushList([]);

			expect(element.querySelector('app-empty-state h2')?.textContent).toBe('Nenhuma categoria encontrada');
		});

		it('should open the create form from the empty state', async () => {
			await flushList([]);

			element.querySelector<HTMLButtonElement>('app-empty-state button')!.click();

			expect(lastSheet(CategoryFormSheet).data).toEqual({ category: undefined });
		});

		it('should show an error state and retry', async () => {
			expectList().flush(null, { status: 500, statusText: 'Server Error' });
			await fixture.whenStable();

			expect(element.querySelector('app-empty-state h2')?.textContent).toBe('Não foi possível carregar as categorias');

			element.querySelector<HTMLButtonElement>('app-empty-state button')!.click();
			await flushList([mercado]);

			expect(renderedNames()).toEqual(['Mercado']);
		});
	});

	describe('filters', () => {
		beforeEach(() => flushList([mercado, educacao]));

		it('should open the filters sheet with the current state', () => {
			emitFrom('app-page-header', 'openFilters');

			expect(lastSheet(CategoryFiltersSheet).data).toEqual({
				filters: { name: '', withTransaction: null },
				sort: CATEGORY_SORT_OPTIONS[0],
				monthRef: null,
			});
		});

		it('should reload with the applied filters and sort and show chips', async () => {
			emitFrom('app-page-header', 'openFilters');
			const sheet = lastSheet(CategoryFiltersSheet).answer({
				filters: { name: 'mer', withTransaction: true },
				sort: { orderBy: 'Balance', ascending: false },
			});

			expect(sheet.close).toHaveBeenCalled();
			const req = expectList();
			expect(req.request.params.get('name')).toBe('mer');
			expect(req.request.params.get('withTransaction')).toBe('true');
			expect(req.request.params.get('orderBy')).toBe('Balance');
			expect(req.request.params.get('asc')).toBe('false');
			req.flush([mercado]);
			await fixture.whenStable();

			const header = fixture.debugElement.query(By.css('app-page-header')).componentInstance;
			expect(header.chips()).toEqual(['"mer"', 'Com transações']);
			expect(header.filtersActive()).toBe(true);
		});

		it('should reload without a filter when its chip is removed', async () => {
			emitFrom('app-page-header', 'openFilters');
			lastSheet(CategoryFiltersSheet).answer({
				filters: { name: 'mer', withTransaction: false },
				sort: CATEGORY_SORT_OPTIONS[0],
			});
			await flushList([]);

			emitFrom('app-page-header', 'removeChip', 0);

			const req = expectList();
			expect(req.request.params.has('name')).toBe(false);
			expect(req.request.params.get('withTransaction')).toBe('false');
			req.flush([]);
		});

		it('should reload for the month picked and send it as dateRef', async () => {
			emitFrom('app-page-header', 'openMonth');
			const sheet = lastSheet(MonthPickerSheet).answer({ month: 8, year: 2026 });

			expect(sheet.close).toHaveBeenCalled();
			const req = expectList();
			expect(req.request.params.get('dateRef')).toBe('2026-09-01');
			req.flush([]);
			await fixture.whenStable();

			const header = fixture.debugElement.query(By.css('app-page-header')).componentInstance;
			expect(header.monthLabel()).toBe('setembro 2026');
		});

		it('should drop dateRef when the month is cleared', async () => {
			emitFrom('app-page-header', 'openMonth');
			lastSheet(MonthPickerSheet).answer({ month: 8, year: 2026 });
			await flushList([]);

			emitFrom('app-page-header', 'openMonth');
			expect(lastSheet(MonthPickerSheet).data).toEqual({ month: 8, year: 2026, clearable: true });
			lastSheet(MonthPickerSheet).answer(null);

			expect(expectList().request.params.has('dateRef')).toBe(false);
		});
	});

	describe('saving', () => {
		beforeEach(() => flushList([mercado, educacao]));

		it('should create a category and reload the list', async () => {
			emitFrom('app-page-header', 'action');
			const sheet = lastSheet(CategoryFormSheet).answer({ type: 'save', result: { name: 'Pets', color: '#123456' } });

			expect(sheet.close).toHaveBeenCalled();
			const req = httpTesting.expectOne(baseUrl);
			expect(req.request.method).toBe('POST');
			expect(req.request.body).toEqual({ name: 'Pets', color: '#123456' });
			req.flush(buildCategory({ id: 'pets', name: 'Pets' }), { status: 201, statusText: 'Created' });

			expect(snackBar.open).toHaveBeenCalledWith('Categoria criada.', undefined, { duration: 3000 });
			await flushList([mercado, educacao, buildCategory({ id: 'pets', name: 'Pets' })]);
			expect(renderedNames()).toContain('Pets');
		});

		it('should open the edit form when a category is clicked and update it', async () => {
			emitFrom('app-category-list', 'openCategory', mercado);
			expect(lastSheet(CategoryFormSheet).data).toEqual({ category: mercado });

			lastSheet(CategoryFormSheet).answer({
				type: 'save',
				result: { id: mercado.id, name: 'Feira', color: mercado.color },
			});

			const req = httpTesting.expectOne(`${baseUrl}/${mercado.id}`);
			expect(req.request.method).toBe('PUT');
			expect(req.request.body).toEqual({ name: 'Feira', color: mercado.color });
			req.flush({ ...mercado, name: 'Feira' });

			expect(snackBar.open).toHaveBeenCalledWith('Categoria salva.', undefined, { duration: 3000 });
			await flushList([{ ...mercado, name: 'Feira' }, educacao]);
		});

		it('should show the API error detail and not reload when saving fails', () => {
			emitFrom('app-page-header', 'action');
			lastSheet(CategoryFormSheet).answer({ type: 'save', result: { name: '', color: 'zzz' } });

			httpTesting
				.expectOne(baseUrl)
				.flush({ status: 400, title: 'Bad Request', detail: 'Cor inválida.' }, { status: 400, statusText: 'Bad Request' });

			expect(snackBar.open).toHaveBeenCalledWith('Cor inválida.', 'Fechar', { duration: 5000 });
			TestBed.tick();
			httpTesting.expectNone(baseUrl);
		});

		it('should show a fallback message when the error has no detail', () => {
			emitFrom('app-category-list', 'openCategory', mercado);
			lastSheet(CategoryFormSheet).answer({ type: 'save', result: { id: mercado.id, name: 'X', color: '#000000' } });

			httpTesting.expectOne(`${baseUrl}/${mercado.id}`).flush(null, { status: 500, statusText: 'Server Error' });

			expect(snackBar.open).toHaveBeenCalledWith('Não foi possível salvar a categoria.', 'Fechar', { duration: 5000 });
		});
	});

	describe('menu', () => {
		beforeEach(() => flushList([mercado, educacao]));

		it('should open the menu for the category', () => {
			emitFrom('app-category-list', 'openMenu', mercado);

			expect(lastSheet(CategoryMenuSheet).data).toBe(mercado);
		});

		it('should open the edit form from the menu', () => {
			emitFrom('app-category-list', 'openMenu', mercado);
			const sheet = lastSheet(CategoryMenuSheet).answer('edit');

			expect(sheet.close).toHaveBeenCalled();
			expect(lastSheet(CategoryFormSheet).data).toEqual({ category: mercado });
		});

		it('should navigate to the category transactions from the menu', () => {
			emitFrom('app-category-list', 'openMenu', mercado);
			lastSheet(CategoryMenuSheet).answer('view-transactions');

			expect(navigate).toHaveBeenCalledWith(['/transactions'], { queryParams: { categoryId: mercado.id } });
		});
	});

	describe('deleting', () => {
		beforeEach(() => flushList([mercado, educacao]));

		function deleteFromMenu(category: Category) {
			emitFrom('app-category-list', 'openMenu', category);
			lastSheet(CategoryMenuSheet).answer('delete');
		}

		it('should block deleting a category with transactions', () => {
			dialogReturns(undefined);
			deleteFromMenu(mercado);

			expect(dialog.open).toHaveBeenCalledWith(CategoryBlockedDialog, expect.objectContaining({
				data: { categoryName: 'Mercado', transactionCount: 14 },
			}));
			expect(navigate).not.toHaveBeenCalled();
			httpTesting.expectNone(`${baseUrl}/${mercado.id}`);
		});

		it('should navigate to the transactions from the blocked dialog', () => {
			dialogReturns('view-transactions');
			deleteFromMenu(mercado);

			expect(navigate).toHaveBeenCalledWith(['/transactions'], { queryParams: { categoryId: mercado.id } });
		});

		it('should do nothing when the deletion is not confirmed', () => {
			dialogReturns(false);
			deleteFromMenu(educacao);

			expect(dialog.open).toHaveBeenCalledWith(ConfirmDeleteDialog, expect.objectContaining({
				data: expect.objectContaining({ title: 'Excluir "Educação"?' }),
			}));
			httpTesting.expectNone(`${baseUrl}/${educacao.id}`);
			expect(snackBar.open).not.toHaveBeenCalled();
		});

		it('should delete a confirmed category and reload the list', async () => {
			dialogReturns(true);
			deleteFromMenu(educacao);

			const req = httpTesting.expectOne(`${baseUrl}/${educacao.id}`);
			expect(req.request.method).toBe('DELETE');
			req.flush(null, { status: 204, statusText: 'No Content' });

			expect(snackBar.open).toHaveBeenCalledWith('Categoria excluída.', undefined, { duration: 3000 });
			await flushList([mercado]);
			expect(renderedNames()).toEqual(['Mercado']);
		});

		it('should delete from the edit form too', () => {
			dialogReturns(true);
			emitFrom('app-category-list', 'openCategory', educacao);
			lastSheet(CategoryFormSheet).answer({ type: 'delete', category: educacao });

			httpTesting.expectOne(`${baseUrl}/${educacao.id}`).flush(null, { status: 204, statusText: 'No Content' });
			expectList().flush([mercado]);
		});

		it('should show the API error when the backend refuses the deletion', () => {
			dialogReturns(true);
			deleteFromMenu(educacao);

			httpTesting
				.expectOne(`${baseUrl}/${educacao.id}`)
				.flush(
					{ status: 400, detail: 'A categoria possui transações vinculadas.' },
					{ status: 400, statusText: 'Bad Request' },
				);

			expect(snackBar.open).toHaveBeenCalledWith('A categoria possui transações vinculadas.', 'Fechar', {
				duration: 5000,
			});
			TestBed.tick();
			httpTesting.expectNone(baseUrl);
		});
	});
});
