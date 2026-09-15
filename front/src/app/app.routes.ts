import { Routes } from '@angular/router';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'dashboard' },
	{
		path: 'dashboard',
		loadComponent: () => import('./dashboard/components/dashboard-page/dashboard-page').then((m) => m.DashboardPage),
	},
	{
		path: 'transactions',
		loadComponent: () => import('./transactions/components/transactions-page/transactions-page').then((m) => m.TransactionsPage),
	},
	{
		path: 'categories',
		loadComponent: () => import('./categories/components/categories-page/categories-page').then((m) => m.CategoriesPage),
	},
];
