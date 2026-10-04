import { Component, resourceFromSnapshots, ResourceSnapshot, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReportCard } from './report-card';

@Component({
	imports: [ReportCard],
	template: `
		<app-report-card
			heading="Despesas por categoria"
			[subtitle]="subtitle()"
			[report]="report"
			[empty]="empty()"
			[emptyText]="emptyText()"
			errorText="Não foi possível carregar as despesas por categoria."
			(retry)="retries.set(retries() + 1)"
		>
			<ng-template><p class="content">O gráfico</p></ng-template>
		</app-report-card>
	`,
})
class Host {
	readonly snapshot = signal<ResourceSnapshot<number[] | undefined>>({ status: 'resolved', value: [1] });
	readonly report = resourceFromSnapshots(this.snapshot);
	readonly subtitle = signal<string | undefined>(undefined);
	readonly empty = signal(false);
	readonly emptyText = signal('Sem movimentações neste mês.');
	readonly retries = signal(0);
}

describe('ReportCard', () => {
	let fixture: ComponentFixture<Host>;
	let host: Host;
	let element: HTMLElement;

	beforeEach(async () => {
		fixture = TestBed.createComponent(Host);
		host = fixture.componentInstance;
		element = fixture.nativeElement;
		await fixture.whenStable();
	});

	function text(selector: string) {
		return element.querySelector(selector)?.textContent?.trim();
	}

	function content() {
		return element.querySelector('.content');
	}

	function retryButton() {
		return Array.from(element.querySelectorAll('button')).find((b) => b.textContent?.includes('Tentar novamente'));
	}

	it('should show the title, and the subtitle only when there is one', async () => {
		expect(text('mat-card-title')).toBe('Despesas por categoria');
		expect(element.querySelector('mat-card-subtitle')).toBeNull();

		host.subtitle.set('Total R$ 5.200,00');
		await fixture.whenStable();

		expect(text('mat-card-subtitle')).toBe('Total R$ 5.200,00');
	});

	it('should show the report once it has loaded', () => {
		expect(text('.content')).toBe('O gráfico');
		expect(element.querySelector('mat-progress-spinner')).toBeNull();
	});

	it('should show a spinner instead of the report while it loads', async () => {
		host.snapshot.set({ status: 'loading', value: undefined });
		await fixture.whenStable();

		expect(element.querySelector('mat-progress-spinner')).not.toBeNull();
		expect(content()).toBeNull();
	});

	it('should keep the report, dimmed, while it reloads', async () => {
		host.snapshot.set({ status: 'reloading', value: [1] });
		await fixture.whenStable();

		expect(content()).not.toBeNull();
		expect(element.querySelector('mat-card-content')!.classList).toContain('reloading');
		expect(element.querySelector('mat-progress-spinner')).toBeNull();
	});

	it('should say what failed and retry from the card', async () => {
		host.snapshot.set({ status: 'error', error: new Error('Server Error') });
		await fixture.whenStable();

		expect(text('mat-card-content p')).toBe('Não foi possível carregar as despesas por categoria.');
		expect(content()).toBeNull();

		retryButton()!.click();

		expect(host.retries()).toBe(1);
	});

	it('should say when there is nothing to show, in its own words', async () => {
		host.empty.set(true);
		await fixture.whenStable();

		expect(text('mat-card-content p')).toBe('Sem movimentações neste mês.');
		expect(content()).toBeNull();

		host.emptyText.set('Sem despesas neste mês.');
		await fixture.whenStable();

		expect(text('mat-card-content p')).toBe('Sem despesas neste mês.');
	});
});
