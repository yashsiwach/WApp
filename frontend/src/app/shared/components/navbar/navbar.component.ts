import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { TokenService } from '../../../core/services/token.service';

interface TopNavItem {
  label: string;
  route: string;
  exact?: boolean;
  adminOnly?: boolean;
}

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  host: {
    class: 'contents',
  },
  template: `
    <header class="border-b border-blue-200/70 bg-[#f8fbff]/92 text-slate-800 shadow-[0_12px_26px_rgba(37,99,235,0.12)] backdrop-blur-xl lg:hidden">
      <div class="px-4 pb-3 pt-3 sm:px-6">
        <div class="flex items-center justify-between gap-3">
          <a
            routerLink="/dashboard/overview"
            class="group flex min-w-0 items-center gap-3 rounded-2xl border border-blue-200/80 bg-white/95 px-3 py-2 shadow-sm"
            aria-label="Go to dashboard overview"
          >
            <span class="flex h-10 w-10 items-center justify-center rounded-xl bg-gradient-to-br from-blue-600 via-sky-500 to-cyan-500 text-sm font-bold text-white shadow-lg shadow-blue-500/30">
              DW
            </span>
            <span class="min-w-0">
              <span class="block truncate font-display text-sm font-bold tracking-tight text-slate-900">DigitalWallet</span>
              <span class="block truncate text-[11px] text-slate-500">Smart money workspace</span>
            </span>
          </a>

          <button
            type="button"
            (click)="logout()"
            class="rounded-xl border border-blue-300/80 bg-white px-3 py-2 text-xs font-semibold text-slate-700 transition-colors hover:border-blue-400 hover:bg-blue-50 hover:text-blue-800"
          >
            Logout
          </button>
        </div>

        <div class="mt-3 flex items-center gap-2 overflow-x-auto pb-1 scrollbar-thin scrollbar-track-transparent scrollbar-thumb-blue-300">
          @for (item of visibleNavItems; track item.route) {
            <a
              [routerLink]="item.route"
              routerLinkActive="border-accent/40 bg-accent/10 text-accent"
              [routerLinkActiveOptions]="{ exact: item.exact ?? false }"
              class="inline-flex shrink-0 items-center rounded-full border border-blue-200 bg-white/90 px-3 py-1.5 text-xs font-medium text-slate-600 transition-colors hover:border-blue-300 hover:bg-blue-50 hover:text-slate-800"
            >
              {{ item.label }}
            </a>
          }
        </div>
      </div>
    </header>

    <aside class="hidden h-full w-72 flex-col border-r border-blue-200/80 bg-[#f4f8ff]/95 text-slate-800 lg:flex">
      <div class="border-b border-blue-200/80 px-5 py-5">
        <a
          routerLink="/dashboard/overview"
          class="group flex items-center gap-3 rounded-2xl border border-blue-200/80 bg-white/95 px-3 py-3 shadow-sm"
          aria-label="Go to dashboard overview"
        >
          <span class="flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-blue-600 via-sky-500 to-cyan-500 text-sm font-bold text-white shadow-lg shadow-blue-500/30">
            DW
          </span>
          <span class="min-w-0">
            <span class="block truncate font-display text-base font-bold tracking-tight text-slate-900">DigitalWallet</span>
            <span class="block truncate text-xs text-slate-500">Designed for effortless control</span>
          </span>
        </a>
      </div>

      <div class="px-5 pt-5">
        <div class="flex items-center gap-3 rounded-2xl border border-blue-200/80 bg-white p-3 shadow-sm">
          <div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-blue-100 to-sky-100 text-sm font-semibold text-blue-800">
            {{ userInitials }}
          </div>
          <div class="min-w-0">
            <p class="truncate text-sm font-semibold text-slate-800">{{ userEmail || 'Signed in user' }}</p>
            <p class="text-[11px] uppercase tracking-[0.14em] text-slate-500">{{ userRole }}</p>
          </div>
        </div>
      </div>

      <nav class="flex-1 overflow-y-auto px-3 py-5">
        <div class="space-y-1">
          @for (item of visibleNavItems; track item.route) {
            <a
              [routerLink]="item.route"
              routerLinkActive="border-accent/40 bg-gradient-to-r from-accent/15 to-accent/5 text-accent shadow-[0_8px_16px_rgba(37,99,235,0.18)]"
              [routerLinkActiveOptions]="{ exact: item.exact ?? false }"
              class="group flex items-center justify-between rounded-xl border border-transparent px-3 py-2.5 text-sm font-medium text-slate-600 transition-all duration-200 hover:border-blue-200 hover:bg-blue-50/70 hover:text-slate-900"
            >
              <span>{{ item.label }}</span>
              <span class="h-1.5 w-1.5 rounded-full bg-current opacity-40 transition-opacity group-hover:opacity-70"></span>
            </a>
          }
        </div>
      </nav>

      <div class="border-t border-blue-200/80 p-4">
        <button
          type="button"
          (click)="logout()"
          class="w-full rounded-xl border border-blue-300 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 transition-colors hover:border-blue-400 hover:bg-blue-50 hover:text-blue-800"
        >
          Logout
        </button>
      </div>
    </aside>
  `,
})
export class NavbarComponent {
  private readonly navItems: TopNavItem[] = [
    { label: 'Overview', route: '/dashboard/overview', exact: true },
    { label: 'Wallet', route: '/dashboard/wallet' },
    { label: 'Rewards', route: '/dashboard/rewards' },
    { label: 'Catalog', route: '/dashboard/catalog' },
    { label: 'Support', route: '/dashboard/support' },
    { label: 'KYC', route: '/dashboard/kyc' },
    { label: 'Notifications', route: '/dashboard/notifications' },
    { label: 'Admin', route: '/admin', adminOnly: true },
  ];

  get userEmail(): string {
    return this.tokenService.getUser()?.email ?? '';
  }

  get userRole(): string {
    return this.tokenService.getUser()?.role ?? 'User';
  }

  get userInitials(): string {
    const email = this.userEmail;
    if (!email) return 'DW';

    const base = email.split('@')[0] || 'dw';
    return base.slice(0, 2).toUpperCase();
  }

  get visibleNavItems(): TopNavItem[] {
    return this.navItems.filter((item) => !item.adminOnly || this.tokenService.isAdmin());
  }

  constructor(
    private readonly tokenService: TokenService,
    private readonly router: Router,
  ) {}

  logout(): void {
    this.tokenService.clear();
    void this.router.navigate(['/login']);
  }
}
