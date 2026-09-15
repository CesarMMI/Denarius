import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';
import { MOCK_CATEGORIES } from '../../../shared/mock-data/mock-data';
import { TransactionFormSheet } from './transaction-form-sheet';

describe('TransactionFormSheet', () => {
	let component: TransactionFormSheet;
	let fixture: ComponentFixture<TransactionFormSheet>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [TransactionFormSheet],
			providers: [
				{ provide: MatBottomSheetRef, useValue: { dismiss: () => undefined } },
				{ provide: MAT_BOTTOM_SHEET_DATA, useValue: { transaction: undefined, categories: MOCK_CATEGORIES } },
			],
		}).compileComponents();

		fixture = TestBed.createComponent(TransactionFormSheet);
		component = fixture.componentInstance;
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
