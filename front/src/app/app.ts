import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
	selector: 'app-root',
	imports: [
		RouterOutlet,
		RouterLink,
		RouterLinkActive,
		MatIconModule,
		MatListModule,
		MatSidenavModule,
		MatToolbarModule,
	],
	templateUrl: './app.html',
	styleUrl: './app.scss',
})
export class App {
	protected readonly links = [
		{ route: '/dashboard', label: 'Resumo', icon: 'pie_chart' },
		{ route: '/transactions', label: 'Transações', icon: 'receipt_long' },
		{ route: '/categories', label: 'Categorias', icon: 'sell' },
	];
}
