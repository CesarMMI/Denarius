import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { LOCALE_ID, resourceFromSnapshots, ResourceSnapshot, signal, WritableSignal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { buildMonthlySummary } from '../../testing/report-fixtures';
import { MonthlySummary } from '../../types/report';
import { SummaryCards } from './summary-cards';

registerLocaleData(localePt);

describe('SummaryCards', () => {
	let fixture: ComponentFixture<SummaryCards>;
	let element: HTMLElement;
	let summary: WritableSignal<ResourceSnapshot<MonthlySummary | undefined>>;

	beforeEach(async () => {
		TestBed.configureTestingModule({ providers: [{ provide: LOCALE_ID, useValue: 'pt-BR' }] });
		summary = signal({ status: 'resolved', value: buildMonthlySummary() });

		fixture = TestBed.createComponent(SummaryCards);
		element = fixture.nativeElement;
		fixture.componentRef.setInput('summary', resourceFromSnapshots(summary));
		await fixture.whenStable();
	});

	async function show(value: MonthlySummary) {
		summary.set({ status: 'resolved', value });
		await fixture.whenStable();
	}

	/** The text of an element, with the currency pipe's non-breaking spaces as plain ones. */
	function text(node: Element | null | undefined) {
		return node?.textContent?.replace(/\s+/g, ' ').trim();
	}

	function card(title: string) {
		const card = Array.from(element.querySelectorAll('mat-card')).find(
			(c) => text(c.querySelector('mat-card-title')) === title,
		);
		return {
			value: card?.querySelector('.value'),
			caption: card?.querySelector('.caption'),
		};
	}

	function titles() {
		return Array.from(element.querySelectorAll('mat-card-title')).map(text);
	}

	it('should show the five cards of the month in BRL, expenses as negative values without red', () => {
		expect(titles()).toEqual(['Saldo', 'Receitas', 'Despesas', 'Taxa de poupança', 'Projeção de despesas']);
		expect(text(card('Saldo').value)).toBe('R$ 2.800,00');
		expect(text(card('Receitas').value)).toBe('R$ 8.000,00');
		expect(text(card('Despesas').value)).toBe('-R$ 5.200,00');
		expect(card('Despesas').value!.classList).not.toContain('negative');
		expect(text(card('Taxa de poupança').value)).toBe('35%');
		expect(text(card('Projeção de despesas').value)).toBe('-R$ 5.200,00');
		expect(card('Projeção de despesas').value!.classList).not.toContain('negative');
		expect(text(card('Projeção de despesas').caption)).toBe('Saldo projetado R$ 2.800,00');
	});

	it('should compare with the previous month by name, going up being bad news only for expenses', () => {
		const balance = card('Saldo').caption!;
		const income = card('Receitas').caption!;
		const expense = card('Despesas').caption!;

		expect(text(balance)).toBe('↓ 30% vs. agosto');
		expect(balance.classList).toContain('bad');
		expect(text(income)).toBe('0% vs. agosto');
		expect(income.classList).not.toContain('good');
		expect(income.classList).not.toContain('bad');
		expect(text(expense)).toBe('↑ 30% vs. agosto');
		expect(expense.classList).toContain('bad');
	});

	it('should show good news in the good color', async () => {
		await show(
			buildMonthlySummary({
				month: '2026-10',
				previousMonth: { ...buildMonthlySummary().previousMonth, balanceChange: 106.79, totalExpenseChange: -57.5 },
			}),
		);

		expect(text(card('Saldo').caption)).toBe('↑ 106,8% vs. setembro');
		expect(card('Saldo').caption!.classList).toContain('good');
		expect(text(card('Despesas').caption)).toBe('↓ 57,5% vs. setembro');
		expect(card('Despesas').caption!.classList).toContain('good');
	});

	it('should say when the previous month has nothing to compare with', async () => {
		await show(
			buildMonthlySummary({
				month: '2026-01',
				previousMonth: { ...buildMonthlySummary().previousMonth, totalIncomeChange: null },
			}),
		);

		expect(text(card('Receitas').caption)).toBe('Sem comparação com dezembro');
	});

	it('should show no savings rate without income, and mark negative values', async () => {
		await show(buildMonthlySummary({ totalIncome: 0, balance: -1200, savingsRate: null, projectedBalance: -1500 }));

		expect(text(card('Taxa de poupança').value)).toBe('—');
		expect(text(card('Taxa de poupança').caption)).toBe('Sem receitas no mês');
		expect(card('Saldo').value!.classList).toContain('negative');
		expect(card('Projeção de despesas').caption!.querySelector('.negative')).not.toBeNull();
	});

	it('should show a spinner instead of the cards while loading', async () => {
		summary.set({ status: 'loading', value: undefined });
		await fixture.whenStable();

		expect(titles()).toEqual(['Resumo do mês']);
		expect(element.querySelector('mat-progress-spinner')).not.toBeNull();
	});

	it('should say when the summary fails to load, and retry it', async () => {
		const retry = vi.fn();
		fixture.componentInstance.retry.subscribe(retry);
		summary.set({ status: 'error', error: new Error('Server Error') });
		await fixture.whenStable();

		expect(text(element.querySelector('mat-card-content p'))).toBe('Não foi possível carregar o resumo do mês.');

		Array.from(element.querySelectorAll('button'))
			.find((b) => b.textContent?.includes('Tentar novamente'))!
			.click();

		expect(retry).toHaveBeenCalledOnce();
	});

	it('should say when the month has no movement', async () => {
		await show(buildMonthlySummary({ totalIncome: 0, totalExpense: 0, balance: 0, savingsRate: null }));

		expect(titles()).toEqual(['Resumo do mês']);
		expect(text(element.querySelector('mat-card-content p'))).toBe('Sem movimentações neste mês.');
	});
});
