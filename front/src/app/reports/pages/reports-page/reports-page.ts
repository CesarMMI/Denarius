import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { DateUtils } from '../../../shared/date-utils/date-utils';
import { MonthField } from '../../../shared/month-field/month-field';
import { PageHeader } from '../../../shared/page-header/page-header';
import { CumulativeComparisonChart } from '../../components/cumulative-comparison-chart/cumulative-comparison-chart';
import { ExpensesByCategoryChart } from '../../components/expenses-by-category-chart/expenses-by-category-chart';
import { IncomeVsExpenseChart } from '../../components/income-vs-expense-chart/income-vs-expense-chart';
import { SummaryCards } from '../../components/summary-cards/summary-cards';
import { TransactionsList } from '../../components/transactions-list/transactions-list';
import { ReportsService } from '../../services/reports.service';
import {
	CumulativeExpenseComparison,
	ExpensesByCategory,
	IncomeVsExpense,
	MonthlySummary,
	MonthlyTransaction,
} from '../../types/report';

/** The month in five blocks, each loading, failing and retrying on its own. */
@Component({
	selector: 'app-reports-page',
	templateUrl: './reports-page.html',
	styleUrl: './reports-page.scss',
	imports: [
		MatButtonModule,
		MatIconModule,
		MatTooltipModule,
		MonthField,
		PageHeader,
		CumulativeComparisonChart,
		ExpensesByCategoryChart,
		IncomeVsExpenseChart,
		SummaryCards,
		TransactionsList,
	],
})
export class ReportsPage {
	private readonly reportsService = inject(ReportsService);

	/** The first day of the month every block shows; the API's current month to begin with. */
	protected readonly month = signal<Date | null>(DateUtils.currentMonth());

	protected readonly summary = httpResource<MonthlySummary>(() => this.reportsService.summary(this.month()));
	protected readonly expensesByCategory = httpResource<ExpensesByCategory>(() =>
		this.reportsService.expensesByCategory(this.month()),
	);
	protected readonly incomeVsExpense = httpResource<IncomeVsExpense[]>(() =>
		this.reportsService.incomeVsExpense(this.month()),
	);
	protected readonly cumulativeExpenses = httpResource<CumulativeExpenseComparison>(() =>
		this.reportsService.cumulativeExpenses(this.month()),
	);
	protected readonly transactions = httpResource<MonthlyTransaction[]>(() =>
		this.reportsService.transactions(this.month()),
	);

	protected reload() {
		this.summary.reload();
		this.expensesByCategory.reload();
		this.incomeVsExpense.reload();
		this.cumulativeExpenses.reload();
		this.transactions.reload();
	}

	protected previousMonth() {
		this.month.set(DateUtils.previousMonth(this.month()));
		this.reload();
	}

	protected nextMonth() {
		this.month.set(DateUtils.nextMonth(this.month()));
		this.reload();
	}
}
