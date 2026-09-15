import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { BottomSheetService } from '../../../shared/bottom-sheet/services/bottom-sheet.service';
import { ConfirmDeleteDialog } from '../../../shared/confirm-delete-dialog/confirm-delete-dialog';
import { EmptyState } from '../../../shared/empty-state/empty-state';
import { MOCK_CATEGORIES } from '../../../shared/mock-data/mock-data';
import { MonthPickerSheet } from '../../../shared/month-picker-sheet/month-picker-sheet';
import { PageHeader } from '../../../shared/page-header/page-header';
import { MonthRef, monthRefToDate } from '../../../shared/types/month-ref';
import { SortValue } from '../../../shared/types/sort';
import { Category } from '../../types/category';
import { CategoryFilters, DEFAULT_CATEGORY_FILTERS } from '../../types/category-filters';
import { CATEGORY_SORT_FIELD, CATEGORY_SORT_OPTIONS, CategorySortField } from '../../types/category-sort';
import { CategoryBlockedDialog, CategoryBlockedDialogResult } from '../category-blocked-dialog/category-blocked-dialog';
import { CategoryFiltersSheet } from '../category-filters-sheet/category-filters-sheet';
import { CategoryFormSheet } from '../category-form-sheet/category-form-sheet';
import { CategoryList } from '../category-list/category-list';
import { CategoryMenuSheet } from '../category-menu-sheet/category-menu-sheet';

@Component({
	selector: 'app-categories-page',
	templateUrl: './categories-page.html',
	styleUrl: './categories-page.scss',
	imports: [PageHeader, CategoryList, EmptyState],
	providers: [DatePipe],
})
export class CategoriesPage {
	private readonly bottomSheetService = inject(BottomSheetService);
	private readonly matDialog = inject(MatDialog);
	private readonly snackBar = inject(MatSnackBar);
	private readonly router = inject(Router);
	private readonly datePipe = inject(DatePipe);

	protected readonly monthRef = signal<MonthRef | null>(null);
	protected readonly filters = signal<CategoryFilters>(DEFAULT_CATEGORY_FILTERS);
	protected readonly sort = signal<SortValue<CategorySortField>>(CATEGORY_SORT_OPTIONS[0]);

	protected readonly filteredCategories = computed(() => {
		const filters = this.filters();
		const sort = this.sort();
		return MOCK_CATEGORIES.filter((c) => c.name.toLowerCase().includes(filters.name.toLowerCase()))
			.filter(
				(c) => filters.withTransaction === null || (filters.withTransaction ? c.transactionCount > 0 : c.transactionCount === 0),
			)
			.slice()
			.sort((a, b) => {
				const diff =
					sort.orderBy === CATEGORY_SORT_FIELD.Name
						? a.name.localeCompare(b.name, 'pt-BR')
						: sort.orderBy === CATEGORY_SORT_FIELD.TransactionCount
							? a.transactionCount - b.transactionCount
							: a.balance - b.balance;
				return sort.ascending ? diff : -diff;
			});
	});

	protected readonly monthLabel = computed(() => {
		const month = this.monthRef();
		return month ? (this.datePipe.transform(monthRefToDate(month), 'MMMM y') ?? '') : 'Todos os meses';
	});

	protected readonly chipList = computed(() => {
		const f = this.filters();
		const items: { label: string; reset: () => void }[] = [];
		if (f.name) items.push({ label: `"${f.name}"`, reset: () => this.filters.update((v) => ({ ...v, name: '' })) });
		if (f.withTransaction !== null)
			items.push({
				label: f.withTransaction ? 'Com transações' : 'Sem transações',
				reset: () => this.filters.update((v) => ({ ...v, withTransaction: null })),
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
		this.bottomSheetService.open(CategoryFiltersSheet, {
			data: { filters: this.filters(), sort: this.sort(), categories: MOCK_CATEGORIES },
			callback: (result, sheet) => {
				this.filters.set(result.filters);
				this.sort.set(result.sort);
				sheet.close();
			},
		});
	}

	protected openCreateForm() {
		this.openCategoryForm(undefined);
	}

	protected openCategory(category: Category) {
		this.openCategoryForm(category);
	}

	protected openMenu(category: Category) {
		this.bottomSheetService.open(CategoryMenuSheet, {
			data: category,
			callback: (action, sheet) => {
				sheet.close();
				if (action === 'edit') return this.openCategoryForm(category);
				if (action === 'view-transactions') return this.goToCategoryTransactions(category);
				this.deleteCategory(category);
			},
		});
	}

	private openCategoryForm(category: Category | undefined) {
		this.bottomSheetService.open(CategoryFormSheet, {
			data: { category },
			callback: (outcome, sheet) => {
				sheet.close();
				if (outcome.type === 'delete') return this.deleteCategory(outcome.category);
				console.log(category ? 'update category' : 'create category', outcome.result);
				this.snackBar.open(category ? 'Categoria salva.' : 'Categoria criada.', undefined, { duration: 3000 });
			},
		});
	}

	private deleteCategory(category: Category) {
		if (category.transactionCount > 0) {
			this.matDialog
				.open(CategoryBlockedDialog, {
					width: 'min(100%, 400px)',
					data: { categoryName: category.name, transactionCount: category.transactionCount },
				})
				.afterClosed()
				.subscribe((result: CategoryBlockedDialogResult) => {
					if (result === 'view-transactions') this.goToCategoryTransactions(category);
				});
			return;
		}

		this.matDialog
			.open(ConfirmDeleteDialog, {
				width: 'min(100%, 400px)',
				data: {
					title: `Excluir "${category.name}"?`,
					body: 'Esta categoria não tem transações no período, então pode ser removida. A ação não pode ser desfeita.',
				},
			})
			.afterClosed()
			.subscribe((confirmed: boolean) => {
				if (!confirmed) return;
				console.log('delete category', category.id);
				this.snackBar.open('Categoria excluída.', undefined, { duration: 3000 });
			});
	}

	private goToCategoryTransactions(category: Category) {
		this.router.navigate(['/transactions'], { queryParams: { categoryId: category.id } });
	}
}
