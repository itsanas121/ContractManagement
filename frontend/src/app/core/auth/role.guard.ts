import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { CurrentUserService } from './current-user';

// Use after authGuard: canActivate: [authGuard, roleGuard], data: { roles: ['Administrator'] }.
// Sends users without an allowed role to /home. Convenience only; the API still enforces roles.
export const roleGuard: CanActivateFn = (route) => {
  const allowedRoles = route.data['roles'] as string[] | undefined;
  if (!allowedRoles) {
    return true;
  }

  const user = inject(CurrentUserService).user;
  return user !== null && allowedRoles.includes(user.role)
    ? true
    : inject(Router).createUrlTree(['/home']);
};