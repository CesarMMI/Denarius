import { CurrencyPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Filter as FilterIcon } from '@primeicons/angular/filter';
import { Pencil as PencilIcon } from '@primeicons/angular/pencil';
import { Plus as PlusIcon } from '@primeicons/angular/plus';
import { Refresh as RefreshIcon } from '@primeicons/angular/refresh';
import { Trash as TrashIcon } from '@primeicons/angular/trash';
import { ButtonDirective, ButtonIcon } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { Account } from '../../models/account.model';
import { AccountsService } from '../../services/accounts.service';

@Component({
	selector: 'app-accounts.page',
	templateUrl: './accounts.page.html',
	styleUrl: './accounts.page.scss',
	imports: [
		CurrencyPipe,
		TableModule,
		ButtonDirective,
		ButtonIcon,
		DividerModule,
		TooltipModule,
		PencilIcon,
		FilterIcon,
		TrashIcon,
		RefreshIcon,
		PlusIcon,
	],
})
export class AccountsPage {
	private readonly accountsService = inject(AccountsService);

	protected readonly accountsResource = rxResource({
		stream: () => this.accountsService.list(),
	});

	protected editAccount(account: Account) {
		// Ponto de integração futura: abrir formulário de edição da conta.
	}

	protected deleteAccount(account: Account) {
		if (!confirm(`Excluir a conta "${account.name}"?`)) {
			return;
		}

		this.accountsService.delete(account.id).subscribe(() => this.accountsResource.reload());
	}
}
