import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute } from '@angular/router';
import { MOCK_CATEGORIES, MOCK_CATEGORY_BY_ID, MOCK_TRANSACTIONS } from '../../../shared/mock-data/mock-data';
import { BottomSheetService } from '../../../shared/bottom-sheet/services/bottom-sheet.service';
import { EmptyState } from '../../../shared/empty-state/empty-state';
import { MonthPickerSheet } from '../../../shared/month-picker-sheet/month-picker-sheet';
import { PageHeader } from '../../../shared/page-header/page-header';
import { MonthRef, monthRefToDate } from '../../../shared/types/month-ref';
import { formatSignedMoney } from '../../../shared/utils/format-currency';
import { Transaction } from '../../types/transaction';
import { DEFAULT_TRANSACTION_FILTERS, TRANSACTION_TYPE, TransactionFilters } from '../../types/transaction-filters';
import { TransactionFiltersSheet } from '../transaction-filters-sheet/transaction-filters-sheet';
import { TransactionFormSheet } from '../transaction-form-sheet/transaction-form-sheet';
import { TransactionList, TransactionListGroup, TransactionListRow } from '../transaction-list/transaction-list';

@Component({
	selector: 'app-transactions-page',
	templateUrl: './transactions-page.html',
	styleUrl: './transactions-page.scss',
	imports: [PageHeader, TransactionList, EmptyState],
	providers: [DatePipe],
})
export class TransactionsPage {
	private readonly bottomSheetService = inject(BottomSheetService);
	private readonly snackBar = inject(MatSnackBar);
	private readonly datePipe = inject(DatePipe);

	private readonly initialCategoryId = inject(ActivatedRoute).snapshot.queryParamMap.get('categoryId');

	protected readonly categories = MOCK_CATEGORIES;

	protected readonly monthRef = signal<MonthRef | null>(null);
	protected readonly filters = signal<TransactionFilters>({
		...DEFAULT_TRANSACTION_FILTERS,
		categoryId: this.initialCategoryId ?? DEFAULT_TRANSACTION_FILTERS.categoryId,
	});

	protected readonly monthFilteredTransactions = computed(() =>
		MOCK_TRANSACTIONS.filter((t) => this.matchesMonth(t, this.monthRef())),
	);

	protected readonly filteredTransactions = computed(() => {
		const filters = this.filters();
		return this.monthFilteredTransactions()
			.filter((t) => t.description.toLowerCase().includes(filters.description.toLowerCase()))
			.filter((t) => filters.categoryId === 'all' || t.categoryId === filters.categoryId)
			.filter((t) => filters.type === TRANSACTION_TYPE.All || (filters.type === TRANSACTION_TYPE.In ? t.value > 0 : t.value < 0))
			.sort((a, b) => b.date.localeCompare(a.date));
	});

	protected readonly groups = computed<TransactionListGroup[]>(() => {
		const byDay = new Map<string, Transaction[]>();
		for (const transaction of this.filteredTransactions()) {
			const list = byDay.get(transaction.date) ?? [];
			list.push(transaction);
			byDay.set(transaction.date, list);
		}
		return Array.from(byDay.entries()).map(([date, items]) => ({
			label: this.datePipe.transform(date, "d 'de' MMMM") ?? date,
			total: `${formatSignedMoney(items.reduce((acc, t) => acc + t.value, 0))} no dia`,
			items: items.map((t) => this.toRow(t)),
		}));
	});

	protected readonly monthLabel = computed(() => {
		const month = this.monthRef();
		return month ? (this.datePipe.transform(monthRefToDate(month), 'MMMM y') ?? '') : 'Todos os meses';
	});

	protected readonly chipList = computed(() => {
		const f = this.filters();
		const items: { label: string; reset: () => void }[] = [];
		if (f.description) items.push({ label: `"${f.description}"`, reset: () => this.filters.update((v) => ({ ...v, description: '' })) });
		if (f.type !== TRANSACTION_TYPE.All)
			items.push({
				label: f.type === TRANSACTION_TYPE.In ? 'Entradas' : 'Saídas',
				reset: () => this.filters.update((v) => ({ ...v, type: TRANSACTION_TYPE.All })),
			});
		if (f.categoryId !== 'all')
			items.push({
				label: MOCK_CATEGORY_BY_ID[f.categoryId]?.name ?? '',
				reset: () => this.filters.update((v) => ({ ...v, categoryId: 'all' })),
			});
		return items;
	});

	protected readonly chipLabels = computed(() => this.chipList().map((c) => c.label));
	protected readonly hasActiveFilters = computed(() => this.chipList().length > 0);

	protected removeChip(index: number) {
		this.chipList()[index]?.reset();
	}

	protected openMonthPicker() {
		const current = this.monthRef() ?? { month: new Date().getMonth(), year: new Date().getFullYear() };
		this.bottomSheetService.open(MonthPickerSheet, {
			data: { month: current.month, year: current.year, clearable: true },
			callback: (result, sheet) => {
				this.monthRef.set(result);
				sheet.close();
			},
		});
	}

	protected openFilters() {
		this.bottomSheetService.open(TransactionFiltersSheet, {
			data: { filters: this.filters(), transactions: this.monthFilteredTransactions(), categories: this.categories },
			callback: (result, sheet) => {
				this.filters.set(result);
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

	private matchesMonth(transaction: Transaction, month: MonthRef | null) {
		if (!month) return true;
		const date = new Date(transaction.date);
		return date.getMonth() === month.month && date.getFullYear() === month.year;
	}

	private toRow(transaction: Transaction): TransactionListRow {
		const category = MOCK_CATEGORY_BY_ID[transaction.categoryId];
		return {
			id: transaction.id,
			icon: transaction.value < 0 ? 'north_east' : 'south_west',
			description: transaction.description || 'Sem descrição',
			descriptionMuted: !transaction.description,
			categoryName: category?.name ?? '',
			categoryColor: category?.color ?? 'var(--mat-sys-outline)',
			value: transaction.value,
		};
	}
}
