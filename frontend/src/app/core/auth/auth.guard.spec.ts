import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  Router,
  RouterStateSnapshot,
  UrlTree,
  provideRouter,
} from '@angular/router';

import { authGuard } from './auth.guard';
import { TOKEN_KEY } from './auth.service';

describe('authGuard', () => {
  const run = (url: string) =>
    TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot),
    );

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
  });

  it('allows navigation when a token exists', () => {
    sessionStorage.setItem(TOKEN_KEY, 'abc');
    expect(run('/contracts')).toBe(true);
  });

  it('redirects to /login with the returnUrl when not authenticated', () => {
    const result = run('/contracts');
    expect(result).toBeInstanceOf(UrlTree);

    const tree = result as UrlTree;
    expect(tree.queryParams['returnUrl']).toBe('/contracts');
    expect(TestBed.inject(Router).serializeUrl(tree).startsWith('/login')).toBe(true);
  });
});