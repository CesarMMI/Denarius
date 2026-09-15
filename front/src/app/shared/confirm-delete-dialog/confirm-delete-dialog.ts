import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

export interface ConfirmDeleteDialogData {
	title: string;
	body: string;
}

@Component({
	selector: 'app-confirm-delete-dialog',
	templateUrl: './confirm-delete-dialog.html',
	styleUrl: './confirm-delete-dialog.scss',
	imports: [MatButtonModule],
})
export class ConfirmDeleteDialog {
	protected readonly data = inject<ConfirmDeleteDialogData>(MAT_DIALOG_DATA);
	private readonly dialogRef = inject<MatDialogRef<ConfirmDeleteDialog, boolean>>(MatDialogRef);

	protected cancel() {
		this.dialogRef.close(false);
	}

	protected confirm() {
		this.dialogRef.close(true);
	}
}
