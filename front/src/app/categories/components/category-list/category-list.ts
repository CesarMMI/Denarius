import { Component, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { formatMoney } from '../../../shared/utils/format-currency';
import { softBackground } from '../../../shared/utils/soft-color';
import { Category } from '../../types/category';

@Component({
	selector: 'app-category-list',
	templateUrl: './category-list.html',
	styleUrl: './category-list.scss',
	imports: [MatIconModule],
})
export class CategoryList {
	readonly categories = input.required<Category[]>();

	readonly openCategory = output<Category>();
	readonly openMenu = output<Category>();

	protected readonly softBackground = softBackground;
	protected readonly formatMoney = formatMoney;
}
