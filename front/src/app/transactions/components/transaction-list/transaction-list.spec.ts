import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { TransactionRow } from '../../../shared/transaction-row/transaction-row';
import { softBackground } from '../../../shared/utils/soft-color';
import { TransactionList, TransactionListGroup, TransactionListRow } from './transaction-list';

describe('TransactionList', () => {
	let component: TransactionList;
	let fixture: ComponentFixture<TransactionList>;
	let element: HTMLElement;

	function buildRow(overrides: Partial<TransactionListRow> = {}): TransactionListRow {
		return {
			id: 't1',
			icon: 'north_east',
			description: 'Feira',
			descriptionMuted: false,
			categoryName: 'Mercado',
			categoryColor: '#43A047',
			value: -186.42,
			...overrides,
		};
	}

	async function render(groups: TransactionListGroup[]) {
		fixture.componentRef.setInput('groups', groups);
		await fixture.whenStable();
	}

	function rows(): TransactionRow[] {
		return fixture.debugElement.queryAll(By.directive(TransactionRow)).map((d) => d.componentInstance);
	}

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [TransactionList],
		}).compileComponents();

		fixture = TestBed.createComponent(TransactionList);
		component = fixture.componentInstance;
		element = fixture.nativeElement;
		await render([]);
	});

	it('should render nothing for an empty list', () => {
		expect(element.querySelectorAll('.group')).toHaveLength(0);
	});

	it('should render one group per entry with its label and total, in the given order', async () => {
		await render([
			{ label: '24 de setembro', total: '-R$ 198,92 no dia', items: [buildRow()] },
			{ label: '5 de setembro', total: '+R$ 8.600,00 no dia', items: [buildRow({ id: 't3' })] },
		]);

		const labels = Array.from(element.querySelectorAll('.group-header .label')).map((n) => n.textContent?.trim());
		const totals = Array.from(element.querySelectorAll('.group-header .total')).map((n) => n.textContent?.trim());
		expect(labels).toEqual(['24 de setembro', '5 de setembro']);
		expect(totals).toEqual(['-R$ 198,92 no dia', '+R$ 8.600,00 no dia']);
	});

	it('should render the rows of each group in the given order', async () => {
		await render([
			{
				label: 'Dia 1',
				total: '',
				items: [buildRow({ id: 'a', description: 'Feira' }), buildRow({ id: 'b', description: 'Pão' })],
			},
			{ label: 'Dia 2', total: '', items: [buildRow({ id: 'c', description: 'Salário' })] },
		]);

		const groups = Array.from(element.querySelectorAll('.group'));
		const descriptionsOf = (group: Element) =>
			Array.from(group.querySelectorAll('app-transaction-row .description')).map((n) => n.textContent?.trim());
		expect(groups.map(descriptionsOf)).toEqual([['Feira', 'Pão'], ['Salário']]);
	});

	it('should pass the row data and a soft category background to each row', async () => {
		await render([
			{ label: 'Dia', total: '', items: [buildRow({ icon: 'south_west', value: 8600, categoryColor: '#1E88E5' })] },
		]);

		const [row] = rows();
		expect(row.icon()).toBe('south_west');
		expect(row.description()).toBe('Feira');
		expect(row.descriptionMuted()).toBe(false);
		expect(row.meta()).toBe('Mercado');
		expect(row.metaDotColor()).toBe('#1E88E5');
		expect(row.iconColor()).toBe('#1E88E5');
		expect(row.iconBackground()).toBe(softBackground('#1E88E5'));
		expect(row.value()).toBe(8600);
		expect(row.bordered()).toBe(true);
	});

	it('should mute a row flagged as without description', async () => {
		await render([
			{ label: 'Dia', total: '', items: [buildRow({ description: 'Sem descrição', descriptionMuted: true })] },
		]);

		expect(element.querySelector('app-transaction-row .description')!.classList).toContain('muted');
	});

	it('should emit openTransaction with the id of the clicked row', async () => {
		const opened: string[] = [];
		component.openTransaction.subscribe((id) => opened.push(id));
		await render([{ label: 'Dia', total: '', items: [buildRow({ id: 'a' }), buildRow({ id: 'b' })] }]);

		element.querySelectorAll<HTMLButtonElement>('app-transaction-row button')[1].click();

		expect(opened).toEqual(['b']);
	});
});
