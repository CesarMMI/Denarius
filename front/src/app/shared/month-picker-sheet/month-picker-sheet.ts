import { DatePipe } from '@angular/common';
import { Component, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { BottomSheetDirective } from '../bottom-sheet/directives/bottom-sheet.directive';
import { MonthRef } from '../types/month-ref';

export interface MonthPickerData {
	month: number;
	year: number;
	clearable: boolean;
}

@Component({
	selector: 'app-month-picker-sheet',
	templateUrl: './month-picker-sheet.html',
	styleUrl: './month-picker-sheet.scss',
	imports: [MatButtonModule, MatIconModule, DatePipe],
})
export class MonthPickerSheet extends BottomSheetDirective<MonthPickerData, MonthRef | null> {
	protected readonly panelYear = signal(this.sheetData.year);
	protected readonly monthIndexes = Array.from({ length: 12 }, (_, i) => i);
	protected readonly monthDates = this.monthIndexes.map((i) => new Date(2000, i, 1));

	protected isSelected(month: number) {
		return this.sheetData.month === month && this.panelYear() === this.sheetData.year;
	}

	protected prevYear() {
		this.panelYear.update((y) => y - 1);
	}

	protected nextYear() {
		this.panelYear.update((y) => y + 1);
	}

	protected select(month: number) {
		this.callback({ month, year: this.panelYear() });
	}

	protected clear() {
		this.callback(null);
	}
}
