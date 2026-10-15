import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService, LOGIN_ROUTE } from './auth.service';

// Usable on any route: canActivate: [authGuard]. Navigation-level convenience only;
// the API still enforces authorization.
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.isAuthenticated
    ? true
    : router.createUrlTree([LOGIN_ROUTE], { queryParams: { returnUrl: state.url } });
};