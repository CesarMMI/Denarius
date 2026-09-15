import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PageHeader } from './page-header';

describe('PageHeader', () => {
	let component: PageHeader;
	let fixture: ComponentFixture<PageHeader>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [PageHeader],
		}).compileComponents();

		fixture = TestBed.createComponent(PageHeader);
		component = fixture.componentInstance;
		fixture.componentRef.setInput('greeting', 'Setembro em curso');
		fixture.componentRef.setInput('title', 'Resumo');
		fixture.componentRef.setInput('actionLabel', 'Transação');
		fixture.componentRef.setInput('monthLabel', 'Setembro 2026');
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
