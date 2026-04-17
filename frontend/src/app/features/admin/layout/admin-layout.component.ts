import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { TokenService } from '../../../core/services/token.service';

interface NavLink {
  label: string;
  route: string;
  icon: string;
  exact: boolean;
}

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './admin-layout.component.html',
})
export class AdminLayoutComponent {
  readonly navLinks: NavLink[] = [
    { label: 'Overview',       route: '/admin/overview',       icon: '📊', exact: true  },
    { label: 'Users',          route: '/admin/users',          icon: '👥', exact: false },
    { label: 'KYC Reviews',    route: '/admin/kyc',            icon: '🪪', exact: false },
    { label: 'Support',        route: '/admin/tickets',        icon: '🎫', exact: false },
    { label: 'Catalog',        route: '/admin/catalog',        icon: '🏷️', exact: false },
    { label: 'Notifications',  route: '/admin/notifications',  icon: '🔔', exact: false },
  ];

  // Surface the signed-in admin email for the header profile area.
  get adminEmail(): string {
    return this.tokenService.getUser()?.email ?? 'admin@system';
  }

  // Build a short avatar label from the current admin email.
  get adminInitials(): string {
    const base = this.adminEmail.split('@')[0] ?? 'AD';
    return base.slice(0, 2).toUpperCase();
  }

  constructor(
    private readonly tokenService: TokenService,
    private readonly router: Router,
  ) {}

  // Clear the stored admin session and return to the login screen.
  logout(): void {
    this.tokenService.clear();
    void this.router.navigate(['/login']);
  }
}
