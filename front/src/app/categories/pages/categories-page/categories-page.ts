import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Sort } from '@angular/material/sort';
import { MatTooltipModule } from '@angular/material/tooltip';
import { finalize, Observable } from 'rxjs';
import { PageHeader } from '../../../shared/page-header/page-header';
import { reloadWhenIdle } from '../../../shared/reload-when-idle/reload-when-idle';
import { shallowEqual } from '../../../shared/shallow-equal/shallow-equal';
import { SortMenu, SortOption } from '../../../shared/sort-menu/sort-menu';
import { CategoriesFilters } from '../../components/categories-filters/categories-filters';
import { CategoriesTable } from '../../components/categories-table/categories-table';
import { CategoryForm } from '../../components/category-form/category-form';
import { CategoriesService } from '../../services/categories.service';
import { Category, CategoryInput } from '../../types/category';
import { CategoryFilters } from '../../types/category-filters';

@Component({
	selector: 'app-categories-page',
	templateUrl: './categories-page.html',
	styleUrl: './categories-page.scss',
	imports: [
		MatButtonModule,
		MatCardModule,
		MatIconModule,
		MatProgressBarModule,
		MatTooltipModule,
		PageHeader,
		SortMenu,
		CategoriesFilters,
		CategoriesTable,
	],
})
export class CategoriesPage {
	private readonly categoriesService = inject(CategoriesService);
	private readonly dialog = inject(MatDialog);
	private readonly snackBar = inject(MatSnackBar);

	/** The filters and the sort only notify, and reload the list, when their content changes. */
	protected readonly filters = signal<CategoryFilters>(
		{ name: '', withTransaction: '', month: null },
		{ equal: shallowEqual },
	);
	protected readonly filtersVisible = signal<boolean>(false);

	protected readonly sort = signal<Sort>({ active: 'name', direction: 'asc' }, { equal: shallowEqual });
	protected readonly sortOptions: SortOption[] = [
		{ active: 'name', direction: 'asc', label: 'Nome (A–Z)' },
		{ active: 'name', direction: 'desc', label: 'Nome (Z–A)' },
		{ active: 'transactionCount', direction: 'desc', label: 'Mais transações primeiro' },
		{ active: 'transactionCount', direction: 'asc', label: 'Menos transações primeiro' },
		{ active: 'balance', direction: 'desc', label: 'Maior saldo primeiro' },
		{ active: 'balance', direction: 'asc', label: 'Menor saldo primeiro' },
	];

	protected readonly categories = httpResource<Category[]>(() =>
		this.categoriesService.list(this.filters(), this.sort()),
	);
	/** Reloads after a change the API accepted, even while the list is already loading. */
	private readonly reloadCategories = reloadWhenIdle(this.categories);

	/** The ids of the categories being deleted, from the click until they leave the list or the API refuses. */
	protected readonly deleting = signal<ReadonlySet<string>>(new Set());
	/** How many undone deletions are being restored; they may overlap. */
	protected readonly restoring = signal(0);

	protected openForm(category?: Category) {
		this.dialog
			.open(CategoryForm, { data: category })
			.afterClosed()
			.subscribe((input?: CategoryInput) => {
				if (!input) return;
				if (category) this.save(this.categoriesService.update(category.id, input), 'Categoria salva.');
				else this.save(this.categoriesService.create(input), 'Categoria criada.');
			});
	}

	/** The API refuses to delete a category with transactions, and its error says so. */
	protected delete({ id, name, color }: Category) {
		// Another request for the same row would be refused, and its error would replace the offer to undo.
		if (this.deleting().has(id)) return;
		this.deleting.update((ids) => new Set(ids).add(id));
		this.categoriesService.delete(id).subscribe({
			// The id stays in `deleting`, so the row stays blocked until it leaves the list: ids are never reused.
			next: () => {
				this.reloadCategories();
				// The API has no undelete, so undoing creates the category again.
				this.snackBar
					.open('Categoria excluída.', 'Desfazer', { duration: 5000 })
					.onAction()
					.subscribe(() => this.restore(this.categoriesService.create({ name, color })));
			},
			error: (error: HttpErrorResponse) => {
				this.deleting.update((ids) => new Set([...ids].filter((other) => other !== id)));
				this.showError(error, 'Não foi possível excluir a categoria.');
			},
		});
	}

	private restore(request: Observable<unknown>) {
		this.restoring.update((count) => count + 1);
		this.save(request.pipe(finalize(() => this.restoring.update((count) => count - 1))), 'Categoria restaurada.');
	}

	private save(request: Observable<unknown>, message: string) {
		request.subscribe({
			next: () => {
				this.snackBar.open(message, undefined, { duration: 3000 });
				this.reloadCategories();
			},
			error: (error: HttpErrorResponse) => this.showError(error, 'Não foi possível salvar a categoria.'),
		});
	}

	private showError(error: HttpErrorResponse, fallback: string) {
		this.snackBar.open(error.error?.detail ?? fallback, 'Fechar', { duration: 5000 });
	}
}
