import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';
import { MonthPickerSheet } from './month-picker-sheet';

describe('MonthPickerSheet', () => {
	let component: MonthPickerSheet;
	let fixture: ComponentFixture<MonthPickerSheet>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [MonthPickerSheet],
			providers: [
				{ provide: MatBottomSheetRef, useValue: { dismiss: () => undefined } },
				{ provide: MAT_BOTTOM_SHEET_DATA, useValue: { month: 8, year: 2026, clearable: true } },
			],
		}).compileComponents();

		fixture = TestBed.createComponent(MonthPickerSheet);
		component = fixture.componentInstance;
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
