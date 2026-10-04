import { Routes } from '@angular/router';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

export const reportsRoutes: Routes = [
	{
		path: '',
		// Here rather than in app.config, so Chart.js loads with this page instead of in the initial bundle.
		providers: [provideCharts(withDefaultRegisterables(), { defaults: { font: { family: 'Roboto' } } })],
		loadComponent: () => import('./pages/reports-page/reports-page').then((m) => m.ReportsPage),
	},
];
