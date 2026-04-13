import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

/**
 * Legacy shell — redirects immediately to /admin/overview.
 * The /admin route in app.routes.ts now uses loadChildren → admin.routes.ts,
 * so this component is no longer referenced by the router.
 * Kept to avoid stale import errors during transition.
 */
@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [],
  template: ``,
})
export class AdminComponent implements OnInit {
  constructor(private readonly router: Router) {}

  ngOnInit(): void {
    void this.router.navigate(['/admin/overview'], { replaceUrl: true });
  }
}
