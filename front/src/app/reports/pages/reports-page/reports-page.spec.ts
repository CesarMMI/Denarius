import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { LOCALE_ID, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDatepicker } from '@angular/material/datepicker';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { BaseChartDirective } from 'ng2-charts';
import { environment } from '../../../../environments/environment';
import { MonthField } from '../../../shared/month-field/month-field';
import { CumulativeComparisonChart } from '../../components/cumulative-comparison-chart/cumulative-comparison-chart';
import { ExpensesByCategoryChart } from '../../components/expenses-by-category-chart/expenses-by-category-chart';
import { IncomeVsExpenseChart } from '../../components/income-vs-expense-chart/income-vs-expense-chart';
import { TransactionsList } from '../../components/transactions-list/transactions-list';
import { ChartColors, ChartThemeService } from '../../services/chart-theme.service';
import { FakeChart } from '../../testing/fake-chart';
import {
	buildCumulativeExpenseComparison,
	buildExpensesByCategory,
	buildIncomeVsExpense,
	buildMonthlySummary,
	buildMonthlyTransaction,
} from '../../testing/report-fixtures';
import { ReportsPage } from './reports-page';

registerLocaleData(localePt);

describe('ReportsPage', () => {
	const baseUrl = `${environment.apiUrl}/reports`;
	const colors: ChartColors = {
		expense: 'red',
		income: 'green',
		neutral: 'gray',
		primary: 'amber',
		primaryVariant: 'brown',
		text: 'ink',
		grid: 'line',
		surface: 'card',
	};
	const reports = {
		summary: buildMonthlySummary({ month: '2026-10' }),
		expensesByCategory: buildExpensesByCategory(),
		incomeVsExpense: [buildIncomeVsExpense({ month: '2026-09' }), buildIncomeVsExpense({ month: '2026-10' })],
		cumulativeExpenses: buildCumulativeExpenseComparison(),
		transactions: [buildMonthlyTransaction()],
	};
	type Report = keyof typeof reports;
	const names = Object.keys(reports) as Report[];

	let fixture: ComponentFixture<ReportsPage>;
	let element: HTMLElement;
	let httpTesting: HttpTestingController;

	beforeEach(() => {
		// 03/10/2026 at noon in São Paulo; only the date is fake, so the app still settles with real timers.
		vi.useFakeTimers({ now: new Date('2026-10-03T15:00:00Z'), toFake: ['Date'] });
		TestBed.configureTestingModule({
			providers: [
				{ provide: LOCALE_ID, useValue: 'pt-BR' },
				provideHttpClient(),
				provideHttpClientTesting(),
				provideRouter([]),
				{ provide: ChartThemeService, useValue: { colors: signal(colors) } },
			],
		});
		for (const chart of [ExpensesByCategoryChart, IncomeVsExpenseChart, CumulativeComparisonChart]) {
			TestBed.overrideComponent(chart, { remove: { imports: [BaseChartDirective] }, add: { imports: [FakeChart] } });
		}

		httpTesting = TestBed.inject(HttpTestingController);
		fixture = TestBed.createComponent(ReportsPage);
		element = fixture.nativeElement;
	});

	afterEach(() => {
		httpTesting.verify();
		vi.useRealTimers();
	});

	function expectReport(report: Report): TestRequest {
		TestBed.tick();
		return httpTesting.expectOne((req) => req.method === 'GET' && req.url === `${baseUrl}/${report}`);
	}

	async function load(failing: Report[] = []) {
		for (const report of names) {
			if (failing.includes(report)) expectReport(report).flush(null, { status: 500, statusText: 'Server Error' });
			else expectReport(report).flush(reports[report]);
		}
		await fixture.whenStable();
	}

	function text(node: Element | null | undefined) {
		return node?.textContent?.replace(/\s+/g, ' ').trim();
	}

	function block(selector: string) {
		return element.querySelector(selector)!;
	}

	function retryButton(selector: string) {
		return Array.from(block(selector).querySelectorAll('button')).find((b) =>
			b.textContent?.includes('Tentar novamente'),
		);
	}

	function headerButton(label: string) {
		return element.querySelector<HTMLButtonElement>(`app-page-header button[aria-label="${label}"]`)!;
	}

	it('should request the five reports of the current month in São Paulo', () => {
		for (const report of names) expect(expectReport(report).request.params.toString()).toBe('month=2026-10');
	});

	it('should show the month in the month field', async () => {
		await load();

		expect(element.querySelector<HTMLInputElement>('app-month-field input')!.value).toBe('10/2026');
	});

	it('should show a spinner in every block while it loads', () => {
		const requests = names.map(expectReport);
		fixture.detectChanges();

		expect(element.querySelectorAll('.bento mat-progress-spinner')).toHaveLength(5);
		requests.forEach((req, i) => req.flush(reports[names[i]]));
	});

	it('should show every block once loaded', async () => {
		await load();

		expect(text(block('app-summary-cards mat-card-title'))).toBe('Saldo');
		expect(fixture.debugElement.queryAll(By.directive(FakeChart))).toHaveLength(3);
		expect(element.querySelectorAll('app-transactions-list tr[mat-row]')).toHaveLength(1);
		expect(element.querySelector('.bento mat-progress-spinner')).toBeNull();
	});

	it('should name the lines of the cumulative comparison after the month shown', async () => {
		await load();

		const lines = fixture.debugElement
			.query(By.css('app-cumulative-comparison-chart'))
			.query(By.directive(FakeChart))
			.injector.get(FakeChart);
		expect(lines.data()!.datasets.map((dataset) => dataset.label)).toEqual(['outubro de 2026', 'setembro de 2026']);
	});

	it('should keep the other blocks when one fails, and retry only that one', async () => {
		await load(['expensesByCategory']);

		expect(text(block('app-expenses-by-category-chart mat-card-content p'))).toBe(
			'Não foi possível carregar as despesas por categoria.',
		);
		expect(text(block('app-summary-cards mat-card-title'))).toBe('Saldo');
		expect(retryButton('app-summary-cards')).toBeUndefined();

		retryButton('app-expenses-by-category-chart')!.click();
		expectReport('expensesByCategory').flush(reports.expensesByCategory);
		// Before whenStable(): a reload of the other blocks would leave their requests pending and hang it.
		TestBed.tick();
		httpTesting.expectNone((req) => req.method === 'GET');
		await fixture.whenStable();

		expect(retryButton('app-expenses-by-category-chart')).toBeUndefined();
		expect(fixture.debugElement.queryAll(By.directive(FakeChart))).toHaveLength(3);
	});

	it('should reload every block from the header', async () => {
		await load();

		headerButton('Recarregar').click();

		await load();
	});

	it('should show every block for the month picked', async () => {
		await load();

		fixture.debugElement.query(By.directive(MonthField)).componentInstance.value.set(new Date(2026, 8, 1));

		for (const report of names) {
			const req = expectReport(report);
			expect(req.request.params.toString()).toBe('month=2026-09');
			req.flush(reports[report]);
		}
		await fixture.whenStable();
		expect(element.querySelector<HTMLInputElement>('app-month-field input')!.value).toBe('09/2026');
	});

	describe('when the month shown is picked again', () => {
		function pickMonth(month: Date) {
			const datepicker: MatDatepicker<Date> = fixture.debugElement.query(By.directive(MatDatepicker)).componentInstance;
			datepicker.monthSelected.emit(month);
		}

		it('should not reload any block', async () => {
			await load();

			pickMonth(new Date(2026, 9, 1));

			TestBed.tick();
			const sent = httpTesting.match((req) => req.method === 'GET');
			sent.forEach((req) => req.flush(null));
			expect(sent.map((req) => req.request.url)).toEqual([]);
		});

		it('should keep the month given to the cumulative comparison and to the transactions', async () => {
			await load();
			const chart: CumulativeComparisonChart = fixture.debugElement.query(
				By.directive(CumulativeComparisonChart),
			).componentInstance;
			const list: TransactionsList = fixture.debugElement.query(By.directive(TransactionsList)).componentInstance;
			const shown = chart.month();
			expect(list.month()).toBe(shown);

			pickMonth(new Date(2026, 9, 1));
			TestBed.tick();
			fixture.detectChanges();
			// Answers what picking the month may have sent; the test above covers it.
			httpTesting.match((req) => req.method === 'GET').forEach((req) => req.flush(null));

			expect(chart.month()).toBe(shown);
			expect(list.month()).toBe(shown);
		});
	});

	it.each([
		['Mês anterior', 'month=2026-09', '09/2026'],
		['Próximo mês', 'month=2026-11', '11/2026'],
	])('should show every block for the month picked with "%s", once', async (label, params, value) => {
		await load();

		headerButton(label).click();

		for (const report of names) {
			const req = expectReport(report);
			expect(req.request.params.toString()).toBe(params);
			req.flush(reports[report]);
		}
		await fixture.whenStable();
		expect(element.querySelector<HTMLInputElement>('app-month-field input')!.value).toBe(value);
	});

	it('should link the transactions to the transactions page on the month shown', async () => {
		await load();

		expect(element.querySelector('app-transactions-list a')!.getAttribute('href')).toBe('/transactions?month=2026-10');
	});
});
