import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CategoryBlockedDialog } from './category-blocked-dialog';

describe('CategoryBlockedDialog', () => {
	let component: CategoryBlockedDialog;
	let fixture: ComponentFixture<CategoryBlockedDialog>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [CategoryBlockedDialog],
			providers: [
				{ provide: MatDialogRef, useValue: { close: () => undefined } },
				{ provide: MAT_DIALOG_DATA, useValue: { categoryName: 'Mercado', transactionCount: 14 } },
			],
		}).compileComponents();

		fixture = TestBed.createComponent(CategoryBlockedDialog);
		component = fixture.componentInstance;
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
