import { Injectable, inject } from '@angular/core';
import { AuthService } from '../../../core/auth/auth.service';

export interface CurrentUser {
  id: number;
  role: string;
  companyId: number;
}

// Reads the logged-in user's Id and role from the JWT payload.
// Used only to decide which buttons to show; the API still enforces the rules.
@Injectable({ providedIn: 'root' })
export class CurrentUserService {
  private readonly auth = inject(AuthService);

  get user(): CurrentUser | null {
    const token = this.auth.token;
    if (!token) {
      return null;
    }

    try {
      const payload = token.split('.')[1];
      const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      const claims = JSON.parse(json);
      return { 
         id: Number(claims.sub),
         role: claims.role,
         companyId: Number(claims.companyId) };
    } catch {
      return null;
    }
  }
}