import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, throwError } from 'rxjs';

import { TokenService } from '../services/token.service';

const EXCLUDED_PATHS = ['/api/auth/login', '/api/auth/register'];
let isHandlingUnauthorized = false;

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  const isExcluded = EXCLUDED_PATHS.some((p) => req.url.includes(p));
  const token = tokenService.getAccessToken();

  const authReq =
    !isExcluded && token
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Clear the local session and redirect once when a protected request comes back unauthorized.
      if (error.status === 401 && !isExcluded) {
        tokenService.clear();

        if (!isHandlingUnauthorized && router.url !== '/login') {
          isHandlingUnauthorized = true;
          void router.navigate(['/login']).finally(() => {
            isHandlingUnauthorized = false;
          });
        }
      }
      return throwError(() => error);
    }),
  );
};
