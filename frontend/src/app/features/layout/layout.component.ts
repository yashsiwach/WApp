import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent],
  template: `
    <div class="flex h-screen flex-col bg-surface-950 text-slate-900 lg:flex-row lg:overflow-hidden">
      <app-navbar />

      <main class="relative min-w-0 flex-1 overflow-y-auto bg-gradient-to-b from-surface-900 via-surface-950 to-surface-900">
        <div class="pointer-events-none absolute inset-x-0 top-0 h-48 bg-gradient-to-b from-blue-100/60 via-sky-100/35 to-transparent"></div>
        <div class="pointer-events-none absolute -left-16 top-24 h-56 w-56 rounded-full bg-blue-200/25 blur-3xl"></div>
        <div class="pointer-events-none absolute -right-16 bottom-20 h-56 w-56 rounded-full bg-cyan-200/20 blur-3xl"></div>
        <div class="relative min-h-full">
          <router-outlet />
        </div>
      </main>
    </div>
  `,
})
export class LayoutComponent {}
