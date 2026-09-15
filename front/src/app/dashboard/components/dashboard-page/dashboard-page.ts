import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { BottomSheetService } from '../../../shared/bottom-sheet/services/bottom-sheet.service';
import { MOCK_CATEGORIES, MOCK_CATEGORY_BY_ID, MOCK_TRANSACTIONS } from '../../../shared/mock-data/mock-data';
import { MonthPickerSheet } from '../../../shared/month-picker-sheet/month-picker-sheet';
import { PageHeader } from '../../../shared/page-header/page-header';
import { MonthRef, monthRefToDate } from '../../../shared/types/month-ref';
import { Transaction } from '../../../transactions/types/transaction';
import { TransactionFormSheet } from '../../../transactions/components/transaction-form-sheet/transaction-form-sheet';
import { BalanceSummaryCard } from '../balance-summary-card/balance-summary-card';
import { CashflowChart } from '../cashflow-chart/cashflow-chart';
import { CategoryBreakdownCard } from '../category-breakdown-card/category-breakdown-card';
import { RecentTransactionsCard, RecentTransactionRow } from '../recent-transactions-card/recent-transactions-card';

const CURRENT_MONTH: MonthRef = { month: 8, year: 2026 };

@Component({
	selector: 'app-dashboard-page',
	templateUrl: './dashboard-page.html',
	styleUrl: './dashboard-page.scss',
	imports: [PageHeader, BalanceSummaryCard, CashflowChart, CategoryBreakdownCard, RecentTransactionsCard],
	providers: [DatePipe],
})
export class DashboardPage {
	private readonly bottomSheetService = inject(BottomSheetService);
	private readonly snackBar = inject(MatSnackBar);
	private readonly router = inject(Router);
	private readonly datePipe = inject(DatePipe);

	protected readonly categories = MOCK_CATEGORIES;
	protected readonly monthRef = signal<MonthRef>(CURRENT_MONTH);

	protected readonly monthLabel = computed(() => this.datePipe.transform(monthRefToDate(this.monthRef()), 'MMMM y') ?? '');
	protected readonly greeting = computed(() => `${this.datePipe.transform(monthRefToDate(this.monthRef()), 'MMMM') ?? ''} em curso`);

	protected readonly transactionsInMonth = computed(() => {
		const month = this.monthRef();
		return MOCK_TRANSACTIONS.filter((t) => {
			const date = new Date(t.date);
			return date.getMonth() === month.month && date.getFullYear() === month.year;
		});
	});

	protected readonly net = computed(() => this.transactionsInMonth().reduce((acc, t) => acc + t.value, 0));
	protected readonly totalIn = computed(() => this.transactionsInMonth().reduce((acc, t) => (t.value > 0 ? acc + t.value : acc), 0));
	protected readonly totalOut = computed(() => this.transactionsInMonth().reduce((acc, t) => (t.value < 0 ? acc + t.value : acc), 0));

	protected readonly recentTransactions = computed<RecentTransactionRow[]>(() =>
		this.transactionsInMonth()
			.slice()
			.sort((a, b) => b.date.localeCompare(a.date))
			.slice(0, 5)
			.map((t) => this.toRow(t)),
	);

	protected openMonthPicker() {
		this.bottomSheetService.open(MonthPickerSheet, {
			data: { month: this.monthRef().month, year: this.monthRef().year, clearable: false },
			callback: (result, sheet) => {
				if (result) this.monthRef.set(result);
				sheet.close();
			},
		});
	}

	protected openCreateForm() {
		this.openTransactionForm(undefined);
	}

	protected openTransaction(id: string) {
		const transaction = MOCK_TRANSACTIONS.find((t) => t.id === id);
		if (transaction) this.openTransactionForm(transaction);
	}

	protected viewAllTransactions() {
		this.router.navigate(['/transactions']);
	}

	private openTransactionForm(transaction: Transaction | undefined) {
		this.bottomSheetService.open(TransactionFormSheet, {
			data: { transaction, categories: this.categories },
			callback: (outcome, sheet) => {
				sheet.close();
				if (outcome.type === 'delete') {
					console.log('delete transaction', outcome.id);
					const ref = this.snackBar.open('Transação excluída.', 'Desfazer', { duration: 5000 });
					ref.onAction().subscribe(() => console.log('undo delete transaction', outcome.id));
					return;
				}
				console.log(transaction ? 'update transaction' : 'create transaction', outcome.result);
				this.snackBar.open(transaction ? 'Transação salva.' : 'Transação criada.', undefined, { duration: 3000 });
			},
		});
	}

	private toRow(transaction: Transaction): RecentTransactionRow {
		const category = MOCK_CATEGORY_BY_ID[transaction.categoryId];
		const day = this.datePipe.transform(transaction.date, 'dd/MM') ?? '';
		return {
			id: transaction.id,
			icon: transaction.value < 0 ? 'north_east' : 'south_west',
			description: transaction.description || 'Sem descrição',
			descriptionMuted: !transaction.description,
			meta: `${day} · ${category?.name ?? ''}`,
			categoryColor: category?.color ?? 'var(--mat-sys-outline)',
			value: transaction.value,
		};
	}
}
