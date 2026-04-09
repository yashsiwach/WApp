import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { TokenService } from '../../../core/services/token.service';

interface NavItem {
  label: string;
  route: string;
  icon: string;
  adminOnly?: boolean;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    <aside class="bg-surface-900 text-zinc-100 w-60 min-h-full flex flex-col border-r border-zinc-800">
      <nav class="flex flex-col gap-1 p-4 flex-1">
        @for (item of visibleItems; track item.route) {
          <a
            [routerLink]="item.route"
            routerLinkActive="bg-accent/15 text-accent border border-amber-600/40"
            class="flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium text-zinc-400 hover:bg-zinc-800 hover:text-zinc-100 transition-colors border border-transparent"
          >
            <span class="text-lg" [innerHTML]="item.icon"></span>
            {{ item.label }}
          </a>
        }
      </nav>
    </aside>
  `,
})
export class SidebarComponent {
  private readonly navItems: NavItem[] = [
    { label: 'Dashboard', route: '/dashboard/overview', icon: '&#9632;&#65039;' },
    { label: 'Wallet', route: '/dashboard/wallet', icon: '&#128179;' },
    { label: 'Rewards', route: '/dashboard/rewards', icon: '&#127873;' },
    { label: 'Notifications', route: '/dashboard/notifications', icon: '&#128276;' },
    { label: 'Support', route: '/dashboard/support', icon: '&#127915;' },
    { label: 'KYC', route: '/dashboard/kyc', icon: '&#128203;' },
    { label: 'Catalog', route: '/dashboard/catalog', icon: '&#127873;' },
    { label: 'Admin', route: '/admin', icon: '&#9881;&#65039;', adminOnly: true },
  ];

  constructor(private readonly tokenService: TokenService) {}

  get visibleItems(): NavItem[] {
    return this.navItems.filter((item) => !item.adminOnly || this.tokenService.isAdmin());
  }
}
