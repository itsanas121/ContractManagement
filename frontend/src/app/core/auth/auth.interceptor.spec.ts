import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { vi } from 'vitest';

import { environment } from '../../../environments/environment';
import { authInterceptor } from './auth.interceptor';
import { LOGIN_PATH, TOKEN_KEY } from './auth.service';

describe('authInterceptor', () => {
  let client: HttpClient;
  let http: HttpTestingController;
  let navigate: ReturnType<typeof vi.spyOn>;
  const apiUrl = `${environment.apiUrl}/api/test`;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    client = TestBed.inject(HttpClient);
    http = TestBed.inject(HttpTestingController);
    navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
  });

  it('adds the bearer token to API requests', () => {
    sessionStorage.setItem(TOKEN_KEY, 'abc');
    client.get(apiUrl).subscribe();
    const req = http.expectOne(apiUrl);
    expect(req.request.headers.get('Authorization')).toBe('Bearer abc');
    req.flush({});
  });

  it('does not add the token to external URLs', () => {
    sessionStorage.setItem(TOKEN_KEY, 'abc');
    client.get('https://example.com/data').subscribe();
    const req = http.expectOne('https://example.com/data');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('clears the session and redirects on 401', () => {
    sessionStorage.setItem(TOKEN_KEY, 'abc');
    client.get(apiUrl).subscribe({ error: () => undefined });
    http.expectOne(apiUrl).flush('', { status: 401, statusText: 'Unauthorized' });

    expect(sessionStorage.getItem(TOKEN_KEY)).toBeNull();
    expect(navigate).toHaveBeenCalledWith(['/login'], { queryParams: { returnUrl: '/' } });
  });

  it('keeps the session on 403', () => {
    sessionStorage.setItem(TOKEN_KEY, 'abc');
    client.get(apiUrl).subscribe({ error: () => undefined });
    http.expectOne(apiUrl).flush('', { status: 403, statusText: 'Forbidden' });

    expect(sessionStorage.getItem(TOKEN_KEY)).toBe('abc');
    expect(navigate).not.toHaveBeenCalled();
  });

  it('does not redirect when the login request itself returns 401', () => {
    const loginUrl = `${environment.apiUrl}${LOGIN_PATH}`;
    client.post(loginUrl, {}).subscribe({ error: () => undefined });
    http.expectOne(loginUrl).flush('', { status: 401, statusText: 'Unauthorized' });

    expect(navigate).not.toHaveBeenCalled();
  });
});