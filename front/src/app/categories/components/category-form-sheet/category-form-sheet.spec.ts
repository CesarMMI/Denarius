import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';
import { CategoryFormSheet } from './category-form-sheet';

describe('CategoryFormSheet', () => {
	let component: CategoryFormSheet;
	let fixture: ComponentFixture<CategoryFormSheet>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [CategoryFormSheet],
			providers: [
				{ provide: MatBottomSheetRef, useValue: { dismiss: () => undefined } },
				{ provide: MAT_BOTTOM_SHEET_DATA, useValue: { category: undefined } },
			],
		}).compileComponents();

		fixture = TestBed.createComponent(CategoryFormSheet);
		component = fixture.componentInstance;
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
