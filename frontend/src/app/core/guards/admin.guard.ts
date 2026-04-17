import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from '../services/token.service';

export const adminGuard: CanActivateFn = () => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  // Restrict admin routes to admin users and send everyone else back to the main dashboard.
  if (tokenService.isAdmin()) {
    return true;
  }

  return router.parseUrl('/dashboard');
};
