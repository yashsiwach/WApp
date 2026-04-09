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
  template: `
    <nav class="sticky top-0 z-40 border-b border-zinc-800/80 bg-surface-950/85 text-zinc-100 shadow-[0_12px_30px_rgba(0,0,0,0.2)] backdrop-blur-xl">
      <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div class="flex flex-col gap-3 py-3 lg:gap-4">
          <div class="flex min-w-0 flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <a
            routerLink="/dashboard/overview"
            class="group flex min-w-0 max-w-full items-center gap-3 self-start rounded-2xl border border-zinc-800/80 bg-zinc-900/70 px-3 py-2 transition-all duration-200 hover:border-amber-500/40 hover:bg-zinc-900 sm:max-w-[19rem] md:max-w-[23rem] lg:max-w-[26rem]"
            aria-label="Go to dashboard overview"
          >
            <span class="flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-amber-400 via-orange-400 to-rose-500 text-surface-950 shadow-lg shadow-amber-500/20 transition-transform duration-200 group-hover:scale-105">
              <svg class="h-6 w-6" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path d="M21 7H3a1 1 0 0 0-1 1v8a1 1 0 0 0 1 1h18a1 1 0 0 0 1-1V8a1 1 0 0 0-1-1zM3 5h18a3 3 0 0 1 3 3v8a3 3 0 0 1-3 3H3a3 3 0 0 1-3-3V8a3 3 0 0 1 3-3zm10 5.5a1.5 1.5 0 1 1 3 0 1.5 1.5 0 0 1-3 0z"/>
              </svg>
            </span>
            <span class="min-w-0">
              <span class="block truncate font-display text-base font-bold tracking-tight text-white sm:text-lg">DigitalWallet</span>
              <span class="hidden truncate text-xs text-zinc-400 sm:block">Payments, rewards, and support in one place</span>
            </span>
          </a>

          <div class="flex flex-wrap items-center justify-end gap-2 sm:gap-3">
            <div class="hidden items-center gap-3 rounded-2xl border border-zinc-800/80 bg-zinc-900/70 px-3 py-2 sm:flex">
              <div class="flex h-10 w-10 items-center justify-center rounded-xl bg-gradient-to-br from-zinc-800 to-zinc-700 text-sm font-semibold text-white">
                {{ userInitials }}
              </div>
              <div class="min-w-0">
                <p class="truncate text-sm font-medium text-zinc-100">{{ userEmail || 'Signed in user' }}</p>
                <p class="text-xs uppercase tracking-[0.18em] text-zinc-500">{{ userRole }}</p>
              </div>
            </div>

            <button
              type="button"
              (click)="logout()"
              class="rounded-xl border border-zinc-700 bg-zinc-900/80 px-4 py-2 text-sm font-semibold text-zinc-100 transition-all duration-200 hover:-translate-y-0.5 hover:border-rose-400/40 hover:bg-rose-500/10 hover:text-rose-100 focus:outline-none focus:ring-2 focus:ring-rose-400/40"
            >
              Logout
            </button>
          </div>
        </div>

        <div class="flex items-center justify-center gap-2 overflow-x-auto pb-1 scrollbar-thin scrollbar-track-transparent scrollbar-thumb-zinc-800">
          @for (item of visibleNavItems; track item.route) {
            <a
              [routerLink]="item.route"
              routerLinkActive="border-amber-500/40 bg-amber-500/15 text-amber-200 shadow-[0_10px_24px_rgba(245,158,11,0.12)]"
              [routerLinkActiveOptions]="{ exact: item.exact ?? false }"
              class="group inline-flex shrink-0 items-center rounded-full border border-zinc-800 bg-zinc-900/70 px-4 py-2 text-sm font-medium text-zinc-400 transition-all duration-200 hover:-translate-y-0.5 hover:border-zinc-700 hover:bg-zinc-900 hover:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-amber-400/30"
            >
              <span class="relative">
                {{ item.label }}
                <span class="absolute -bottom-1 left-0 h-px w-full origin-left scale-x-0 bg-current transition-transform duration-200 group-hover:scale-x-100"></span>
              </span>
            </a>
          }
        </div>
        </div>
      </div>
    </nav>
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
    this.router.navigate(['/login']);
  }
}
