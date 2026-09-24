import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CategoryBlockedDialog } from './category-blocked-dialog';

describe('CategoryBlockedDialog', () => {
	let close: ReturnType<typeof vi.fn>;

	async function render(transactionCount: number) {
		close = vi.fn();
		await TestBed.configureTestingModule({
			imports: [CategoryBlockedDialog],
			providers: [
				{ provide: MatDialogRef, useValue: { close } },
				{ provide: MAT_DIALOG_DATA, useValue: { categoryName: 'Mercado', transactionCount } },
			],
		}).compileComponents();

		const fixture = TestBed.createComponent(CategoryBlockedDialog);
		await fixture.whenStable();
		return fixture.nativeElement as HTMLElement;
	}

	function buttons(element: HTMLElement) {
		return Array.from(element.querySelectorAll<HTMLButtonElement>('.actions button'));
	}

	it('should name the category in the title', async () => {
		const element = await render(14);

		expect(element.querySelector('h1')?.textContent).toContain('"Mercado"');
	});

	it('should use the plural for several transactions', async () => {
		const element = await render(14);

		expect(element.querySelector('p')?.textContent).toContain('Existem 14 transações vinculadas');
	});

	it('should use the singular for a single transaction', async () => {
		const element = await render(1);

		expect(element.querySelector('p')?.textContent).toContain('Existe 1 transação vinculada');
	});

	it('should close with view-transactions when asked to see the transactions', async () => {
		const element = await render(14);

		buttons(element)[0].click();

		expect(close).toHaveBeenCalledWith('view-transactions');
	});

	it('should close without a result when dismissed', async () => {
		const element = await render(14);

		buttons(element)[1].click();

		expect(close).toHaveBeenCalledWith();
	});
});
