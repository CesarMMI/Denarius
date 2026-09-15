import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DATE_LOCALE, provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Category } from '../../../categories/types/category';
import { BottomSheetDirective } from '../../../shared/bottom-sheet/directives/bottom-sheet.directive';
import { ConfirmDeleteDialog } from '../../../shared/confirm-delete-dialog/confirm-delete-dialog';
import { Transaction } from '../../types/transaction';
import { TransactionFormResult } from '../../types/transaction-form-result';

export interface TransactionFormData {
	transaction: Transaction | undefined;
	categories: Category[];
}

export type TransactionFormOutcome = { type: 'save'; result: TransactionFormResult } | { type: 'delete'; id: string };

type TransactionSign = 'in' | 'out';

@Component({
	selector: 'app-transaction-form-sheet',
	templateUrl: './transaction-form-sheet.html',
	styleUrl: './transaction-form-sheet.scss',
	imports: [
		ReactiveFormsModule,
		MatButtonModule,
		MatDatepickerModule,
		MatFormFieldModule,
		MatIconModule,
		MatInputModule,
		MatSelectModule,
	],
	providers: [{ provide: MAT_DATE_LOCALE, useValue: 'pt-BR' }, provideNativeDateAdapter()],
})
export class TransactionFormSheet extends BottomSheetDirective<TransactionFormData, TransactionFormOutcome> {
	private readonly matDialog = inject(MatDialog);

	protected readonly categories = this.sheetData.categories;
	protected readonly isEditing = !!this.sheetData.transaction;
	protected readonly descriptionMaxLength = 255;

	protected readonly sign = signal<TransactionSign>(
		this.sheetData.transaction ? (this.sheetData.transaction.value > 0 ? 'in' : 'out') : 'out',
	);

	protected readonly form = new FormGroup({
		value: new FormControl(this.sheetData.transaction ? this.formatValue(Math.abs(this.sheetData.transaction.value)) : '', {
			nonNullable: true,
			validators: [Validators.required, Validators.pattern(/^\d+([.,]\d{1,2})?$/)],
		}),
		date: new FormControl(this.sheetData.transaction ? new Date(this.sheetData.transaction.date) : new Date(), {
			nonNullable: true,
			validators: [Validators.required],
		}),
		categoryId: new FormControl(this.sheetData.transaction?.categoryId ?? this.categories[0]?.id ?? '', {
			nonNullable: true,
			validators: [Validators.required],
		}),
		description: new FormControl(this.sheetData.transaction?.description ?? '', {
			nonNullable: true,
			validators: [Validators.maxLength(this.descriptionMaxLength)],
		}),
	});

	protected setSign(sign: TransactionSign) {
		this.sign.set(sign);
	}

	protected submit() {
		if (this.form.invalid) return this.form.markAllAsTouched();
		const raw = this.form.getRawValue();
		const magnitude = Math.abs(parseFloat(raw.value.replace(',', '.')));
		const value = this.sign() === 'out' ? -magnitude : magnitude;
		this.callback({
			type: 'save',
			result: {
				id: this.sheetData.transaction?.id,
				description: raw.description,
				categoryId: raw.categoryId,
				value,
				date: raw.date.toISOString(),
			},
		});
	}

	protected askDelete() {
		const transaction = this.sheetData.transaction;
		if (!transaction) return;
		this.matDialog
			.open(ConfirmDeleteDialog, {
				width: 'min(100%, 400px)',
				data: {
					title: 'Excluir esta transação?',
					body: 'O lançamento será removido do mês e dos totais. Você pode desfazer logo depois.',
				},
			})
			.afterClosed()
			.subscribe((confirmed: boolean) => {
				if (!confirmed) return;
				this.callback({ type: 'delete', id: transaction.id });
			});
	}

	private formatValue(value: number) {
		return value.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
	}
}
