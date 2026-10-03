import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { buildCategory } from '../testing/category-fixture';
import { CategoriesService } from './categories.service';

describe('CategoriesService', () => {
	const baseUrl = `${environment.apiUrl}/categories`;

	let service: CategoriesService;
	let httpTesting: HttpTestingController;

	beforeEach(() => {
		TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
		service = TestBed.inject(CategoriesService);
		httpTesting = TestBed.inject(HttpTestingController);
	});

	afterEach(() => httpTesting.verify());

	describe('list', () => {
		it('should target the categories endpoint without params by default', () => {
			const { url, params } = service.list();
			expect(url).toBe(baseUrl);
			expect(params.keys()).toEqual([]);
		});

		it('should skip empty filters', () => {
			expect(service.list({ name: '', withTransaction: '', month: null }).params.keys()).toEqual([]);
		});

		it('should send the sort as orderBy and asc', () => {
			expect(service.list({}, { active: 'balance', direction: 'desc' }).params.toString()).toBe(
				'orderBy=balance&asc=false',
			);
		});

		it.each([true, false])('should send withTransaction=%s', (withTransaction) => {
			expect(service.list({ withTransaction }).params.get('withTransaction')).toBe(String(withTransaction));
		});

		it('should send every filter together, with the first day of the month as dateRef', () => {
			const { params } = service.list(
				{ name: 'merc', withTransaction: true, month: new Date(2026, 8, 1) },
				{ active: 'name', direction: 'asc' },
			);
			expect(params.toString()).toBe('name=merc&withTransaction=true&dateRef=2026-09-01&orderBy=name&asc=true');
		});
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
