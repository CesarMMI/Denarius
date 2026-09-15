import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';
import { MOCK_CATEGORIES } from '../../../shared/mock-data/mock-data';
import { CategoryMenuSheet } from './category-menu-sheet';

describe('CategoryMenuSheet', () => {
	let component: CategoryMenuSheet;
	let fixture: ComponentFixture<CategoryMenuSheet>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [CategoryMenuSheet],
			providers: [
				{ provide: MatBottomSheetRef, useValue: { dismiss: () => undefined } },
				{ provide: MAT_BOTTOM_SHEET_DATA, useValue: MOCK_CATEGORIES[0] },
			],
		}).compileComponents();

		fixture = TestBed.createComponent(CategoryMenuSheet);
		component = fixture.componentInstance;
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
