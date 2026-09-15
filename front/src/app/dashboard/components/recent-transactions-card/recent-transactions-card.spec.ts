import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RecentTransactionsCard } from './recent-transactions-card';

describe('RecentTransactionsCard', () => {
	let component: RecentTransactionsCard;
	let fixture: ComponentFixture<RecentTransactionsCard>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [RecentTransactionsCard],
		}).compileComponents();

		fixture = TestBed.createComponent(RecentTransactionsCard);
		component = fixture.componentInstance;
		fixture.componentRef.setInput('periodLabel', 'setembro 2026');
		fixture.componentRef.setInput('transactions', []);
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
