import { Component, input } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';

@Component({
	selector: 'app-page-header',
	templateUrl: './page-header.html',
	styleUrl: './page-header.scss',
	imports: [MatToolbarModule],
})
export class PageHeader {
	readonly text = input.required<string>();
}
