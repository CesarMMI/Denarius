import { Directive, input } from '@angular/core';
import { ChartData, ChartOptions, ChartType } from 'chart.js';

/**
 * Stands in for ng2-charts' `BaseChartDirective` in specs, where jsdom has no canvas to draw on. It takes the same
 * inputs, so a spec reads what the component would give Chart.js.
 */
@Directive({
	// eslint-disable-next-line @angular-eslint/directive-selector -- the selector of the directive it replaces
	selector: 'canvas[baseChart]',
})
export class FakeChart {
	readonly type = input<ChartType>();
	readonly data = input<ChartData>();
	readonly options = input<ChartOptions>();
}
