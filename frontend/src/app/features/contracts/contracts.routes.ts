import { Routes } from '@angular/router';

export const CONTRACTS_ROUTES: Routes = [
  {
    path: 'create',
    title: 'Create contract | Contract Management',
    loadComponent: () =>
      import('./contract-create/contract-create').then(
        (m) => m.ContractCreate,
      ),
  },
];