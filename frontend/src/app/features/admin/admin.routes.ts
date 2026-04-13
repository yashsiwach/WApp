import { Routes } from '@angular/router';

export const adminRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layout/admin-layout.component').then((m) => m.AdminLayoutComponent),
    children: [
      { path: '', redirectTo: 'overview', pathMatch: 'full' },
      {
        path: 'overview',
        loadComponent: () =>
          import('./pages/overview/admin-overview.component').then(
            (m) => m.AdminOverviewComponent,
          ),
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./pages/users/admin-users.component').then((m) => m.AdminUsersComponent),
      },
      {
        path: 'kyc',
        loadComponent: () =>
          import('./pages/kyc/admin-kyc.component').then((m) => m.AdminKycComponent),
      },
      {
        path: 'tickets',
        loadComponent: () =>
          import('./pages/tickets/admin-tickets.component').then(
            (m) => m.AdminTicketsComponent,
          ),
      },
      {
        path: 'catalog',
        loadComponent: () =>
          import('./pages/catalog/admin-catalog.component').then(
            (m) => m.AdminCatalogComponent,
          ),
      },
      {
        path: 'notifications',
        loadComponent: () =>
          import('./pages/notifications/admin-notifications.component').then(
            (m) => m.AdminNotificationsComponent,
          ),
      },
    ],
  },
];
