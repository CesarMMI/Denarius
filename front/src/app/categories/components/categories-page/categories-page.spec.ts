import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { CategoriesPage } from './categories-page';

describe('CategoriesPage', () => {
	let component: CategoriesPage;
	let fixture: ComponentFixture<CategoriesPage>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [CategoriesPage],
			providers: [provideRouter([])],
		}).compileComponents();

		fixture = TestBed.createComponent(CategoriesPage);
		component = fixture.componentInstance;
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
