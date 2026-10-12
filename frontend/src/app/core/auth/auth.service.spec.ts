import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { AuthService, LOGIN_PATH } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  it('is not authenticated without a token', () => {
    expect(service.isAuthenticated).toBe(false);
  });

  it('stores the token after a successful login', () => {
    service.login({ email: 'a@b.c', password: 'x' }).subscribe();

    const req = http.expectOne(`${environment.apiUrl}${LOGIN_PATH}`);
    expect(req.request.method).toBe('POST');
    req.flush({ token: 'abc' });

    expect(service.token).toBe('abc');
    expect(service.isAuthenticated).toBe(true);
  });

  it('clears the token on logout', () => {
    sessionStorage.setItem('cms.auth.token', 'abc');
    service.logout();
    expect(service.isAuthenticated).toBe(false);
  });
});