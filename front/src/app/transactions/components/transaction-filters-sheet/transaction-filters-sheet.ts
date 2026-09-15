import { Component, computed, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Category } from '../../../categories/types/category';
import { BottomSheetDirective } from '../../../shared/bottom-sheet/directives/bottom-sheet.directive';
import { Transaction } from '../../types/transaction';
import {
	DEFAULT_TRANSACTION_FILTERS,
	TRANSACTION_TYPE,
	TRANSACTION_TYPE_OPTIONS,
	TransactionFilters,
	TransactionType,
} from '../../types/transaction-filters';

export interface TransactionFiltersData {
	filters: TransactionFilters;
	transactions: Transaction[];
	categories: Category[];
}

@Component({
	selector: 'app-transaction-filters-sheet',
	templateUrl: './transaction-filters-sheet.html',
	styleUrl: './transaction-filters-sheet.scss',
	imports: [MatButtonModule, MatIconModule],
})
export class TransactionFiltersSheet extends BottomSheetDirective<TransactionFiltersData, TransactionFilters> {
	protected readonly typeOptions = TRANSACTION_TYPE_OPTIONS;
	protected readonly categories = this.sheetData.categories;

	protected readonly draft = signal<TransactionFilters>(this.sheetData.filters);

	protected readonly resultCount = computed(() => this.filterTransactions(this.draft()).length);

	protected setDescription(event: Event) {
		const value = (event.target as HTMLInputElement).value;
		this.draft.update((f) => ({ ...f, description: value }));
	}

	protected setType(type: TransactionType) {
		this.draft.update((f) => ({ ...f, type }));
	}

	protected setCategory(categoryId: string) {
		this.draft.update((f) => ({ ...f, categoryId }));
	}

	protected clear() {
		this.draft.set(DEFAULT_TRANSACTION_FILTERS);
	}

	protected apply() {
		this.callback(this.draft());
	}

	private filterTransactions(filters: TransactionFilters) {
		return this.sheetData.transactions
			.filter((t) => t.description.toLowerCase().includes(filters.description.toLowerCase()))
			.filter((t) => filters.categoryId === 'all' || t.categoryId === filters.categoryId)
			.filter((t) => filters.type === TRANSACTION_TYPE.All || (filters.type === TRANSACTION_TYPE.In ? t.value > 0 : t.value < 0));
	}
}
