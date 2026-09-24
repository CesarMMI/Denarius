import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { BottomSheetDirective } from '../../../shared/bottom-sheet/directives/bottom-sheet.directive';
import { MonthRef } from '../../../shared/types/month-ref';
import { SortOption, SortValue } from '../../../shared/types/sort';
import { CategoriesService } from '../../services/categories.service';
import { Category } from '../../types/category';
import { CATEGORY_TRANSACTION_FILTER_OPTIONS, CategoryFilters, DEFAULT_CATEGORY_FILTERS } from '../../types/category-filters';
import { CATEGORY_SORT_OPTIONS, CategorySortField } from '../../types/category-sort';

export interface CategoryFiltersData {
	filters: CategoryFilters;
	sort: SortValue<CategorySortField>;
	monthRef: MonthRef | null;
}

export interface CategoryFiltersResult {
	filters: CategoryFilters;
	sort: SortValue<CategorySortField>;
}

@Component({
	selector: 'app-category-filters-sheet',
	templateUrl: './category-filters-sheet.html',
	styleUrl: './category-filters-sheet.scss',
	imports: [MatButtonModule, MatIconModule],
})
export class CategoryFiltersSheet extends BottomSheetDirective<CategoryFiltersData, CategoryFiltersResult> {
	private readonly categoriesService = inject(CategoriesService);

	protected readonly sortOptions = CATEGORY_SORT_OPTIONS;
	protected readonly withOptions = CATEGORY_TRANSACTION_FILTER_OPTIONS;

	protected readonly draftFilters = signal<CategoryFilters>(this.sheetData.filters);
	protected readonly draftSort = signal<SortValue<CategorySortField>>(this.sheetData.sort);

	private readonly previewResource = httpResource<Category[]>(() =>
		this.categoriesService.list(this.draftFilters(), this.sheetData.sort, this.sheetData.monthRef),
	);

	protected readonly resultCount = computed(() =>
		this.previewResource.hasValue() ? this.previewResource.value().length : null,
	);

	protected setName(event: Event) {
		const value = (event.target as HTMLInputElement).value;
		this.draftFilters.update((f) => ({ ...f, name: value }));
	}

	protected setWithTransaction(value: boolean | null) {
		this.draftFilters.update((f) => ({ ...f, withTransaction: value }));
	}

	protected selectSort(option: SortOption<CategorySortField>) {
		this.draftSort.set({ orderBy: option.orderBy, ascending: option.ascending });
	}

	protected isSortSelected(option: SortOption<CategorySortField>) {
		const sort = this.draftSort();
		return sort.orderBy === option.orderBy && sort.ascending === option.ascending;
	}

	protected clear() {
		this.draftFilters.set(DEFAULT_CATEGORY_FILTERS);
	}

	protected apply() {
		this.callback({ filters: this.draftFilters(), sort: this.draftSort() });
	}
}
