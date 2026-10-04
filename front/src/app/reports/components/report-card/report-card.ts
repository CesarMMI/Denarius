import { NgTemplateOutlet } from '@angular/common';
import { Component, contentChild, input, output, Resource, TemplateRef } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

/**
 * A block of the reports page: a titled card that shows its report, or why it can't. Reloading keeps the report on
 * screen, dimmed, until the new one arrives.
 */
@Component({
	selector: 'app-report-card',
	templateUrl: './report-card.html',
	styleUrl: './report-card.scss',
	imports: [NgTemplateOutlet, MatButtonModule, MatCardModule, MatIconModule, MatProgressSpinnerModule],
})
export class ReportCard {
	readonly heading = input.required<string>();
	readonly subtitle = input<string>();
	readonly report = input.required<Resource<unknown>>();
	/** The report loaded with nothing to show. */
	readonly empty = input(false);
	readonly emptyText = input('Sem movimentações neste mês.');
	readonly errorText = input.required<string>();
	readonly retry = output<void>();

	/** Rendered only with a report to show, so a chart is never created over missing data. */
	protected readonly content = contentChild(TemplateRef);
}
