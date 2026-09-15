import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { BottomSheetDirective } from '../../../shared/bottom-sheet/directives/bottom-sheet.directive';
import { formatMoney } from '../../../shared/utils/format-currency';
import { softBackground } from '../../../shared/utils/soft-color';
import { Category } from '../../types/category';

export type CategoryMenuAction = 'edit' | 'view-transactions' | 'delete';

@Component({
	selector: 'app-category-menu-sheet',
	templateUrl: './category-menu-sheet.html',
	styleUrl: './category-menu-sheet.scss',
	imports: [MatIconModule],
})
export class CategoryMenuSheet extends BottomSheetDirective<Category, CategoryMenuAction> {
	protected readonly category = this.sheetData;
	protected readonly softBackground = softBackground;
	protected readonly formattedBalance = formatMoney(this.category.balance);

	protected select(action: CategoryMenuAction) {
		this.callback(action);
	}
}
