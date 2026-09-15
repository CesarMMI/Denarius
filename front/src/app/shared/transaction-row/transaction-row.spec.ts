import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TransactionRow } from './transaction-row';

describe('TransactionRow', () => {
	let component: TransactionRow;
	let fixture: ComponentFixture<TransactionRow>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [TransactionRow],
		}).compileComponents();

		fixture = TestBed.createComponent(TransactionRow);
		component = fixture.componentInstance;
		fixture.componentRef.setInput('icon', 'north_east');
		fixture.componentRef.setInput('description', 'Feira da semana');
		fixture.componentRef.setInput('meta', '28/09 · Mercado');
		fixture.componentRef.setInput('value', -186.42);
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
