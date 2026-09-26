import { DatePipe } from '@angular/common';
import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute } from '@angular/router';
import { CategoriesService } from '../../../categories/services/categories.service';
import { Category } from '../../../categories/types/category';
import { DEFAULT_CATEGORY_FILTERS } from '../../../categories/types/category-filters';
import { CATEGORY_SORT_OPTIONS } from '../../../categories/types/category-sort';
import { BottomSheetService } from '../../../shared/bottom-sheet/services/bottom-sheet.service';
import { EmptyState } from '../../../shared/empty-state/empty-state';
import { MonthPickerSheet } from '../../../shared/month-picker-sheet/month-picker-sheet';
import { PageHeader } from '../../../shared/page-header/page-header';
import { MonthRef, monthRefToDate } from '../../../shared/types/month-ref';
import { SortValue } from '../../../shared/types/sort';
import { fromApiDate, toDateKey } from '../../../shared/utils/api-date';
import { formatSignedMoney } from '../../../shared/utils/format-currency';
import { TransactionsService } from '../../services/transactions.service';
import { Transaction } from '../../types/transaction';
import { DEFAULT_TRANSACTION_FILTERS, TRANSACTION_TYPE, TransactionFilters } from '../../types/transaction-filters';
import { TransactionFormResult, TransactionInput } from '../../types/transaction-form-result';
import { TRANSACTION_SORT_FIELD, TRANSACTION_SORT_OPTIONS, TransactionSortField } from '../../types/transaction-sort';
import { TransactionFiltersSheet } from '../transaction-filters-sheet/transaction-filters-sheet';
import { TransactionFormSheet } from '../transaction-form-sheet/transaction-form-sheet';
import { TransactionList, TransactionListGroup, TransactionListRow } from '../transaction-list/transaction-list';

@Component({
	selector: 'app-transactions-page',
	templateUrl: './transactions-page.html',
	styleUrl: './transactions-page.scss',
	imports: [PageHeader, TransactionList, EmptyState, MatProgressBarModule],
	providers: [DatePipe],
})
export class TransactionsPage {
	private readonly bottomSheetService = inject(BottomSheetService);
	private readonly transactionsService = inject(TransactionsService);
	private readonly categoriesService = inject(CategoriesService);
	private readonly snackBar = inject(MatSnackBar);
	private readonly datePipe = inject(DatePipe);

	private readonly initialCategoryId = inject(ActivatedRoute).snapshot.queryParamMap.get('categoryId');

	protected readonly monthRef = signal<MonthRef | null>(null);
	protected readonly filters = signal<TransactionFilters>({
		...DEFAULT_TRANSACTION_FILTERS,
		categoryId: this.initialCategoryId ?? DEFAULT_TRANSACTION_FILTERS.categoryId,
	});
	protected readonly sort = signal<SortValue<TransactionSortField>>(TRANSACTION_SORT_OPTIONS[0]);

	protected readonly transactionsResource = httpResource<Transaction[]>(
		() => this.transactionsService.list(this.filters(), this.sort(), this.monthRef()),
		{ defaultValue: [] },
	);

	private readonly categoriesResource = httpResource<Category[]>(
		() => this.categoriesService.list(DEFAULT_CATEGORY_FILTERS, CATEGORY_SORT_OPTIONS[0]),
		{ defaultValue: [] },
	);

	protected readonly transactions = computed(() =>
		this.transactionsResource.hasValue() ? this.transactionsResource.value() : [],
	);

	protected readonly categories = computed(() =>
		this.categoriesResource.hasValue() ? this.categoriesResource.value() : [],
	);

	private readonly categoryById = computed(() => new Map(this.categories().map((c) => [c.id, c])));

	/** Grouped by day when sorted by date; otherwise a single group keeps the API order. */
	protected readonly groups = computed<TransactionListGroup[]>(() => {
		const transactions = this.transactions();
		if (transactions.length === 0) return [];

		const sort = this.sort();
		if (sort.orderBy !== TRANSACTION_SORT_FIELD.Date) {
			const label = TRANSACTION_SORT_OPTIONS.find((o) => o.orderBy === sort.orderBy && o.ascending === sort.ascending)?.label;
			return [this.toGroup(label ?? '', `${this.sumOf(transactions)} no total`, transactions)];
		}

		const byDay = new Map<string, Transaction[]>();
		for (const transaction of transactions) {
			const key = toDateKey(fromApiDate(transaction.date));
			const list = byDay.get(key) ?? [];
			list.push(transaction);
			byDay.set(key, list);
		}
		return Array.from(byDay.values()).map((items) =>
			this.toGroup(
				this.datePipe.transform(fromApiDate(items[0].date), "d 'de' MMMM") ?? '',
				`${this.sumOf(items)} no dia`,
				items,
			),
		);
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
				label: this.categoryById().get(f.categoryId)?.name ?? 'Categoria',
				reset: () => this.filters.update((v) => ({ ...v, categoryId: 'all' })),
			});
		return items;
	});

	protected readonly chipLabels = computed(() => this.chipList().map((c) => c.label));
	protected readonly hasActiveFilters = computed(() => this.chipList().length > 0);

	protected removeChip(index: number) {
		this.chipList()[index]?.reset();
	}

	protected reload() {
		this.transactionsResource.reload();
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
			data: { filters: this.filters(), sort: this.sort(), monthRef: this.monthRef(), categories: this.categories() },
			callback: (result, sheet) => {
				this.filters.set(result.filters);
				this.sort.set(result.sort);
				sheet.close();
			},
		});
	}

	protected openCreateForm() {
		this.openTransactionForm(undefined);
	}

	protected openTransaction(id: string) {
		const transaction = this.transactions().find((t) => t.id === id);
		if (transaction) this.openTransactionForm(transaction);
	}

	private openTransactionForm(transaction: Transaction | undefined) {
		this.bottomSheetService.open(TransactionFormSheet, {
			data: { transaction, categories: this.categories() },
			callback: (outcome, sheet) => {
				sheet.close();
				if (outcome.type === 'save') return this.saveTransaction(outcome.result);
				if (transaction) this.deleteTransaction(transaction);
			},
		});
	}

	private saveTransaction({ id, ...input }: TransactionFormResult) {
		const request = id ? this.transactionsService.update(id, input) : this.transactionsService.create(input);
		request.subscribe({
			next: () => {
				this.snackBar.open(id ? 'Transação salva.' : 'Transação criada.', undefined, { duration: 3000 });
				this.reload();
			},
			error: (error: HttpErrorResponse) =>
				this.showError(error, id ? 'Não foi possível salvar a transação.' : 'Não foi possível criar a transação.'),
		});
	}

	private deleteTransaction(transaction: Transaction) {
		this.transactionsService.delete(transaction.id).subscribe({
			next: () => {
				this.reload();
				this.snackBar
					.open('Transação excluída.', 'Desfazer', { duration: 5000 })
					.onAction()
					.subscribe(() => this.restoreTransaction(transaction));
			},
			error: (error: HttpErrorResponse) => this.showError(error, 'Não foi possível excluir a transação.'),
		});
	}

	/** The API has no undelete, so undoing recreates the transaction with the same data. */
	private restoreTransaction({ description, categoryId, value, date }: Transaction) {
		const input: TransactionInput = { description, categoryId, value, date };
		this.transactionsService.create(input).subscribe({
			next: () => {
				this.snackBar.open('Transação restaurada.', undefined, { duration: 3000 });
				this.reload();
			},
			error: (error: HttpErrorResponse) => this.showError(error, 'Não foi possível restaurar a transação.'),
		});
	}

	private showError(error: HttpErrorResponse, fallback: string) {
		const detail = typeof error.error?.detail === 'string' ? error.error.detail : null;
		this.snackBar.open(detail ?? fallback, 'Fechar', { duration: 5000 });
	}

	private sumOf(transactions: Transaction[]) {
		return formatSignedMoney(transactions.reduce((acc, t) => acc + t.value, 0));
	}

	private toGroup(label: string, total: string, transactions: Transaction[]): TransactionListGroup {
		return { label, total, items: transactions.map((t) => this.toRow(t)) };
	}

	private toRow(transaction: Transaction): TransactionListRow {
		const category = this.categoryById().get(transaction.categoryId);
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
