import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { ReportsService } from './reports.service';

describe('ReportsService', () => {
	const baseUrl = `${environment.apiUrl}/reports`;
	const september = new Date(2026, 8, 1);

	let service: ReportsService;

	beforeEach(() => {
		service = TestBed.inject(ReportsService);
	});

	it.each([
		['summary', () => service.summary(september)],
		['expensesByCategory', () => service.expensesByCategory(september)],
		['incomeVsExpense', () => service.incomeVsExpense(september)],
		['cumulativeExpenses', () => service.cumulativeExpenses(september)],
		['transactions', () => service.transactions(september)],
	])('should request the %s report of the month', (report, request) => {
		const { url, params } = request();

		expect(url).toBe(`${baseUrl}/${report}`);
		expect(params.toString()).toBe('month=2026-09');
	});

	it('should leave the month to the API when there is none', () => {
		expect(service.summary(null).params.keys()).toEqual([]);
	});

	it('should send how many months the income-vs-expense series covers, only when given', () => {
		expect(service.incomeVsExpense(new Date(2027, 0, 1), 6).params.toString()).toBe('month=2027-01&months=6');
		expect(service.incomeVsExpense(null).params.keys()).toEqual([]);
	});
});
