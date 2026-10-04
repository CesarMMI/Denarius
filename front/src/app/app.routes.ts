import { Routes } from '@angular/router';

export const routes: Routes = [
	{
		path: '',
		pathMatch: 'full',
		redirectTo: 'transactions',
	},
	{
		path: 'transactions',
		loadChildren: () => import('./transactions/transactions.routes').then((m) => m.transactionsRoutes),
	},
	{
		path: 'categories',
		loadChildren: () => import('./categories/categories.routes').then((m) => m.categoriesRoutes),
	},
	{
		path: 'reports',
		loadChildren: () => import('./reports/reports.routes').then((m) => m.reportsRoutes),
	},
];
