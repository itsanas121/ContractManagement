import { Routes } from '@angular/router';
import { roleGuard } from '../../core/auth/role.guard';

export const CONTRACTS_ROUTES: Routes = [
  {
    path: 'create',
    title: 'Create contract | Contract Management',
    canActivate: [roleGuard],
    data: { roles: ['ContractManager'] },
    loadComponent: () =>
      import('./contract-create/contract-create').then(
        (m) => m.ContractCreate,
      ),
  },
];