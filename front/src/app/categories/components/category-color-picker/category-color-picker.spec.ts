import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CategoryColorPicker } from './category-color-picker';

describe('CategoryColorPicker', () => {
	let component: CategoryColorPicker;
	let fixture: ComponentFixture<CategoryColorPicker>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [CategoryColorPicker],
		}).compileComponents();

		fixture = TestBed.createComponent(CategoryColorPicker);
		component = fixture.componentInstance;
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
