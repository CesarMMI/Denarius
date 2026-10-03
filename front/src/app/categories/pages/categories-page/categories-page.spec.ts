import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { LOCALE_ID } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { of, Subject } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CategoriesFilters } from '../../components/categories-filters/categories-filters';
import { CategoryForm } from '../../components/category-form/category-form';
import { buildCategory } from '../../testing/category-fixture';
import { Category } from '../../types/category';
import { CategoriesPage } from './categories-page';

registerLocaleData(localePt);

describe('CategoriesPage', () => {
	const baseUrl = `${environment.apiUrl}/categories`;
	const mercado = buildCategory({
		id: 'mercado',
		name: 'Mercado',
		color: '#43A047',
		transactionCount: 14,
		balance: -1842.55,
	});
	const educacao = buildCategory({ id: 'educacao', name: 'Educação', transactionCount: 0, balance: 0 });
	const salario = buildCategory({ id: 'salario', name: 'Salário', transactionCount: 1, balance: 8600 });

	let fixture: ComponentFixture<CategoriesPage>;
	let element: HTMLElement;
	let httpTesting: HttpTestingController;
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

	function filters(): CategoriesFilters | undefined {
		return fixture.debugElement.query(By.directive(CategoriesFilters))?.componentInstance;
	}

	function dialogReturns(result: unknown) {
		dialog.open.mockReturnValue({ afterClosed: () => of(result) });
	}

	describe('loading', () => {
		it('should request the categories sorted by name when opened', () => {
			expect(expectList().request.params.toString()).toBe('orderBy=name&asc=true');
		});

		it('should show a spinner while loading', () => {
			const req = expectList();
			fixture.detectChanges();

			expect(spinner()).not.toBeNull();
			req.flush([]);
		});

		it('should list the categories in the API order', async () => {
			await flushList([mercado, educacao, salario]);

			expect(column('name')).toEqual(['Mercado', 'Educação', 'Salário']);
			expect(spinner()).toBeNull();
		});

		it('should show an error and reload from the header', async () => {
			expectList().flush(null, { status: 500, statusText: 'Server Error' });
			await fixture.whenStable();

			expect(noDataRow()).toBe('Não foi possível carregar as categorias.');

			headerButton('Recarregar')!.click();
			await flushList([mercado]);

			expect(column('name')).toEqual(['Mercado']);
		});
	});

	describe('filters', () => {
		beforeEach(() => flushList([mercado, educacao]));

		it('should be hidden until shown from the header', async () => {
			expect(filters()).toBeUndefined();

			headerButton('Exibir filtros')!.click();
			await fixture.whenStable();
			expect(filters()).toBeDefined();

			headerButton('Ocultar filtros')!.click();
			await fixture.whenStable();
			expect(filters()).toBeUndefined();
		});

		it('should reload with the filters chosen', async () => {
			headerButton('Exibir filtros')!.click();
			await fixture.whenStable();

			filters()!.filters.set({ name: 'mer', withTransaction: true, month: new Date(2026, 8, 1) });

			expect(expectList().request.params.toString()).toBe(
				'name=mer&withTransaction=true&dateRef=2026-09-01&orderBy=name&asc=true',
			);
		});
	});

	it('should reload sorted by the option chosen in the sort menu', async () => {
		await flushList([mercado, educacao]);
		expect(headerButton('Ordenar: Nome (A–Z)')).not.toBeNull();

		await sortBy('Menor saldo primeiro');

		expect(expectList().request.params.toString()).toBe('orderBy=balance&asc=true');
	});

	describe('saving', () => {
		beforeEach(() => flushList([mercado, educacao]));

		it('should create a category from the form and reload', async () => {
			dialogReturns({ name: 'Pets', color: '#123456' });
			button('Nova categoria').click();

			expect(dialog.open).toHaveBeenCalledWith(CategoryForm, { data: undefined });
			const req = httpTesting.expectOne(baseUrl);
			expect(req.request.method).toBe('POST');
			expect(req.request.body).toEqual({ name: 'Pets', color: '#123456' });
			req.flush(buildCategory({ id: 'pets', name: 'Pets' }), { status: 201, statusText: 'Created' });

			expect(snackBar.open).toHaveBeenCalledWith('Categoria criada.', undefined, { duration: 3000 });
			await flushList([mercado, educacao, buildCategory({ id: 'pets', name: 'Pets' })]);
			expect(column('name')).toContain('Pets');
		});

		it('should update the category edited and reload', async () => {
			dialogReturns({ name: 'Feira', color: mercado.color });
			rowButton(0, 'Editar').click();

			expect(dialog.open).toHaveBeenCalledWith(CategoryForm, { data: mercado });
			const req = httpTesting.expectOne(`${baseUrl}/${mercado.id}`);
			expect(req.request.method).toBe('PUT');
			expect(req.request.body).toEqual({ name: 'Feira', color: mercado.color });
			req.flush({ ...mercado, name: 'Feira' });

			expect(snackBar.open).toHaveBeenCalledWith('Categoria salva.', undefined, { duration: 3000 });
			await flushList([{ ...mercado, name: 'Feira' }, educacao]);
		});

		it('should do nothing when the form is cancelled', () => {
			dialogReturns(undefined);
			button('Nova categoria').click();

			TestBed.tick();
			httpTesting.expectNone(baseUrl);
			expect(snackBar.open).not.toHaveBeenCalled();
		});

		it('should show the API error detail and not reload when saving fails', () => {
			dialogReturns({ name: 'Mercado', color: '#43A047' });
			button('Nova categoria').click();

			httpTesting
				.expectOne(baseUrl)
				.flush(
					{ status: 409, title: 'Conflict', detail: 'Já existe uma categoria com esse nome.' },
					{ status: 409, statusText: 'Conflict' },
				);

			expect(snackBar.open).toHaveBeenCalledWith('Já existe uma categoria com esse nome.', 'Fechar', { duration: 5000 });
			TestBed.tick();
			httpTesting.expectNone(baseUrl);
		});

		it('should show a fallback message when the error has no detail', () => {
			dialogReturns({ name: 'X', color: '#000000' });
			rowButton(0, 'Editar').click();

			httpTesting.expectOne(`${baseUrl}/${mercado.id}`).flush(null, { status: 500, statusText: 'Server Error' });

			expect(snackBar.open).toHaveBeenCalledWith('Não foi possível salvar a categoria.', 'Fechar', { duration: 5000 });
		});
	});

	describe('deleting', () => {
		beforeEach(() => flushList([mercado, educacao]));

		function deleteEducacao() {
			rowButton(1, 'Excluir').click();
			const req = httpTesting.expectOne(`${baseUrl}/${educacao.id}`);
			expect(req.request.method).toBe('DELETE');
			return req;
		}

		it('should delete the category, reload and offer to undo', async () => {
			deleteEducacao().flush(null, { status: 204, statusText: 'No Content' });

			expect(snackBar.open).toHaveBeenCalledWith('Categoria excluída.', 'Desfazer', { duration: 5000 });
			await flushList([mercado]);
			expect(column('name')).toEqual(['Mercado']);
		});

		it('should create the category again when the deletion is undone', async () => {
			deleteEducacao().flush(null, { status: 204, statusText: 'No Content' });
			await flushList([mercado]);

			snackBarAction.next();

			const req = httpTesting.expectOne(baseUrl);
			expect(req.request.method).toBe('POST');
			expect(req.request.body).toEqual({ name: educacao.name, color: educacao.color });
			req.flush({ ...educacao, id: 'educacao-2' }, { status: 201, statusText: 'Created' });

			expect(snackBar.open).toHaveBeenCalledWith('Categoria restaurada.', undefined, { duration: 3000 });
			await flushList([mercado, { ...educacao, id: 'educacao-2' }]);
		});

		it('should show why the API refuses to delete a category with transactions', () => {
			rowButton(0, 'Excluir').click();

			httpTesting
				.expectOne(`${baseUrl}/${mercado.id}`)
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
