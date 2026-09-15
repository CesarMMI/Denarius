import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { BottomNav } from './shared/bottom-nav/bottom-nav';

@Component({
	selector: 'app-root',
	imports: [RouterOutlet, BottomNav],
	template: `
		<main><router-outlet /></main>
		<app-bottom-nav />
	`,
	styles: `
		:host {
			display: flex;
			flex-direction: column;
			width: min(100vw, 560px);
			height: 100vh;
			margin: 0 auto;
			box-sizing: border-box;
			background-color: var(--mat-sys-surface-container-lowest);
		}
		main {
			flex: 1 1 auto;
			overflow-y: auto;
		}
	`,
})
export class AppComponent {}
