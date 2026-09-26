import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { buildTransaction } from '../testing/transaction-fixture';
import { DEFAULT_TRANSACTION_FILTERS, TRANSACTION_TYPE } from '../types/transaction-filters';
import { TransactionInput } from '../types/transaction-form-result';
import { TRANSACTION_SORT_FIELD, TRANSACTION_SORT_OPTIONS } from '../types/transaction-sort';
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
		it('should target the transactions endpoint', () => {
			expect(service.list(DEFAULT_TRANSACTION_FILTERS, TRANSACTION_SORT_OPTIONS[0]).url).toBe(baseUrl);
		});

		it('should only send sort params when no filter is set', () => {
			const { params } = service.list(DEFAULT_TRANSACTION_FILTERS, TRANSACTION_SORT_OPTIONS[0]);
			expect(params.keys()).toEqual(['orderBy', 'asc']);
			expect(params.get('orderBy')).toBe('Date');
			expect(params.get('asc')).toBe('false');
		});

		it.each(TRANSACTION_SORT_OPTIONS)('should send the "$label" sort', (option) => {
			const { params } = service.list(DEFAULT_TRANSACTION_FILTERS, option);
			expect(params.get('orderBy')).toBe(option.orderBy);
			expect(params.get('asc')).toBe(String(option.ascending));
		});

		it('should send the description filter', () => {
			const { params } = service.list(
				{ ...DEFAULT_TRANSACTION_FILTERS, description: 'feira' },
				TRANSACTION_SORT_OPTIONS[0],
			);
			expect(params.get('description')).toBe('feira');
		});

		it.each([TRANSACTION_TYPE.In, TRANSACTION_TYPE.Out])('should send type=%s', (type) => {
			const { params } = service.list({ ...DEFAULT_TRANSACTION_FILTERS, type }, TRANSACTION_SORT_OPTIONS[0]);
			expect(params.get('type')).toBe(type);
		});

		it('should send the category filter', () => {
			const { params } = service.list(
				{ ...DEFAULT_TRANSACTION_FILTERS, categoryId: 'mercado' },
				TRANSACTION_SORT_OPTIONS[0],
			);
			expect(params.get('categoryId')).toBe('mercado');
		});

		it('should send the first day of the selected month as dateRef', () => {
			const { params } = service.list(DEFAULT_TRANSACTION_FILTERS, TRANSACTION_SORT_OPTIONS[0], { month: 0, year: 2027 });
			expect(params.get('dateRef')).toBe('2027-01-01');
		});

		it('should send every filter together', () => {
			const { params } = service.list(
				{ description: 'uber', type: TRANSACTION_TYPE.Out, categoryId: 'transporte' },
				{ orderBy: TRANSACTION_SORT_FIELD.Value, ascending: true },
				{ month: 8, year: 2026 },
			);
			expect(params.keys()).toEqual(['description', 'type', 'categoryId', 'dateRef', 'orderBy', 'asc']);
			expect(params.get('dateRef')).toBe('2026-09-01');
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
