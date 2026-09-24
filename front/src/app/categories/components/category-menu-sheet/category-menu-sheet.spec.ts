import { TestBed } from '@angular/core/testing';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';
import { buildCategory } from '../../testing/category-fixture';
import { Category } from '../../types/category';
import { CategoryMenuSheet } from './category-menu-sheet';

describe('CategoryMenuSheet', () => {
	let callback: ReturnType<typeof vi.fn>;

	async function render(category: Category) {
		callback = vi.fn();
		await TestBed.configureTestingModule({
			imports: [CategoryMenuSheet],
			providers: [
				{ provide: MatBottomSheetRef, useValue: { dismiss: () => undefined } },
				{ provide: MAT_BOTTOM_SHEET_DATA, useValue: { ...category, callback } },
			],
		}).compileComponents();

		const fixture = TestBed.createComponent(CategoryMenuSheet);
		await fixture.whenStable();
		return fixture.nativeElement as HTMLElement;
	}

	function items(element: HTMLElement) {
		return Array.from(element.querySelectorAll<HTMLButtonElement>('.item'));
	}

	it('should summarize a category without transactions and allow deleting', async () => {
		const element = await render(buildCategory({ name: 'Educação', transactionCount: 0 }));

		expect(element.querySelector('.name')?.textContent?.trim()).toBe('Educação');
		expect(element.querySelector('.sub')?.textContent?.trim()).toBe('Sem transações no período');
		const deleteItem = items(element)[2];
		expect(deleteItem.textContent).toContain('Excluir');
		expect(deleteItem.textContent).not.toContain('bloqueado');
		expect(deleteItem.classList).toContain('danger');
	});

	it('should show count and balance and lock deleting when there are transactions', async () => {
		const element = await render(buildCategory({ transactionCount: 3, balance: -268 }));

		const sub = element.querySelector('.sub')?.textContent ?? '';
		expect(sub).toContain('3 transações');
		expect(sub).toContain('268,00');
		const deleteItem = items(element)[2];
		expect(deleteItem.textContent).toContain('Excluir — bloqueado');
		expect(deleteItem.classList).toContain('locked');
	});

	it('should use the singular for a single transaction', async () => {
		const element = await render(buildCategory({ transactionCount: 1, balance: 8600 }));

		expect(element.querySelector('.sub')?.textContent).toContain('1 transação ·');
	});

	it.each([
		[0, 'edit'],
		[1, 'view-transactions'],
		[2, 'delete'],
	])('item %i should report the "%s" action', async (index, action) => {
		const element = await render(buildCategory());

		items(element)[index].click();

		expect(callback).toHaveBeenCalledWith(action, expect.any(CategoryMenuSheet));
	});
});
