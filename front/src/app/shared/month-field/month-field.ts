import { booleanAttribute, Component, input, model } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_NATIVE_DATE_FORMATS, provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

/** Picks a month, as its first day, from the year view of the datepicker. */
@Component({
	selector: 'app-month-field',
	templateUrl: './month-field.html',
	styleUrl: './month-field.scss',
	imports: [MatButtonModule, MatDatepickerModule, MatFormFieldModule, MatIconModule, MatInputModule],
	providers: [
		provideNativeDateAdapter({
			...MAT_NATIVE_DATE_FORMATS,
			display: { ...MAT_NATIVE_DATE_FORMATS.display, dateInput: { month: '2-digit', year: 'numeric' } },
		}),
	],
})
export class MonthField {
	readonly value = model<Date | null>(null);
	/** Lets the month be cleared, to mean every month. */
	readonly clearable = input(false, { transform: booleanAttribute });
}
