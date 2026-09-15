import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BalanceSummaryCard } from './balance-summary-card';

describe('BalanceSummaryCard', () => {
	let component: BalanceSummaryCard;
	let fixture: ComponentFixture<BalanceSummaryCard>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [BalanceSummaryCard],
		}).compileComponents();

		fixture = TestBed.createComponent(BalanceSummaryCard);
		component = fixture.componentInstance;
		fixture.componentRef.setInput('periodLabel', 'setembro 2026');
		fixture.componentRef.setInput('net', 1000);
		fixture.componentRef.setInput('totalIn', 8600);
		fixture.componentRef.setInput('totalOut', -7600);
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
