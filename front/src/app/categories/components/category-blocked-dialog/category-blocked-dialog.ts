import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

export interface CategoryBlockedDialogData {
	categoryName: string;
	transactionCount: number;
}

export type CategoryBlockedDialogResult = 'view-transactions' | undefined;

@Component({
	selector: 'app-category-blocked-dialog',
	templateUrl: './category-blocked-dialog.html',
	styleUrl: './category-blocked-dialog.scss',
	imports: [MatButtonModule, MatIconModule],
})
export class CategoryBlockedDialog {
	protected readonly data = inject<CategoryBlockedDialogData>(MAT_DIALOG_DATA);
	private readonly dialogRef = inject<MatDialogRef<CategoryBlockedDialog, CategoryBlockedDialogResult>>(MatDialogRef);

	protected readonly blockedPhrase =
		this.data.transactionCount === 1 ? 'Existe 1 transação vinculada' : `Existem ${this.data.transactionCount} transações vinculadas`;

	protected viewTransactions() {
		this.dialogRef.close('view-transactions');
	}

	protected dismiss() {
		this.dialogRef.close();
	}
}
