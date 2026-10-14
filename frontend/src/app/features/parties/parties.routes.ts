import { Routes } from '@angular/router';

export const PARTIES_ROUTES: Routes = [
  {
    path: '',
    title: 'Parties | Contract Management',
    loadComponent: () => import('./parties').then((m) => m.Parties),
  },
];