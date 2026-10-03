import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Sort } from '@angular/material/sort';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import { CategoriesService } from '../../../categories/services/categories.service';
import { Category } from '../../../categories/types/category';
import { PageHeader } from '../../../shared/page-header/page-header';
import { SortMenu, SortOption } from '../../../shared/sort-menu/sort-menu';
import { TransactionForm, TransactionFormData } from '../../components/transaction-form/transaction-form';
import { TransactionsFilters } from '../../components/transactions-filters/transactions-filters';
import { TransactionsTable } from '../../components/transactions-table/transactions-table';
import { TransactionsService } from '../../services/transactions.service';
import { Transaction, TransactionInput } from '../../types/transaction';
import { TransactionFilters } from '../../types/transaction-filters';

@Component({
	selector: 'app-transactions-page',
	templateUrl: './transactions-page.html',
	styleUrl: './transactions-page.scss',
	imports: [
		MatButtonModule,
		MatCardModule,
		MatIconModule,
		MatTooltipModule,
		PageHeader,
		SortMenu,
		TransactionsFilters,
		TransactionsTable,
	],
})
export class TransactionsPage {
	private readonly transactionsService = inject(TransactionsService);
	private readonly categoriesService = inject(CategoriesService);
	private readonly dialog = inject(MatDialog);
	private readonly snackBar = inject(MatSnackBar);

	/** The categories page links here with a category, and the filters start open to show it. */
	private readonly categoryId = inject(ActivatedRoute).snapshot.queryParamMap.get('categoryId') ?? '';

	protected readonly filters = signal<TransactionFilters>({
		description: '',
		type: '',
		categoryId: this.categoryId,
		month: null,
	});
	protected readonly filtersVisible = signal<boolean>(!!this.categoryId);

	protected readonly sort = signal<Sort>({ active: 'date', direction: 'desc' });
	protected readonly sortOptions: SortOption[] = [
		{ active: 'date', direction: 'desc', label: 'Mais recentes primeiro' },
		{ active: 'date', direction: 'asc', label: 'Mais antigos primeiro' },
		{ active: 'description', direction: 'asc', label: 'Descrição (A–Z)' },
		{ active: 'description', direction: 'desc', label: 'Descrição (Z–A)' },
		{ active: 'categoryName', direction: 'asc', label: 'Categoria (A–Z)' },
		{ active: 'categoryName', direction: 'desc', label: 'Categoria (Z–A)' },
		{ active: 'value', direction: 'desc', label: 'Maior valor primeiro' },
		{ active: 'value', direction: 'asc', label: 'Menor valor primeiro' },
	];

	protected readonly transactions = httpResource<Transaction[]>(() =>
		this.transactionsService.list(this.filters(), this.sort()),
	);
	/** Name and color the rows, and feed the filters and the form. */
	protected readonly categories = httpResource<Category[]>(() => this.categoriesService.list());
	protected readonly categoryList = computed(() => (this.categories.hasValue() ? this.categories.value() : []));

	protected reload() {
		this.transactions.reload();
		this.categories.reload();
	}

	protected openForm(transaction?: Transaction) {
		if (this.categories.error()) {
			this.snackBar.open('Não foi possível carregar as categorias. Tente novamente.', 'Fechar', { duration: 5000 });
			return;
		}
		const data: TransactionFormData = { transaction, categories: this.categoryList() };
		this.dialog
			.open(TransactionForm, { data })
			.afterClosed()
			.subscribe((input?: TransactionInput) => {
				if (!input) return;
				if (transaction) this.save(this.transactionsService.update(transaction.id, input), 'Transação salva.');
				else this.save(this.transactionsService.create(input), 'Transação criada.');
			});
	}

	protected delete({ id, description, categoryId, value, date }: Transaction) {
		this.transactionsService.delete(id).subscribe({
			next: () => {
				this.transactions.reload();
				// The API has no undelete, so undoing creates the transaction again.
				this.snackBar
					.open('Transação excluída.', 'Desfazer', { duration: 5000 })
					.onAction()
					.subscribe(() =>
						this.save(this.transactionsService.create({ description, categoryId, value, date }), 'Transação restaurada.'),
					);
			},
			error: (error: HttpErrorResponse) => this.showError(error, 'Não foi possível excluir a transação.'),
		});
	}

	private save(request: Observable<unknown>, message: string) {
		request.subscribe({
			next: () => {
				this.snackBar.open(message, undefined, { duration: 3000 });
				this.transactions.reload();
			},
			error: (error: HttpErrorResponse) => this.showError(error, 'Não foi possível salvar a transação.'),
		});
	}

	private showError(error: HttpErrorResponse, fallback: string) {
		this.snackBar.open(error.error?.detail ?? fallback, 'Fechar', { duration: 5000 });
	}
}
