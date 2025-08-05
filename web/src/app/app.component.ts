import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidenavComponent } from './core/components/sidenav/sidenav.component';
import { ToolbarComponent } from './core/components/toolbar/toolbar.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, SidenavComponent, ToolbarComponent],
  template: `
    <app-toolbar />
    <app-sidenav>
      <router-outlet />
    </app-sidenav>
  `,
  styles: `
    :host {
      height: 100%;
      display: grid;
      grid-template-rows: auto 1fr
    }
  `,
})
export class AppComponent {}
