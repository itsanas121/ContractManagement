import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthService, LOGIN_PATH, LOGIN_ROUTE } from './auth.service';

// True only for requests to our own API, so the JWT is never sent to third parties.
function isApiRequest(url: string): boolean {
  if (environment.apiUrl) {
    return url.startsWith(`${environment.apiUrl}/`);
  }
  return url.startsWith('/') && !url.startsWith('//');
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (!isApiRequest(req.url)) {
    return next(req);
  }

  const auth = inject(AuthService);
  const router = inject(Router);
  const isLogin = req.url.endsWith(LOGIN_PATH);
  const token = auth.token;

// Requests are immutable, so clone to add the Authorization header.
  const request =
    token && !isLogin
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

  return next(request).pipe(
    catchError((error: unknown) => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        !isLogin &&
        !router.url.startsWith(LOGIN_ROUTE)
      ) {
        auth.logout();
        void router.navigate([LOGIN_ROUTE], {
          queryParams: { returnUrl: router.url },
        });
      }
      // 403 means "authenticated but not allowed": keep the session, let callers handle it.
      return throwError(() => error);
    }),
  );
};