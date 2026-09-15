import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CategoryBreakdownCard } from './category-breakdown-card';

describe('CategoryBreakdownCard', () => {
	let component: CategoryBreakdownCard;
	let fixture: ComponentFixture<CategoryBreakdownCard>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [CategoryBreakdownCard],
		}).compileComponents();

		fixture = TestBed.createComponent(CategoryBreakdownCard);
		component = fixture.componentInstance;
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
