import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from '../services/token.service';

export const authGuard: CanActivateFn = () => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  // Keep anonymous users on the login screen until a token is available.
  if (tokenService.isLoggedIn()) {
    return true;
  }

  return router.parseUrl('/login');
};
