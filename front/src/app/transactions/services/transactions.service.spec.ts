import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { buildTransaction } from '../testing/transaction-fixture';
import { TransactionInput } from '../types/transaction';
import { TransactionsService } from './transactions.service';

describe('TransactionsService', () => {
	const baseUrl = `${environment.apiUrl}/Transactions`;
	const input: TransactionInput = {
		description: 'Feira da semana',
		categoryId: 'mercado',
		value: -186.42,
		date: '2026-09-24T00:00:00.000Z',
	};

	let service: TransactionsService;
	let httpTesting: HttpTestingController;

	beforeEach(() => {
		TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
		service = TestBed.inject(TransactionsService);
		httpTesting = TestBed.inject(HttpTestingController);
	});

	afterEach(() => httpTesting.verify());

	describe('list', () => {
		it('should target the transactions endpoint without params by default', () => {
			const { url, params } = service.list();
			expect(url).toBe(baseUrl);
			expect(params.keys()).toEqual([]);
		});

		it('should skip empty filters', () => {
			const { params } = service.list({ description: '', type: '', categoryId: '', month: null });
			expect(params.keys()).toEqual([]);
		});

		it('should send the sort as orderBy and asc', () => {
			expect(service.list({}, { active: 'Value', direction: 'asc' }).params.toString()).toBe('orderBy=Value&asc=true');
			expect(service.list({}, { active: 'Date', direction: 'desc' }).params.toString()).toBe('orderBy=Date&asc=false');
		});

		it('should leave the order to the API when the sort has no direction', () => {
			expect(service.list({}, { active: 'Value', direction: '' }).params.keys()).toEqual([]);
		});

		it('should send every filter together, with the first day of the month as dateRef', () => {
			const { params } = service.list(
				{ description: 'uber', type: 'Out', categoryId: 'transporte', month: new Date(2027, 0, 1) },
				{ active: 'Date', direction: 'desc' },
			);
			expect(params.toString()).toBe(
				'description=uber&type=Out&categoryId=transporte&dateRef=2027-01-01&orderBy=Date&asc=false',
			);
		});
	});

	it('create should POST the transaction', () => {
		const transaction = buildTransaction();
		let response: unknown;
		service.create(input).subscribe((value) => (response = value));

		const req = httpTesting.expectOne(baseUrl);
		expect(req.request.method).toBe('POST');
		expect(req.request.body).toEqual(input);
		req.flush(transaction, { status: 201, statusText: 'Created' });
		expect(response).toEqual(transaction);
	});

	it('update should PUT the transaction', () => {
		const transaction = buildTransaction();
		let response: unknown;
		service.update(transaction.id, input).subscribe((value) => (response = value));

		const req = httpTesting.expectOne(`${baseUrl}/${transaction.id}`);
		expect(req.request.method).toBe('PUT');
		expect(req.request.body).toEqual(input);
		req.flush(transaction);
		expect(response).toEqual(transaction);
	});

	it('delete should DELETE the transaction', () => {
		const transaction = buildTransaction();
		let completed = false;
		service.delete(transaction.id).subscribe({ complete: () => (completed = true) });

		const req = httpTesting.expectOne(`${baseUrl}/${transaction.id}`);
		expect(req.request.method).toBe('DELETE');
		req.flush(null, { status: 204, statusText: 'No Content' });
		expect(completed).toBe(true);
	});
});
