import { resourceFromSnapshots, ResourceSnapshot, signal, WritableSignal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { Scale, TooltipItem, TooltipModel } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { ChartColors, ChartThemeService } from '../../services/chart-theme.service';
import { FakeChart } from '../../testing/fake-chart';
import { buildIncomeVsExpense } from '../../testing/report-fixtures';
import { IncomeVsExpense } from '../../types/report';
import { IncomeVsExpenseChart } from './income-vs-expense-chart';

describe('IncomeVsExpenseChart', () => {
	const colors: ChartColors = {
		expense: 'red',
		income: 'green',
		neutral: 'gray',
		primary: 'amber',
		primaryVariant: 'brown',
		text: 'ink',
		grid: 'line',
		surface: 'card',
	};
	const months = [
		buildIncomeVsExpense({ month: '2026-08', income: 8000, expense: 4000, balance: 4000 }),
		buildIncomeVsExpense({ month: '2026-09', income: 0, expense: 0, balance: 0 }),
		buildIncomeVsExpense({ month: '2026-10', income: 8000, expense: 2210.5, balance: 5789.5 }),
	];

	let fixture: ComponentFixture<IncomeVsExpenseChart>;
	let element: HTMLElement;
	let series: WritableSignal<ResourceSnapshot<IncomeVsExpense[] | undefined>>;

	beforeEach(async () => {
		TestBed.configureTestingModule({ providers: [{ provide: ChartThemeService, useValue: { colors: signal(colors) } }] });
		TestBed.overrideComponent(IncomeVsExpenseChart, {
			remove: { imports: [BaseChartDirective] },
			add: { imports: [FakeChart] },
		});
		series = signal({ status: 'resolved', value: months });

		fixture = TestBed.createComponent(IncomeVsExpenseChart);
		element = fixture.nativeElement;
		fixture.componentRef.setInput('series', resourceFromSnapshots(series));
		await fixture.whenStable();
	});

	function chart() {
		return fixture.debugElement.query(By.directive(FakeChart))?.injector.get(FakeChart);
	}

	function text(node: Element | null) {
		return node?.textContent?.replace(/\s+/g, ' ').trim();
	}

	it('should draw income and expenses side by side for every month, quiet months included', () => {
		const data = chart()!.data()!;

		expect(chart()!.type()).toBe('bar');
		expect(data.labels).toEqual([['ago.', '2026'], 'set.', 'out.']);
		expect(data.datasets.map((dataset) => dataset.label)).toEqual(['Receitas', 'Despesas']);
		expect(data.datasets[0].data).toEqual([8000, 0, 8000]);
		expect(data.datasets[1].data).toEqual([4000, 0, 2210.5]);
		expect(chart()!.options()!.scales!['x']!.ticks).toEqual(expect.objectContaining({ maxRotation: 0 }));
	});

	it('should write the year under the first month and every January', async () => {
		series.set({
			status: 'resolved',
			value: ['2025-12', '2026-01', '2026-02'].map((month) => buildIncomeVsExpense({ month })),
		});
		await fixture.whenStable();

		expect(chart()!.data()!.labels).toEqual([['dez.', '2025'], ['jan.', '2026'], 'fev.']);
	});

	it('should paint income green and expenses red, in thin bars with rounded tops', () => {
		const [income, expense] = chart()!.data()!.datasets;

		expect(income).toEqual(
			expect.objectContaining({ backgroundColor: 'green', maxBarThickness: 24, borderRadius: 4, borderSkipped: 'start' }),
		);
		expect(expense).toEqual(expect.objectContaining({ backgroundColor: 'red' }));
	});

	it('should write the value axis in compact BRL, in five steps', () => {
		const y = chart()!.options()!.scales!['y']!;
		const tick = y.ticks!.callback!.call({} as Scale, 1200, 0, []);

		expect(String(tick).replace(/\s/g, ' ')).toBe('R$ 1,2 mil');
		expect(y.ticks).toEqual(expect.objectContaining({ count: 5 }));
	});

	it('should spell the month out and give both values in BRL in the tooltip', () => {
		const callbacks = chart()!.options()!.plugins!.tooltip!.callbacks!;
		const item = { dataIndex: 2, dataset: { label: 'Despesas' }, parsed: { x: 2, y: 2210.5 } } as TooltipItem<'bar'>;

		expect(callbacks.title!.call({} as TooltipModel<'bar'>, [item])).toBe('outubro de 2026');
		expect(String(callbacks.label!.call({} as TooltipModel<'bar'>, item)).replace(/\s/g, ' ')).toBe(
			'Despesas: R$ 2.210,50',
		);
		expect(chart()!.options()!.interaction).toEqual({ mode: 'index', intersect: false });
	});

	it('should say when the whole period has no movement', async () => {
		series.set({ status: 'resolved', value: [buildIncomeVsExpense({ income: 0, expense: 0, balance: 0 })] });
		await fixture.whenStable();

		expect(chart()).toBeUndefined();
		expect(text(element.querySelector('mat-card-content p'))).toBe('Sem movimentações neste período.');
	});

	it('should say when the series fails to load, and retry it', async () => {
		const retry = vi.fn();
		fixture.componentInstance.retry.subscribe(retry);
		series.set({ status: 'error', error: new Error('Server Error') });
		await fixture.whenStable();

		expect(chart()).toBeUndefined();
		expect(text(element.querySelector('mat-card-content p'))).toBe(
			'Não foi possível carregar as receitas e despesas dos últimos meses.',
		);
		Array.from(element.querySelectorAll('button'))
			.find((b) => b.textContent?.includes('Tentar novamente'))!
			.click();
		expect(retry).toHaveBeenCalledOnce();
	});
});
