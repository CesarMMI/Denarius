import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { BottomSheetDirective } from '../../../shared/bottom-sheet/directives/bottom-sheet.directive';
import { Category } from '../../types/category';
import { CategoryFormResult } from '../../types/category-form-result';
import { CategoryColorPicker } from '../category-color-picker/category-color-picker';

export interface CategoryFormData {
	category: Category | undefined;
}

export type CategoryFormOutcome = { type: 'save'; result: CategoryFormResult } | { type: 'delete'; category: Category };

@Component({
	selector: 'app-category-form-sheet',
	templateUrl: './category-form-sheet.html',
	styleUrl: './category-form-sheet.scss',
	imports: [ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule, CategoryColorPicker],
})
export class CategoryFormSheet extends BottomSheetDirective<CategoryFormData, CategoryFormOutcome> {
	protected readonly isEditing = !!this.sheetData.category;
	protected readonly nameMaxLength = 100;

	protected readonly form = new FormGroup({
		name: new FormControl(this.sheetData.category?.name ?? '', {
			nonNullable: true,
			validators: [Validators.required, Validators.maxLength(this.nameMaxLength)],
		}),
		color: new FormControl(this.sheetData.category?.color ?? '#43A047', {
			nonNullable: true,
			validators: [Validators.required],
		}),
	});

	protected submit() {
		if (this.form.invalid) return this.form.markAllAsTouched();
		const raw = this.form.getRawValue();
		this.callback({ type: 'save', result: { ...raw, id: this.sheetData.category?.id } });
	}

	protected askDelete() {
		const category = this.sheetData.category;
		if (!category) return;
		this.callback({ type: 'delete', category });
	}
}
