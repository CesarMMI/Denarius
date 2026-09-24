import { ComponentFixture, TestBed } from '@angular/core/testing';
import { buildCategory } from '../../testing/category-fixture';
import { Category } from '../../types/category';
import { CategoryList } from './category-list';

describe('CategoryList', () => {
	let component: CategoryList;
	let fixture: ComponentFixture<CategoryList>;
	let element: HTMLElement;

	async function render(categories: Category[]) {
		fixture.componentRef.setInput('categories', categories);
		await fixture.whenStable();
	}

	function cards() {
		return Array.from(element.querySelectorAll<HTMLElement>('.card'));
	}

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [CategoryList],
		}).compileComponents();

		fixture = TestBed.createComponent(CategoryList);
		component = fixture.componentInstance;
		element = fixture.nativeElement;
		await render([]);
	});

	it('should render nothing for an empty list', () => {
		expect(cards()).toHaveLength(0);
	});

	it('should render one card per category, in the given order', async () => {
		await render([buildCategory({ id: 'a', name: 'Salário' }), buildCategory({ id: 'b', name: 'Aluguel' })]);

		expect(cards().map((c) => c.querySelector('.name')?.textContent?.trim())).toEqual(['Salário', 'Aluguel']);
	});

	it('should say there are no transactions and hide the balance when the count is zero', async () => {
		await render([buildCategory({ transactionCount: 0, balance: 0 })]);

		expect(element.querySelector('.count')?.textContent?.trim()).toBe('Sem transações no período');
		expect(element.querySelector('.balance')).toBeNull();
	});

	it('should use the singular for a single transaction', async () => {
		await render([buildCategory({ transactionCount: 1, balance: -50 })]);

		expect(element.querySelector('.count')?.textContent?.trim()).toBe('1 transação');
	});

	it('should use the plural and show a negative balance', async () => {
		await render([buildCategory({ transactionCount: 14, balance: -1842.55 })]);

		const balance = element.querySelector('.balance')!;
		expect(element.querySelector('.count')?.textContent?.trim()).toBe('14 transações');
		expect(balance.classList).toContain('negative');
		expect(balance.classList).not.toContain('positive');
		expect(balance.textContent).toContain('1.842,55');
	});

	it('should mark a positive balance', async () => {
		await render([buildCategory({ transactionCount: 2, balance: 1250 })]);

		const balance = element.querySelector('.balance')!;
		expect(balance.classList).toContain('positive');
		expect(balance.classList).not.toContain('negative');
	});

	it('should emit openCategory when the card is clicked', async () => {
		const category = buildCategory();
		const opened: Category[] = [];
		component.openCategory.subscribe((c) => opened.push(c));
		await render([category]);

		element.querySelector<HTMLButtonElement>('.card-main')!.click();

		expect(opened).toEqual([category]);
	});

	it('should emit openMenu when the menu button is clicked', async () => {
		const category = buildCategory();
		const opened: Category[] = [];
		component.openMenu.subscribe((c) => opened.push(c));
		await render([category]);

		element.querySelector<HTMLButtonElement>('.menu-btn')!.click();

		expect(opened).toEqual([category]);
	});
});
