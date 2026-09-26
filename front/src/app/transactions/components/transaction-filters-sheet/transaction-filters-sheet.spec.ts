import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';
import { environment } from '../../../../environments/environment';
import { buildCategory } from '../../../categories/testing/category-fixture';
import { MonthRef } from '../../../shared/types/month-ref';
import { buildTransaction } from '../../testing/transaction-fixture';
import { DEFAULT_TRANSACTION_FILTERS, TransactionFilters } from '../../types/transaction-filters';
import { TRANSACTION_SORT_OPTIONS } from '../../types/transaction-sort';
import { TransactionFiltersData, TransactionFiltersSheet } from './transaction-filters-sheet';

describe('TransactionFiltersSheet', () => {
	const baseUrl = `${environment.apiUrl}/Transactions`;
	const categories = [
		buildCategory({ id: 'mercado', name: 'Mercado', color: '#43A047' }),
		buildCategory({ id: 'salario', name: 'Salário', color: '#1E88E5' }),
	];

	let fixture: ComponentFixture<TransactionFiltersSheet>;
	let element: HTMLElement;
	let httpTesting: HttpTestingController;
	let callback: ReturnType<typeof vi.fn>;

	async function render(data: Partial<TransactionFiltersData> = {}) {
		callback = vi.fn();
		await TestBed.configureTestingModule({
			imports: [TransactionFiltersSheet],
			providers: [
				provideHttpClient(),
				provideHttpClientTesting(),
				{ provide: MatBottomSheetRef, useValue: { dismiss: () => undefined } },
				{
					provide: MAT_BOTTOM_SHEET_DATA,
					useValue: {
						filters: DEFAULT_TRANSACTION_FILTERS,
						sort: TRANSACTION_SORT_OPTIONS[0],
						monthRef: null,
						categories,
						...data,
						callback,
					},
				},
			],
		}).compileComponents();

		httpTesting = TestBed.inject(HttpTestingController);
		fixture = TestBed.createComponent(TransactionFiltersSheet);
		element = fixture.nativeElement;
	}

	function expectPreview(): TestRequest {
		TestBed.tick();
		return httpTesting.expectOne((req) => req.method === 'GET' && req.url === baseUrl);
	}

	async function flushPreview(count: number) {
		expectPreview().flush(Array.from({ length: count }, (_, i) => buildTransaction({ id: `t${i}` })));
		await fixture.whenStable();
	}

	function applyButton() {
		return element.querySelector<HTMLButtonElement>('.apply')!;
	}

	function typeOptions() {
		return Array.from(element.querySelectorAll<HTMLButtonElement>('.option'));
	}

	function categoryChips() {
		return Array.from(element.querySelectorAll<HTMLButtonElement>('.chip'));
	}

	function sortOptions() {
		return Array.from(element.querySelectorAll<HTMLButtonElement>('.sort-option'));
	}

	function typeDescription(value: string) {
		const input = element.querySelector<HTMLInputElement>('.search input')!;
		input.value = value;
		input.dispatchEvent(new Event('input'));
	}

	afterEach(() => httpTesting.verify());

	it('should show a generic label while the preview is loading', async () => {
		await render();
		const req = expectPreview();
		fixture.detectChanges();

		expect(applyButton().textContent?.trim()).toBe('Ver resultados');

		req.flush([]);
		await fixture.whenStable();
	});

	it.each([
		[0, 'Ver 0 resultados'],
		[1, 'Ver 1 resultado'],
		[3, 'Ver 3 resultados'],
	])('should show %i result(s) from the API', async (count, label) => {
		await render();
		await flushPreview(count);

		expect(applyButton().textContent?.trim()).toBe(label);
	});

	it('should preview with the current filters, sort and month', async () => {
		const monthRef: MonthRef = { month: 2, year: 2026 };
		const filters: TransactionFilters = { description: 'feira', type: 'Out', categoryId: 'mercado' };
		await render({ filters, sort: TRANSACTION_SORT_OPTIONS[2], monthRef });

		const { params } = expectPreview().request;
		expect(params.get('description')).toBe('feira');
		expect(params.get('type')).toBe('Out');
		expect(params.get('categoryId')).toBe('mercado');
		expect(params.get('dateRef')).toBe('2026-03-01');
		expect(params.get('orderBy')).toBe('Value');
		expect(params.get('asc')).toBe('false');
	});

	it('should list "Todas" and the given categories', async () => {
		await render();
		await flushPreview(0);

		expect(categoryChips().map((c) => c.textContent?.trim())).toEqual(['Todas', 'Mercado', 'Salário']);
		expect(categoryChips()[0].classList).toContain('selected');
	});

	it('should prefill the draft filters and sort', async () => {
		await render({
			filters: { description: 'feira', type: 'In', categoryId: 'salario' },
			sort: TRANSACTION_SORT_OPTIONS[6],
		});
		await flushPreview(0);

		expect(element.querySelector<HTMLInputElement>('.search input')!.value).toBe('feira');
		expect(typeOptions()[1].classList).toContain('selected');
		expect(categoryChips()[2].classList).toContain('selected');
		expect(sortOptions()[6].classList).toContain('selected');
		expect(sortOptions()[6].querySelector('mat-icon')?.textContent).toBe('check');
	});

	it('should refresh the preview when the description changes', async () => {
		await render();
		await flushPreview(5);

		typeDescription('uber');

		const req = expectPreview();
		expect(req.request.params.get('description')).toBe('uber');
		req.flush([buildTransaction()]);
		await fixture.whenStable();
		expect(applyButton().textContent?.trim()).toBe('Ver 1 resultado');
	});

	it('should refresh the preview when the type changes', async () => {
		await render();
		await flushPreview(5);

		typeOptions()[2].click();

		const req = expectPreview();
		expect(req.request.params.get('type')).toBe('Out');
		req.flush([]);
		await fixture.whenStable();
		expect(typeOptions()[2].classList).toContain('selected');
	});

	it('should refresh the preview when the category changes', async () => {
		await render();
		await flushPreview(5);

		categoryChips()[1].click();

		const req = expectPreview();
		expect(req.request.params.get('categoryId')).toBe('mercado');
		req.flush([]);
		await fixture.whenStable();
		expect(categoryChips()[1].classList).toContain('selected');
	});

	it('should not refresh the preview when only the sort changes', async () => {
		await render();
		await flushPreview(5);

		sortOptions()[3].click();
		await fixture.whenStable();
		TestBed.tick();

		httpTesting.expectNone(baseUrl);
		expect(sortOptions()[3].classList).toContain('selected');
		expect(sortOptions()[0].classList).not.toContain('selected');
	});

	it('should reset the filters but keep the sort when clearing', async () => {
		await render({
			filters: { description: 'feira', type: 'Out', categoryId: 'mercado' },
			sort: TRANSACTION_SORT_OPTIONS[4],
		});
		await flushPreview(1);

		element.querySelector<HTMLButtonElement>('.header button')!.click();
		await flushPreview(10);
		applyButton().click();

		expect(callback).toHaveBeenCalledWith(
			{ filters: DEFAULT_TRANSACTION_FILTERS, sort: TRANSACTION_SORT_OPTIONS[4] },
			expect.any(TransactionFiltersSheet),
		);
	});

	it('should apply the draft filters and sort', async () => {
		await render();
		await flushPreview(5);

		typeDescription('uber');
		await flushPreview(1);
		typeOptions()[2].click();
		await flushPreview(1);
		categoryChips()[1].click();
		await flushPreview(1);
		sortOptions()[7].click();
		applyButton().click();

		expect(callback).toHaveBeenCalledWith(
			{
				filters: { description: 'uber', type: 'Out', categoryId: 'mercado' },
				sort: { orderBy: 'CategoryName', ascending: false },
			},
			expect.any(TransactionFiltersSheet),
		);
	});
});
