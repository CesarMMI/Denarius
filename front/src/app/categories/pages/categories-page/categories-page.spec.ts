import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { LOCALE_ID } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDatepicker } from '@angular/material/datepicker';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltip } from '@angular/material/tooltip';
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
		canDelete: false,
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

	function tooltip(target: HTMLElement) {
		return fixture.debugElement
			.queryAll(By.directive(MatTooltip))
			.find((el) => el.nativeElement === target)!
			.injector.get(MatTooltip).message;
	}

	function expectNoListRequest() {
		TestBed.tick();
		httpTesting.expectNone((req) => req.method === 'GET' && req.url === baseUrl);
	}

	function failWithoutResponse(req: TestRequest) {
		req.error(new ProgressEvent('error'), { status: 0, statusText: '' });
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

		it('should keep the rows, without a spinner, while reloading from the header', async () => {
			await flushList([mercado, educacao]);

			headerButton('Recarregar')!.click();
			const req = expectList();
			fixture.detectChanges();

			expect(column('name')).toEqual(['Mercado', 'Educação']);
			expect(spinner()).toBeNull();

			req.flush([mercado, educacao, salario]);
			await fixture.whenStable();
			expect(column('name')).toEqual(['Mercado', 'Educação', 'Salário']);
		});

		it('should keep the rows, without a spinner, while reloading after saving', async () => {
			await flushList([mercado, educacao]);
			dialogReturns({ name: 'Pets', color: '#123456' });
			button('Nova categoria').click();
			httpTesting
				.expectOne(baseUrl)
				.flush(buildCategory({ id: 'pets', name: 'Pets' }), { status: 201, statusText: 'Created' });

			const req = expectList();
			fixture.detectChanges();

			expect(column('name')).toEqual(['Mercado', 'Educação']);
			expect(spinner()).toBeNull();

			req.flush([mercado, educacao, buildCategory({ id: 'pets', name: 'Pets' })]);
			await fixture.whenStable();
			expect(column('name')).toEqual(['Mercado', 'Educação', 'Pets']);
		});

		it('should be titled "Categorias", with "Recarregar" as the reload tooltip', async () => {
			await flushList([mercado]);

			expect(element.querySelector('app-page-header h1')?.textContent?.trim()).toBe('Categorias');
			expect(tooltip(headerButton('Recarregar')!)).toBe('Recarregar');
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

		it('should keep the filters, without reloading, while they are hidden', async () => {
			const chosen = { name: 'mer', withTransaction: true, month: new Date(2026, 8, 1) };
			headerButton('Exibir filtros')!.click();
			await fixture.whenStable();
			filters()!.filters.set(chosen);
			await flushList([mercado]);

			headerButton('Ocultar filtros')!.click();
			fixture.detectChanges();
			expectNoListRequest();

			headerButton('Exibir filtros')!.click();
			await fixture.whenStable();
			expectNoListRequest();

			expect(filters()!.filters()).toEqual(chosen);
			expect(element.querySelector<HTMLInputElement>('app-categories-filters input[matInput]')!.value).toBe('mer');
			expect(element.querySelector('app-categories-filters mat-select')!.textContent).toContain('Com transações');
			expect(column('name')).toEqual(['Mercado']);
		});

		describe('when nothing changes', () => {
			beforeEach(async () => {
				headerButton('Exibir filtros')!.click();
				await fixture.whenStable();
			});

			function nameInput() {
				return element.querySelector<HTMLInputElement>('app-categories-filters input[matInput]')!;
			}

			/** Typed in the DOM: a harness would wait on the request the typing may send. */
			function typeName(text: string) {
				nameInput().value = text;
				nameInput().dispatchEvent(new Event('input'));
			}

			function pause() {
				return new Promise((resolve) => setTimeout(resolve, 300));
			}

			function pickMonth(month: Date) {
				const datepicker: MatDatepicker<Date> = fixture.debugElement.query(By.directive(MatDatepicker)).componentInstance;
				datepicker.monthSelected.emit(month);
			}

			async function applyName(text: string) {
				typeName(text);
				await pause();
				const req = expectList();
				expect(req.request.params.get('name')).toBe(text);
				req.flush([mercado]);
				await fixture.whenStable();
			}

			it('should not reload when the name is typed again as the text applied', async () => {
				await applyName('mer');

				typeName('me');
				typeName('mer');
				await pause();

				expectNoListRequest();
			});

			it('should not reload when leaving the name field after the text applied did not change', async () => {
				await applyName('mer');
				typeName('me');
				typeName('mer');
				await pause();
				// Answers what the typing may have sent; the test above covers it.
				TestBed.tick();
				httpTesting.match((req) => req.method === 'GET' && req.url === baseUrl).forEach((req) => req.flush([mercado]));

				nameInput().dispatchEvent(new Event('blur'));

				expectNoListRequest();
			});

			it('should not reload when the month in use is picked again', async () => {
				pickMonth(new Date(2026, 8, 1));
				const req = expectList();
				expect(req.request.params.get('dateRef')).toBe('2026-09-01');
				req.flush([mercado]);
				await fixture.whenStable();

				pickMonth(new Date(2026, 8, 1));

				expectNoListRequest();
			});
		});

		it('should say "Exibir filtros" or "Ocultar filtros" in the tooltip of the filters button', async () => {
			expect(tooltip(headerButton('Exibir filtros')!)).toBe('Exibir filtros');

			headerButton('Exibir filtros')!.click();
			await fixture.whenStable();

			expect(tooltip(headerButton('Ocultar filtros')!)).toBe('Ocultar filtros');
		});
	});

	it('should reload sorted by the option chosen in the sort menu', async () => {
		await flushList([mercado, educacao]);
		expect(headerButton('Ordenar: Nome (A–Z)')).not.toBeNull();

		await sortBy('Menor saldo primeiro');

		expect(expectList().request.params.toString()).toBe('orderBy=balance&asc=true');
	});

	it('should not reload when the sort in use is chosen again', async () => {
		await flushList([mercado, educacao]);

		await sortBy('Nome (A–Z)');

		expectNoListRequest();
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
			dialogReturns({ name: '   ', color: '#43A047' });
			button('Nova categoria').click();

			httpTesting
				.expectOne(baseUrl)
				.flush(
					{ status: 400, title: 'Bad Request', detail: 'O nome da categoria não pode ser vazio.' },
					{ status: 400, statusText: 'Bad Request' },
				);

			expect(snackBar.open).toHaveBeenCalledWith('O nome da categoria não pode ser vazio.', 'Fechar', { duration: 5000 });
			TestBed.tick();
			httpTesting.expectNone(baseUrl);
		});

		it('should show a fallback message when the error has no detail', () => {
			dialogReturns({ name: 'X', color: '#000000' });
			rowButton(0, 'Editar').click();

			failWithoutResponse(httpTesting.expectOne(`${baseUrl}/${mercado.id}`));

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
			// The list is stale: the category gained transactions after it loaded.
			rowButton(1, 'Excluir').click();

			httpTesting
				.expectOne(`${baseUrl}/${educacao.id}`)
				.flush(
					{ status: 400, detail: 'Não é possível excluir uma categoria que possui transações associadas.' },
					{ status: 400, statusText: 'Bad Request' },
				);

			expect(snackBar.open).toHaveBeenCalledWith(
				'Não é possível excluir uma categoria que possui transações associadas.',
				'Fechar',
				{ duration: 5000 },
			);
			TestBed.tick();
			httpTesting.expectNone(baseUrl);
		});

		it('should show a fallback message when deleting fails without a detail', () => {
			failWithoutResponse(deleteEducacao());

			expect(snackBar.open).toHaveBeenCalledWith('Não foi possível excluir a categoria.', 'Fechar', {
				duration: 5000,
			});
			expectNoListRequest();
		});

		describe('when undoing fails', () => {
			async function undoDeletion() {
				deleteEducacao().flush(null, { status: 204, statusText: 'No Content' });
				await flushList([mercado]);
				snackBar.open.mockClear();

				snackBarAction.next();

				const req = httpTesting.expectOne(baseUrl);
				expect(req.request.method).toBe('POST');
				return req;
			}

			it('should show the API error detail and not reload', async () => {
				(await undoDeletion()).flush(
					{ status: 500, title: 'Internal Server Error', detail: 'Ocorreu um erro inesperado.' },
					{ status: 500, statusText: 'Internal Server Error' },
				);

				expect(snackBar.open).toHaveBeenCalledExactlyOnceWith('Ocorreu um erro inesperado.', 'Fechar', {
					duration: 5000,
				});
				expectNoListRequest();
			});

			it('should show a fallback message on a network error and not reload', async () => {
				failWithoutResponse(await undoDeletion());

				expect(snackBar.open).toHaveBeenCalledExactlyOnceWith('Não foi possível salvar a categoria.', 'Fechar', {
					duration: 5000,
				});
				expectNoListRequest();
			});
		});
	});

	describe('deleting, while in progress', () => {
		const pets = buildCategory({ id: 'pets', name: 'Pets', canDelete: true });
		const noContent = { status: 204, statusText: 'No Content' };

		beforeEach(() => flushList([mercado, educacao, pets]));

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
			rowButton(1, 'Excluir').click();
			fixture.detectChanges();
			rowButton(1, 'Excluir').click();

			const requests = deleteRequests(educacao.id);
			expect(requests).toHaveLength(1);

			requests[0].flush(null, noContent);
			await flushList([mercado, pets]);
		});

		it('should not send another DELETE after the deletion is accepted and before the list reloads', async () => {
			rowButton(1, 'Excluir').click();
			deleteRequests(educacao.id)[0].flush(null, noContent);
			const reload = expectList();
			fixture.detectChanges();

			rowButton(1, 'Excluir').click();

			expect(deleteRequests(educacao.id)).toHaveLength(0);
			expect(snackBar.open).toHaveBeenCalledExactlyOnceWith('Categoria excluída.', 'Desfazer', { duration: 5000 });
			reload.flush([mercado, pets]);
			await fixture.whenStable();
			expect(column('name')).toEqual(['Mercado', 'Pets']);
		});

		it('should disable "Excluir" and show a spinner in the row while it is being deleted', async () => {
			rowButton(1, 'Excluir').click();
			const request = deleteRequests(educacao.id)[0];
			fixture.detectChanges();

			expect(isDisabled(rowButton(1, 'Excluir'))).toBe(true);
			expect(rowSpinner(1)).not.toBeNull();

			request.flush(null, noContent);
			await flushList([mercado, pets]);
		});

		it('should keep "Excluir" of the other rows enabled, without a spinner, while a row is being deleted', async () => {
			rowButton(1, 'Excluir').click();
			const educacaoRequest = deleteRequests(educacao.id)[0];
			fixture.detectChanges();

			expect(isDisabled(rowButton(2, 'Excluir'))).toBe(false);
			expect(rowSpinner(2)).toBeNull();

			rowButton(2, 'Excluir').click();
			const petsRequests = deleteRequests(pets.id);
			expect(petsRequests).toHaveLength(1);

			educacaoRequest.flush(null, noContent);
			petsRequests[0].flush({ status: 400, detail: 'Recusada.' }, { status: 400, statusText: 'Bad Request' });
			await flushList([mercado, pets]);
		});

		it('should enable "Excluir" again when the deletion is refused', async () => {
			rowButton(1, 'Excluir').click();
			deleteRequests(educacao.id)[0].flush(
				{ status: 400, detail: 'Não é possível excluir uma categoria que possui transações associadas.' },
				{ status: 400, statusText: 'Bad Request' },
			);
			await fixture.whenStable();

			expect(isDisabled(rowButton(1, 'Excluir'))).toBe(false);
			expect(rowSpinner(1)).toBeNull();

			rowButton(1, 'Excluir').click();
			const again = deleteRequests(educacao.id);
			expect(again).toHaveLength(1);
			again[0].flush(null, noContent);
			await flushList([mercado, pets]);
		});

		it('should disable "Editar" while the row is being deleted', async () => {
			dialogReturns(undefined);
			rowButton(1, 'Excluir').click();
			const request = deleteRequests(educacao.id)[0];
			fixture.detectChanges();

			expect(isDisabled(rowButton(1, 'Editar'))).toBe(true);
			rowButton(1, 'Editar').click();
			expect(dialog.open).not.toHaveBeenCalled();

			request.flush(null, noContent);
			const reload = expectList();
			fixture.detectChanges();

			expect(isDisabled(rowButton(1, 'Editar'))).toBe(true);
			rowButton(1, 'Editar').click();
			expect(dialog.open).not.toHaveBeenCalled();
			expect(isDisabled(rowButton(2, 'Editar'))).toBe(false);

			reload.flush([mercado, pets]);
			await fixture.whenStable();
		});

		describe('restoring', () => {
			async function undoDeletion() {
				rowButton(1, 'Excluir').click();
				deleteRequests(educacao.id)[0].flush(null, noContent);
				await flushList([mercado, pets]);
				expect(progressBar()).toBeNull();

				snackBarAction.next();
				return httpTesting.expectOne((req) => req.method === 'POST' && req.url === baseUrl);
			}

			it('should show that the restoration is in progress', async () => {
				const request = await undoDeletion();
				fixture.detectChanges();

				expect(progressBar()).not.toBeNull();
				expect(progressBar()!.getAttribute('aria-label')).toBe('Restaurando a categoria');

				request.flush({ ...educacao, id: 'educacao-2' }, { status: 201, statusText: 'Created' });
				await flushList([mercado, { ...educacao, id: 'educacao-2' }, pets]);
			});

			it('should hide the progress once the restoration is accepted', async () => {
				const request = await undoDeletion();

				request.flush({ ...educacao, id: 'educacao-2' }, { status: 201, statusText: 'Created' });
				const reload = expectList();
				fixture.detectChanges();
				expect(progressBar()).toBeNull();

				reload.flush([mercado, { ...educacao, id: 'educacao-2' }, pets]);
				await fixture.whenStable();
				expect(column('name')).toEqual(['Mercado', 'Educação', 'Pets']);
			});

			it('should hide the progress once the restoration is refused', async () => {
				const request = await undoDeletion();

				request.flush({ status: 500, detail: 'Ocorreu um erro inesperado.' }, { status: 500, statusText: 'Error' });
				await fixture.whenStable();

				expect(progressBar()).toBeNull();
				expectNoListRequest();
			});
		});

		describe('when the list is already loading', () => {
			it('should reload again when a deletion is accepted during a reload', async () => {
				rowButton(1, 'Excluir').click();
				deleteRequests(educacao.id)[0].flush(null, noContent);
				const firstReload = expectList();

				rowButton(2, 'Excluir').click();
				deleteRequests(pets.id)[0].flush(null, noContent);

				// Answered before the API saw Pets go.
				firstReload.flush([mercado, pets]);
				await settle();
				const secondReload = expectList();
				fixture.detectChanges();

				expect(column('name')).toEqual(['Mercado', 'Pets']);
				expect(isDisabled(rowButton(1, 'Excluir'))).toBe(true);
				rowButton(1, 'Excluir').click();
				expect(deleteRequests(pets.id)).toHaveLength(0);

				secondReload.flush([mercado]);
				await fixture.whenStable();
				expect(column('name')).toEqual(['Mercado']);
				expectNoListRequest();
			});

			it('should reload again when a deletion is accepted while the filters load the list', async () => {
				headerButton('Exibir filtros')!.click();
				await fixture.whenStable();

				rowButton(1, 'Excluir').click();
				const request = deleteRequests(educacao.id)[0];
				filters()!.filters.set({ name: 'e', withTransaction: '', month: null });
				const filtered = expectList();

				request.flush(null, noContent);
				filtered.flush([educacao, pets]);
				await settle();
				const reload = expectList();
				expect(reload.request.params.get('name')).toBe('e');

				reload.flush([pets]);
				await fixture.whenStable();
				expect(column('name')).toEqual(['Pets']);
				expectNoListRequest();
			});
		});
	});
});
