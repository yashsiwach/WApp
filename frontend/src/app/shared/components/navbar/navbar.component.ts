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
  templateUrl: './navbar.component.html',
 
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
