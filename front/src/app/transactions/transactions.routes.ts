import { Routes } from '@angular/router';

export const transactionsRoutes: Routes = [
	{
		path: '',
		loadComponent: () => import('./pages/transactions-page/transactions-page').then((m) => m.TransactionsPage),
	},
];
