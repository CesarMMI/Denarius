import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ConfirmDeleteDialog } from './confirm-delete-dialog';

describe('ConfirmDeleteDialog', () => {
	let component: ConfirmDeleteDialog;
	let fixture: ComponentFixture<ConfirmDeleteDialog>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [ConfirmDeleteDialog],
			providers: [
				{ provide: MatDialogRef, useValue: { close: () => undefined } },
				{ provide: MAT_DIALOG_DATA, useValue: { title: 'Excluir?', body: 'Esta ação não pode ser desfeita.' } },
			],
		}).compileComponents();

		fixture = TestBed.createComponent(ConfirmDeleteDialog);
		component = fixture.componentInstance;
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
