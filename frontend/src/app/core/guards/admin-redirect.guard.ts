import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { TokenService } from '../services/token.service';

export const adminRedirectGuard: CanActivateFn = () => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  // Skip the user dashboard entirely for admins and send them into the admin area.
  if (tokenService.isAdmin()) {
    return router.parseUrl('/admin');
  }

  return true;
};
