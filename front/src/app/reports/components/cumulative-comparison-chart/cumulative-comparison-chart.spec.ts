import { resourceFromSnapshots, ResourceSnapshot, signal, WritableSignal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { TooltipItem, TooltipModel } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { ChartColors, ChartThemeService } from '../../services/chart-theme.service';
import { FakeChart } from '../../testing/fake-chart';
import { buildCumulativeExpenseComparison } from '../../testing/report-fixtures';
import { CumulativeExpenseComparison } from '../../types/report';
import { CumulativeComparisonChart } from './cumulative-comparison-chart';

describe('CumulativeComparisonChart', () => {
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

	let fixture: ComponentFixture<CumulativeComparisonChart>;
	let element: HTMLElement;
	let comparison: WritableSignal<ResourceSnapshot<CumulativeExpenseComparison | undefined>>;

	beforeEach(async () => {
		TestBed.configureTestingModule({ providers: [{ provide: ChartThemeService, useValue: { colors: signal(colors) } }] });
		TestBed.overrideComponent(CumulativeComparisonChart, {
			remove: { imports: [BaseChartDirective] },
			add: { imports: [FakeChart] },
		});
		comparison = signal({
			status: 'resolved',
			value: buildCumulativeExpenseComparison({ daysInCurrentMonth: 5, daysInPreviousMonth: 4 }),
		});

		fixture = TestBed.createComponent(CumulativeComparisonChart);
		element = fixture.nativeElement;
		fixture.componentRef.setInput('comparison', resourceFromSnapshots(comparison));
		fixture.componentRef.setInput('month', new Date(2026, 9, 1));
		await fixture.whenStable();
	});

	function chart() {
		return fixture.debugElement.query(By.directive(FakeChart))?.injector.get(FakeChart);
	}

	function text(node: Element | null) {
		return node?.textContent?.replace(/\s+/g, ' ').trim();
	}

	it('should draw both months over the days of the longer one, stopping where a month has no value yet', () => {
		const data = chart()!.data()!;

		expect(chart()!.type()).toBe('line');
		expect(data.labels).toEqual([1, 2, 3, 4, 5]);
		expect(data.datasets[0].data).toEqual([150, 210, 210, null, null]);
		expect(data.datasets[1].data).toEqual([100, 100, 800, 800, null]);
	});

	it('should name the lines after the months, the month in the primary color and the one before in its variant', () => {
		const [current, previous] = chart()!.data()!.datasets;

		expect(current).toEqual(expect.objectContaining({ label: 'outubro de 2026', borderColor: 'amber', borderWidth: 2 }));
		expect(previous).toEqual(expect.objectContaining({ label: 'setembro de 2026', borderColor: 'brown' }));
		expect(chart()!.options()!.plugins!.legend!.labels).toEqual(
			expect.objectContaining({ color: 'ink', pointStyle: 'line' }),
		);
	});

	it('should compare a January with December of the year before', async () => {
		fixture.componentRef.setInput('month', new Date(2026, 0, 1));
		await fixture.whenStable();

		expect(
			chart()!
				.data()!
				.datasets.map((dataset) => dataset.label),
		).toEqual(['janeiro de 2026', 'dezembro de 2025']);
	});

	it('should show both months of a day in BRL in the tooltip, leaving out a month without that day', () => {
		const tooltip = chart()!.options()!.plugins!.tooltip!;
		const item = { label: '3', dataset: { label: 'outubro de 2026' }, parsed: { x: 2, y: 210 } } as TooltipItem<'line'>;
		const missing = { parsed: { x: 4, y: null } } as unknown as TooltipItem<'line'>;

		expect(tooltip.callbacks!.title!.call({} as TooltipModel<'line'>, [item])).toBe('Dia 3');
		expect(String(tooltip.callbacks!.label!.call({} as TooltipModel<'line'>, item)).replace(/\s/g, ' ')).toBe(
			'outubro de 2026: R$ 210,00',
		);
		expect(tooltip.filter!(item, 0, [], {} as never)).toBe(true);
		expect(tooltip.filter!(missing, 0, [], {} as never)).toBe(false);
	});

	it('should describe where each line ends', () => {
		expect(element.querySelector('canvas')!.getAttribute('aria-label')?.replace(/\s/g, ' ')).toBe(
			'Despesas acumuladas: outubro de 2026, R$ 210,00 até o dia 3; setembro de 2026, R$ 800,00 até o dia 4.',
		);
	});

	it('should say when neither month has expenses', async () => {
		comparison.set({
			status: 'resolved',
			value: buildCumulativeExpenseComparison({ currentMonth: [], previousMonth: [{ day: 1, accumulated: 0 }] }),
		});
		await fixture.whenStable();

		expect(chart()).toBeUndefined();
		expect(text(element.querySelector('mat-card-content p'))).toBe('Sem despesas neste mês nem no anterior.');
	});

	it('should say when the comparison fails to load, and retry it', async () => {
		const retry = vi.fn();
		fixture.componentInstance.retry.subscribe(retry);
		comparison.set({ status: 'error', error: new Error('Server Error') });
		await fixture.whenStable();

		expect(chart()).toBeUndefined();
		expect(text(element.querySelector('mat-card-content p'))).toBe('Não foi possível carregar as despesas acumuladas.');
		Array.from(element.querySelectorAll('button'))
			.find((b) => b.textContent?.includes('Tentar novamente'))!
			.click();
		expect(retry).toHaveBeenCalledOnce();
	});
});
