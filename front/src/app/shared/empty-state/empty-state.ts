import { Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
	selector: 'app-empty-state',
	templateUrl: './empty-state.html',
	styleUrl: './empty-state.scss',
	imports: [MatButtonModule, MatIconModule],
})
export class EmptyState {
	readonly icon = input('inbox');
	readonly title = input.required<string>();
	readonly body = input.required<string>();
	readonly primaryLabel = input<string | null>(null);
	readonly secondaryLabel = input<string | null>(null);

	readonly primaryAction = output<void>();
	readonly secondaryAction = output<void>();
}
