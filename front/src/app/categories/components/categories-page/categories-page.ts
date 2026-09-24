import { DatePipe } from '@angular/common';
import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { BottomSheetService } from '../../../shared/bottom-sheet/services/bottom-sheet.service';
import { ConfirmDeleteDialog } from '../../../shared/confirm-delete-dialog/confirm-delete-dialog';
import { EmptyState } from '../../../shared/empty-state/empty-state';
import { MonthPickerSheet } from '../../../shared/month-picker-sheet/month-picker-sheet';
import { PageHeader } from '../../../shared/page-header/page-header';
import { MonthRef, monthRefToDate } from '../../../shared/types/month-ref';
import { SortValue } from '../../../shared/types/sort';
import { CategoriesService } from '../../services/categories.service';
import { Category } from '../../types/category';
import { CategoryFilters, DEFAULT_CATEGORY_FILTERS } from '../../types/category-filters';
import { CategoryFormResult } from '../../types/category-form-result';
import { CATEGORY_SORT_OPTIONS, CategorySortField } from '../../types/category-sort';
import { CategoryBlockedDialog, CategoryBlockedDialogResult } from '../category-blocked-dialog/category-blocked-dialog';
import { CategoryFiltersSheet } from '../category-filters-sheet/category-filters-sheet';
import { CategoryFormSheet } from '../category-form-sheet/category-form-sheet';
import { CategoryList } from '../category-list/category-list';
import { CategoryMenuSheet } from '../category-menu-sheet/category-menu-sheet';

@Component({
	selector: 'app-categories-page',
	templateUrl: './categories-page.html',
	styleUrl: './categories-page.scss',
	imports: [PageHeader, CategoryList, EmptyState, MatProgressBarModule],
	providers: [DatePipe],
})
export class CategoriesPage {
	private readonly bottomSheetService = inject(BottomSheetService);
	private readonly categoriesService = inject(CategoriesService);
	private readonly matDialog = inject(MatDialog);
	private readonly snackBar = inject(MatSnackBar);
	private readonly router = inject(Router);
	private readonly datePipe = inject(DatePipe);

	protected readonly monthRef = signal<MonthRef | null>(null);
	protected readonly filters = signal<CategoryFilters>(DEFAULT_CATEGORY_FILTERS);
	protected readonly sort = signal<SortValue<CategorySortField>>(CATEGORY_SORT_OPTIONS[0]);

	protected readonly categoriesResource = httpResource<Category[]>(
		() => this.categoriesService.list(this.filters(), this.sort(), this.monthRef()),
		{ defaultValue: [] },
	);

	protected readonly categories = computed(() =>
		this.categoriesResource.hasValue() ? this.categoriesResource.value() : [],
	);

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

	protected reload() {
		this.categoriesResource.reload();
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
			data: { filters: this.filters(), sort: this.sort(), monthRef: this.monthRef() },
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
				this.saveCategory(outcome.result);
			},
		});
	}

	private saveCategory({ id, name, color }: CategoryFormResult) {
		const request = id
			? this.categoriesService.update(id, { name, color })
			: this.categoriesService.create({ name, color });
		request.subscribe({
			next: () => {
				this.snackBar.open(id ? 'Categoria salva.' : 'Categoria criada.', undefined, { duration: 3000 });
				this.reload();
			},
			error: (error: HttpErrorResponse) =>
				this.showError(error, id ? 'Não foi possível salvar a categoria.' : 'Não foi possível criar a categoria.'),
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
				this.categoriesService.delete(category.id).subscribe({
					next: () => {
						this.snackBar.open('Categoria excluída.', undefined, { duration: 3000 });
						this.reload();
					},
					error: (error: HttpErrorResponse) => this.showError(error, 'Não foi possível excluir a categoria.'),
				});
			});
	}

	private showError(error: HttpErrorResponse, fallback: string) {
		const detail = typeof error.error?.detail === 'string' ? error.error.detail : null;
		this.snackBar.open(detail ?? fallback, 'Fechar', { duration: 5000 });
	}

	private goToCategoryTransactions(category: Category) {
		this.router.navigate(['/transactions'], { queryParams: { categoryId: category.id } });
	}
}
