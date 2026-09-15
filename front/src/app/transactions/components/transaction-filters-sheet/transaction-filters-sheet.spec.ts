import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';
import { MOCK_CATEGORIES, MOCK_TRANSACTIONS } from '../../../shared/mock-data/mock-data';
import { DEFAULT_TRANSACTION_FILTERS } from '../../types/transaction-filters';
import { TransactionFiltersSheet } from './transaction-filters-sheet';

describe('TransactionFiltersSheet', () => {
	let component: TransactionFiltersSheet;
	let fixture: ComponentFixture<TransactionFiltersSheet>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [TransactionFiltersSheet],
			providers: [
				{ provide: MatBottomSheetRef, useValue: { dismiss: () => undefined } },
				{
					provide: MAT_BOTTOM_SHEET_DATA,
					useValue: { filters: DEFAULT_TRANSACTION_FILTERS, transactions: MOCK_TRANSACTIONS, categories: MOCK_CATEGORIES },
				},
			],
		}).compileComponents();

		fixture = TestBed.createComponent(TransactionFiltersSheet);
		component = fixture.componentInstance;
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
