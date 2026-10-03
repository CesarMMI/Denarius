import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { PageHeader } from './page-header';

@Component({
	imports: [PageHeader],
	template: `<app-page-header text="Transações"><button>Nova transação</button></app-page-header>`,
})
class Host {}

describe('PageHeader', () => {
	it('should show the title and the actions given', async () => {
		const fixture = TestBed.createComponent(Host);
		await fixture.whenStable();
		const element: HTMLElement = fixture.nativeElement;

		expect(element.querySelector('h1')?.textContent).toBe('Transações');
		expect(element.querySelector('.actions button')?.textContent).toBe('Nova transação');
	});
});
