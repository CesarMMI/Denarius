import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './auth/guards/auth.guard';

export const routes: Routes = [
	{
		path: 'auth',
		canActivate: [guestGuard],
		loadChildren: () => import('./auth/auth.routes').then((m) => m.authRoutes),
	},
	{
		path: '',
		canActivate: [authGuard],
		loadComponent: () => import('./menu/components/menu').then((m) => m.Menu),
		children: [
			{
				path: '',
				redirectTo: 'accounts',
				pathMatch: 'full',
			},
			{
				path: 'accounts',
				loadChildren: () => import('./accounts/accounts.routes').then((m) => m.accountsRoutes),
			},
			{
				path: 'categories',
				loadChildren: () => import('./categories/categories.routes').then((m) => m.categoriesRoutes),
			},
		],
	},
];
