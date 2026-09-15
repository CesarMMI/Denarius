import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CategoryList } from './category-list';

describe('CategoryList', () => {
	let component: CategoryList;
	let fixture: ComponentFixture<CategoryList>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [CategoryList],
		}).compileComponents();

		fixture = TestBed.createComponent(CategoryList);
		component = fixture.componentInstance;
		fixture.componentRef.setInput('categories', []);
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
