import { Routes } from '@angular/router';

export const publicRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/public/public.component').then((c) => c.PublicComponent),
  },
];
