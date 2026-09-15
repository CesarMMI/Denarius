import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

// Placeholder: gráfico de barras agrupadas (entradas x saídas por mês, últimos
// 6 meses). Renderização real fica para quando o Chart.js for instalado.
@Component({
	selector: 'app-cashflow-chart',
	templateUrl: './cashflow-chart.html',
	styleUrl: './cashflow-chart.scss',
	imports: [MatIconModule],
})
export class CashflowChart {}
