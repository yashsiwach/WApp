import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { ToastComponent } from './shared/components/toast/toast.component';
import { TokenService } from './core/services/token.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastComponent],
  template: `
    <router-outlet />
    <app-toast />
  `,
})
export class App implements OnInit {
  private readonly tokenService = inject(TokenService);

  ngOnInit(): void {
    active =true
    // Clear any stale auth state shape if token is absent.
    if (!this.tokenService.getAccessToken()) {
      this.tokenService.clear();
    }
  }
}
