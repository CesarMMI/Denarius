import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { buildCategory } from '../testing/category-fixture';
import { DEFAULT_CATEGORY_FILTERS } from '../types/category-filters';
import { CATEGORY_SORT_FIELD, CATEGORY_SORT_OPTIONS } from '../types/category-sort';
import { CategoriesService } from './categories.service';

describe('CategoriesService', () => {
	const baseUrl = `${environment.apiUrl}/Categories`;

	let service: CategoriesService;
	let httpTesting: HttpTestingController;

	beforeEach(() => {
		TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
		service = TestBed.inject(CategoriesService);
		httpTesting = TestBed.inject(HttpTestingController);
	});

	afterEach(() => httpTesting.verify());

	describe('list', () => {
		it('should target the categories endpoint', () => {
			expect(service.list(DEFAULT_CATEGORY_FILTERS, CATEGORY_SORT_OPTIONS[0]).url).toBe(baseUrl);
		});

		it('should only send sort params when no filter is set', () => {
			const { params } = service.list(DEFAULT_CATEGORY_FILTERS, { orderBy: CATEGORY_SORT_FIELD.Balance, ascending: false });
			expect(params.keys()).toEqual(['orderBy', 'asc']);
			expect(params.get('orderBy')).toBe('Balance');
			expect(params.get('asc')).toBe('false');
		});

		it('should send the name filter', () => {
			const { params } = service.list({ ...DEFAULT_CATEGORY_FILTERS, name: 'merc' }, CATEGORY_SORT_OPTIONS[0]);
			expect(params.get('name')).toBe('merc');
		});

		it.each([true, false])('should send withTransaction=%s', (withTransaction) => {
			const { params } = service.list({ ...DEFAULT_CATEGORY_FILTERS, withTransaction }, CATEGORY_SORT_OPTIONS[0]);
			expect(params.get('withTransaction')).toBe(String(withTransaction));
		});

		it('should send the first day of the selected month as dateRef', () => {
			const { params } = service.list(DEFAULT_CATEGORY_FILTERS, CATEGORY_SORT_OPTIONS[0], { month: 8, year: 2026 });
			expect(params.get('dateRef')).toBe('2026-09-01');
		});

		it('should zero-pad single-digit months in dateRef', () => {
			const { params } = service.list(DEFAULT_CATEGORY_FILTERS, CATEGORY_SORT_OPTIONS[0], { month: 0, year: 2027 });
			expect(params.get('dateRef')).toBe('2027-01-01');
		});

		it('should omit dateRef when no month is selected', () => {
			const { params } = service.list(DEFAULT_CATEGORY_FILTERS, CATEGORY_SORT_OPTIONS[0], null);
			expect(params.has('dateRef')).toBe(false);
		});
	});

	it('getById should GET the category', () => {
		const category = buildCategory();
		let response: unknown;
		service.getById(category.id).subscribe((value) => (response = value));

		const req = httpTesting.expectOne(`${baseUrl}/${category.id}`);
		expect(req.request.method).toBe('GET');
		req.flush(category);
		expect(response).toEqual(category);
	});

	it('create should POST name and color', () => {
		const category = buildCategory();
		let response: unknown;
		service.create({ name: category.name, color: category.color }).subscribe((value) => (response = value));

		const req = httpTesting.expectOne(baseUrl);
		expect(req.request.method).toBe('POST');
		expect(req.request.body).toEqual({ name: category.name, color: category.color });
		req.flush(category, { status: 201, statusText: 'Created' });
		expect(response).toEqual(category);
	});

	it('update should PUT name and color to the category', () => {
		const category = buildCategory({ name: 'Feira' });
		let response: unknown;
		service.update(category.id, { name: 'Feira', color: category.color }).subscribe((value) => (response = value));

		const req = httpTesting.expectOne(`${baseUrl}/${category.id}`);
		expect(req.request.method).toBe('PUT');
		expect(req.request.body).toEqual({ name: 'Feira', color: category.color });
		req.flush(category);
		expect(response).toEqual(category);
	});

	it('delete should DELETE the category', () => {
		const category = buildCategory();
		let completed = false;
		service.delete(category.id).subscribe({ complete: () => (completed = true) });

		const req = httpTesting.expectOne(`${baseUrl}/${category.id}`);
		expect(req.request.method).toBe('DELETE');
		req.flush(null, { status: 204, statusText: 'No Content' });
		expect(completed).toBe(true);
	});
});
