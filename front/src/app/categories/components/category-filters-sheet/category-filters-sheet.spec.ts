import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';
import { environment } from '../../../../environments/environment';
import { MonthRef } from '../../../shared/types/month-ref';
import { buildCategory } from '../../testing/category-fixture';
import { CategoryFilters, DEFAULT_CATEGORY_FILTERS } from '../../types/category-filters';
import { CATEGORY_SORT_OPTIONS } from '../../types/category-sort';
import { CategoryFiltersData, CategoryFiltersSheet } from './category-filters-sheet';

describe('CategoryFiltersSheet', () => {
	let fixture: ComponentFixture<CategoryFiltersSheet>;
	let element: HTMLElement;
	let httpTesting: HttpTestingController;
	let callback: ReturnType<typeof vi.fn>;

	async function render(data: Partial<CategoryFiltersData> = {}) {
		callback = vi.fn();
		await TestBed.configureTestingModule({
			imports: [CategoryFiltersSheet],
			providers: [
				provideHttpClient(),
				provideHttpClientTesting(),
				{ provide: MatBottomSheetRef, useValue: { dismiss: () => undefined } },
				{
					provide: MAT_BOTTOM_SHEET_DATA,
					useValue: { filters: DEFAULT_CATEGORY_FILTERS, sort: CATEGORY_SORT_OPTIONS[0], monthRef: null, ...data, callback },
				},
			],
		}).compileComponents();

		httpTesting = TestBed.inject(HttpTestingController);
		fixture = TestBed.createComponent(CategoryFiltersSheet);
		element = fixture.nativeElement;
	}

	function expectPreview(): TestRequest {
		TestBed.tick();
		return httpTesting.expectOne((req) => req.method === 'GET' && req.url === `${environment.apiUrl}/Categories`);
	}

	async function flushPreview(count: number) {
		expectPreview().flush(Array.from({ length: count }, (_, i) => buildCategory({ id: `c${i}` })));
		await fixture.whenStable();
	}

	function applyButton() {
		return element.querySelector<HTMLButtonElement>('.apply')!;
	}

	function transactionOptions() {
		return Array.from(element.querySelectorAll<HTMLButtonElement>('.option'));
	}

	function sortOptions() {
		return Array.from(element.querySelectorAll<HTMLButtonElement>('.sort-option'));
	}

	function typeName(value: string) {
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
		const filters: CategoryFilters = { name: 'mer', withTransaction: true };
		await render({ filters, sort: CATEGORY_SORT_OPTIONS[2], monthRef });

		const { params } = expectPreview().request;
		expect(params.get('name')).toBe('mer');
		expect(params.get('withTransaction')).toBe('true');
		expect(params.get('dateRef')).toBe('2026-03-01');
		expect(params.get('orderBy')).toBe('Balance');
	});

	it('should prefill the draft filters and sort', async () => {
		await render({ filters: { name: 'mer', withTransaction: false }, sort: CATEGORY_SORT_OPTIONS[1] });
		await flushPreview(0);

		expect(element.querySelector<HTMLInputElement>('.search input')!.value).toBe('mer');
		expect(transactionOptions()[2].classList).toContain('selected');
		expect(sortOptions()[1].classList).toContain('selected');
	});

	it('should refresh the preview when the name changes', async () => {
		await render();
		await flushPreview(5);

		typeName('sal');

		const req = expectPreview();
		expect(req.request.params.get('name')).toBe('sal');
		req.flush([buildCategory()]);
		await fixture.whenStable();
		expect(applyButton().textContent?.trim()).toBe('Ver 1 resultado');
	});

	it('should refresh the preview when the transaction filter changes', async () => {
		await render();
		await flushPreview(5);

		transactionOptions()[2].click();

		const req = expectPreview();
		expect(req.request.params.get('withTransaction')).toBe('false');
		req.flush([]);
		await fixture.whenStable();
		expect(transactionOptions()[2].classList).toContain('selected');
	});

	it('should not refresh the preview when only the sort changes', async () => {
		await render();
		await flushPreview(5);

		sortOptions()[3].click();
		await fixture.whenStable();
		TestBed.tick();

		httpTesting.expectNone(`${environment.apiUrl}/Categories`);
		expect(sortOptions()[3].classList).toContain('selected');
		expect(sortOptions()[3].querySelector('mat-icon')?.textContent).toBe('check');
	});

	it('should reset the filters but keep the sort when clearing', async () => {
		await render({ filters: { name: 'mer', withTransaction: true }, sort: CATEGORY_SORT_OPTIONS[4] });
		await flushPreview(1);

		element.querySelector<HTMLButtonElement>('.header button')!.click();
		await flushPreview(10);
		applyButton().click();

		expect(callback).toHaveBeenCalledWith(
			{ filters: DEFAULT_CATEGORY_FILTERS, sort: CATEGORY_SORT_OPTIONS[4] },
			expect.any(CategoryFiltersSheet),
		);
	});

	it('should apply the draft filters and sort', async () => {
		await render();
		await flushPreview(5);

		typeName('sal');
		await flushPreview(1);
		transactionOptions()[1].click();
		await flushPreview(1);
		sortOptions()[2].click();
		applyButton().click();

		expect(callback).toHaveBeenCalledWith(
			{ filters: { name: 'sal', withTransaction: true }, sort: { orderBy: 'Balance', ascending: false } },
			expect.any(CategoryFiltersSheet),
		);
	});
});
