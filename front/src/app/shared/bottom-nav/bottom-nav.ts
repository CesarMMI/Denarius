import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
	selector: 'app-bottom-nav',
	templateUrl: './bottom-nav.html',
	styleUrl: './bottom-nav.scss',
	imports: [RouterLink, RouterLinkActive, MatIconModule],
})
export class BottomNav {
	protected readonly links = [
		{ route: '/dashboard', label: 'Resumo', icon: 'pie_chart' },
		{ route: '/transactions', label: 'Transações', icon: 'receipt_long' },
		{ route: '/categories', label: 'Categorias', icon: 'sell' },
	];
}
