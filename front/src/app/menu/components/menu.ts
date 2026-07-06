import { Component, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { MenuModule } from 'primeng/menu';
import { AuthService } from '../../auth/services/auth.service';

@Component({
	selector: 'app-menu',
	templateUrl: './menu.html',
	styleUrl: './menu.scss',
	imports: [RouterOutlet, BreadcrumbModule, MenuModule],
})
export class Menu {
	private readonly authService = inject(AuthService);
	private readonly router = inject(Router);

	protected readonly currentUser = this.authService.currentUser;

	protected readonly menuItems: MenuItem[] = [
		{ label: 'Contas', routerLink: '/accounts' },
		{ label: 'Categorias', routerLink: '/categories' },
		{
			label: 'Sair',
			command: () => {
				this.authService.logout();
				this.router.navigateByUrl('/auth/login');
			},
		},
	];

	protected readonly breadcrumbItems: MenuItem[] = [
		{ label: 'Products' },
		{ label: 'Electronics' },
		{ label: 'Laptops' },
		{ label: 'Dell' },
	];
}
