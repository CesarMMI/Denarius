import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

// Placeholder: gráfico de rosca (donut) com o gasto por categoria no mês,
// mais a legenda das categorias. Renderização real fica para quando o
// Chart.js for instalado.
@Component({
	selector: 'app-category-breakdown-card',
	templateUrl: './category-breakdown-card.html',
	styleUrl: './category-breakdown-card.scss',
	imports: [MatIconModule],
})
export class CategoryBreakdownCard {}
