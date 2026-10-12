import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse } from './auth.models';

export const LOGIN_ROUTE = '/login';
export const LOGIN_PATH = '/api/auth/login';
export const TOKEN_KEY = 'cms.auth.token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  get token(): string | null {
    return sessionStorage.getItem(TOKEN_KEY);
  }

   // Only checks that a token exists. The API decides if it is still valid (401).
  get isAuthenticated(): boolean {
    return this.token !== null;
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}${LOGIN_PATH}`, request)
      .pipe(tap((response) => sessionStorage.setItem(TOKEN_KEY, response.token)));
  }

  logout(): void {
    sessionStorage.removeItem(TOKEN_KEY);
  }
}