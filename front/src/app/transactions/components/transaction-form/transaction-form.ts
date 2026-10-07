import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { DateAdapter, provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerIntl, MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Category } from '../../../categories/types/category';
import { DateUtils } from '../../../shared/date-utils/date-utils';
import { PtBrDatepickerIntl } from '../../../shared/datepicker-intl/pt-br-datepicker-intl';
import { Transaction, TransactionInput } from '../../types/transaction';
import { PtBrDateAdapter } from './pt-br-date-adapter';

export interface TransactionFormData {
	transaction?: Transaction;
	categories: Category[];
}

/** A dialog that closes with the transaction to save, or with nothing when cancelled. */
@Component({
	selector: 'app-transaction-form',
	templateUrl: './transaction-form.html',
	styleUrl: './transaction-form.scss',
	imports: [
		ReactiveFormsModule,
		MatButtonModule,
		MatButtonToggleModule,
		MatDatepickerModule,
		MatDialogModule,
		MatFormFieldModule,
		MatInputModule,
		MatSelectModule,
	],
	providers: [
		provideNativeDateAdapter(),
		{ provide: DateAdapter, useClass: PtBrDateAdapter },
		{ provide: MatDatepickerIntl, useClass: PtBrDatepickerIntl },
	],
})
export class TransactionForm {
	protected readonly data = inject<TransactionFormData>(MAT_DIALOG_DATA);
	private readonly dialogRef = inject<MatDialogRef<TransactionForm, TransactionInput>>(MatDialogRef);

	protected readonly descriptionMaxLength = 255;

	private readonly transaction = this.data.transaction;
	protected readonly form = new FormGroup({
		type: new FormControl(this.transaction && this.transaction.value > 0 ? 'in' : 'out', { nonNullable: true }),
		value: new FormControl(this.transaction ? Math.abs(this.transaction.value).toFixed(2).replace('.', ',') : '', {
			nonNullable: true,
			validators: [Validators.required, Validators.pattern(/^\d+([.,]\d{1,2})?$/)],
		}),
		date: new FormControl(this.transaction ? DateUtils.fromApiDate(this.transaction.date) : new Date(), {
			nonNullable: true,
			validators: [Validators.required],
		}),
		categoryId: new FormControl(this.transaction?.categoryId ?? this.data.categories[0]?.id ?? '', {
			nonNullable: true,
			validators: [Validators.required],
		}),
		description: new FormControl(this.transaction?.description ?? '', { nonNullable: true }),
	});

	protected submit() {
		if (this.form.invalid) return;
		const { type, value, date, categoryId, description } = this.form.getRawValue();
		const amount = parseFloat(value.replace(',', '.'));
		this.dialogRef.close({
			description: description.trim() || null,
			categoryId,
			value: type === 'out' ? -amount : amount,
			date: DateUtils.toApiDate(date),
		});
	}
}
