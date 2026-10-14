import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'home',
  },
  {
    path: 'login',
    "title": 'Sign in | Contract Management',
    loadComponent: () =>
      import('./features/auth/login/login')
        .then((m) => m.Login
      ),
  },
  {
    path: 'home',
    canActivate : [authGuard],
    loadChildren: () =>
      import('./features/home/home.routes').then(
        (m) => m.HOME_ROUTES,
      ),
  },
    {
    path: 'parties',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/parties/parties.routes').then((m) => m.PARTIES_ROUTES),
  },
  {
    path: '**',
    redirectTo: 'home',
  },
];
