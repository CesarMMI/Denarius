import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';
import { MOCK_CATEGORIES } from '../../../shared/mock-data/mock-data';
import { DEFAULT_CATEGORY_FILTERS } from '../../types/category-filters';
import { CATEGORY_SORT_OPTIONS } from '../../types/category-sort';
import { CategoryFiltersSheet } from './category-filters-sheet';

describe('CategoryFiltersSheet', () => {
	let component: CategoryFiltersSheet;
	let fixture: ComponentFixture<CategoryFiltersSheet>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [CategoryFiltersSheet],
			providers: [
				{ provide: MatBottomSheetRef, useValue: { dismiss: () => undefined } },
				{
					provide: MAT_BOTTOM_SHEET_DATA,
					useValue: { filters: DEFAULT_CATEGORY_FILTERS, sort: CATEGORY_SORT_OPTIONS[0], categories: MOCK_CATEGORIES },
				},
			],
		}).compileComponents();

		fixture = TestBed.createComponent(CategoryFiltersSheet);
		component = fixture.componentInstance;
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
