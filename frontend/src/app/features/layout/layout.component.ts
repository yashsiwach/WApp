import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent],
  template: `
    <div class="flex flex-col h-screen bg-surface-950 text-zinc-100">
      <app-navbar />
      <main class="flex-1 overflow-y-auto bg-gradient-to-b from-surface-900 to-surface-950">
        <router-outlet />
      </main>
    </div>
  `,
})
export class LayoutComponent {}
