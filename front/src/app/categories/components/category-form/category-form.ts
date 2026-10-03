import { Component, effect, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { ColorsService } from '../../services/colors.service';
import { Category, CategoryInput } from '../../types/category';

/** A dialog that closes with the category to save, or with nothing when cancelled. */
@Component({
	selector: 'app-category-form',
	templateUrl: './category-form.html',
	styleUrl: './category-form.scss',
	imports: [ReactiveFormsModule, MatButtonModule, MatDialogModule, MatFormFieldModule, MatIconModule, MatInputModule],
})
export class CategoryForm {
	protected readonly category = inject<Category | undefined>(MAT_DIALOG_DATA);
	private readonly dialogRef = inject<MatDialogRef<CategoryForm, CategoryInput>>(MatDialogRef);
	private readonly colorsService = inject(ColorsService);

	protected readonly nameMaxLength = 100;

	protected readonly colors = this.colorsService.colors;

	protected readonly form = new FormGroup({
		name: new FormControl(this.category?.name ?? '', { nonNullable: true, validators: [Validators.required] }),
		color: new FormControl(this.category?.color ?? '#000000', { nonNullable: true }),
	});

	constructor() {
		// A new category starts with the first color of the palette.
		effect(() => {
			const [first] = this.colors();
			if (first && !this.category) this.form.controls.color.setValue(first);
		});
	}

	protected submit() {
		if (this.form.valid) this.dialogRef.close(this.form.getRawValue());
	}
}
