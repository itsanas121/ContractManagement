import { Component, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';

import { ContractService } from '../contracts/contract.service';
import { ContractSummary } from '../contracts/contract.models';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [DecimalPipe],
  styleUrl: './home.scss',
  templateUrl: './home.html',
})
export class Home {
  private readonly contracts = inject(ContractService);

  readonly summary = signal<ContractSummary | null>(null);
  readonly error = signal(false);

  constructor() {
    this.contracts.getSummary().subscribe({
      next: (s) => this.summary.set(s),
      error: () => this.error.set(true),
    });
  }
}