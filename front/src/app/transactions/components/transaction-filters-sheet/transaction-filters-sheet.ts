import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Category } from '../../../categories/types/category';
import { BottomSheetDirective } from '../../../shared/bottom-sheet/directives/bottom-sheet.directive';
import { MonthRef } from '../../../shared/types/month-ref';
import { SortOption, SortValue } from '../../../shared/types/sort';
import { TransactionsService } from '../../services/transactions.service';
import { Transaction } from '../../types/transaction';
import {
	DEFAULT_TRANSACTION_FILTERS,
	TRANSACTION_TYPE_OPTIONS,
	TransactionFilters,
	TransactionType,
} from '../../types/transaction-filters';
import { TRANSACTION_SORT_OPTIONS, TransactionSortField } from '../../types/transaction-sort';

export interface TransactionFiltersData {
	filters: TransactionFilters;
	sort: SortValue<TransactionSortField>;
	monthRef: MonthRef | null;
	categories: Category[];
}

export interface TransactionFiltersResult {
	filters: TransactionFilters;
	sort: SortValue<TransactionSortField>;
}

@Component({
	selector: 'app-transaction-filters-sheet',
	templateUrl: './transaction-filters-sheet.html',
	styleUrl: './transaction-filters-sheet.scss',
	imports: [MatButtonModule, MatIconModule],
})
export class TransactionFiltersSheet extends BottomSheetDirective<TransactionFiltersData, TransactionFiltersResult> {
	private readonly transactionsService = inject(TransactionsService);

	protected readonly typeOptions = TRANSACTION_TYPE_OPTIONS;
	protected readonly sortOptions = TRANSACTION_SORT_OPTIONS;
	protected readonly categories = this.sheetData.categories;

	protected readonly draftFilters = signal<TransactionFilters>(this.sheetData.filters);
	protected readonly draftSort = signal<SortValue<TransactionSortField>>(this.sheetData.sort);

	private readonly previewResource = httpResource<Transaction[]>(() =>
		this.transactionsService.list(this.draftFilters(), this.sheetData.sort, this.sheetData.monthRef),
	);

	protected readonly resultCount = computed(() =>
		this.previewResource.hasValue() ? this.previewResource.value().length : null,
	);

	protected setDescription(event: Event) {
		const value = (event.target as HTMLInputElement).value;
		this.draftFilters.update((f) => ({ ...f, description: value }));
	}

	protected setType(type: TransactionType) {
		this.draftFilters.update((f) => ({ ...f, type }));
	}

	protected setCategory(categoryId: string) {
		this.draftFilters.update((f) => ({ ...f, categoryId }));
	}

	protected selectSort(option: SortOption<TransactionSortField>) {
		this.draftSort.set({ orderBy: option.orderBy, ascending: option.ascending });
	}

	protected isSortSelected(option: SortOption<TransactionSortField>) {
		const sort = this.draftSort();
		return sort.orderBy === option.orderBy && sort.ascending === option.ascending;
	}

	protected clear() {
		this.draftFilters.set(DEFAULT_TRANSACTION_FILTERS);
	}

	protected apply() {
		this.callback({ filters: this.draftFilters(), sort: this.draftSort() });
	}
}
