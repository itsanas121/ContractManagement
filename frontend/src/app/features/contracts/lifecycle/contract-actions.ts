import { Component, computed, inject, input, output, signal, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Observable, finalize } from 'rxjs';
import { ContractLifecycleService } from './contract-lifecycle.service';
import { CurrentUserService } from '../../../core/auth/current-user';
import { apiErrorMessages } from './api-errors';

type ReasonAction = 'reject' | 'terminate';

// Lifecycle buttons for the contract details page (S2-06, S2-07, S2-08 screens).
// Usage: <app-contract-actions [contractId]="..." [status]="..." [ownerId]="..." (changed)="reload()" />
@Component({
  selector: 'app-contract-actions',
  imports: [FormsModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  templateUrl: './contract-actions.html',
  styleUrl: './contract-actions.scss',
})
export class ContractActions {
  private readonly lifecycle = inject(ContractLifecycleService);
  private readonly currentUser = inject(CurrentUserService);
  private readonly destroyRef = inject(DestroyRef);

  // Data from the parent page
  contractId = input.required<number>();
  status = input.required<string>();
  ownerId = input.required<number>();
  readonly disabled = input(false);
  readonly busyChange = output<boolean>();

  // Tells the parent page to reload the contract after a successful action
  changed = output<void>();

  // Screen state
  loading = signal(false);
  readonly busy = computed(
    () => this.loading() || this.disabled(),
  );
  errors = signal<string[]>([]);
  successMessage = signal('');
  reasonFor = signal<ReasonAction | null>(null);
  reason = '';

  // Who can see which button
  isOwner = computed(() => this.currentUser.user?.id === this.ownerId());
  isReviewer = computed(() => this.currentUser.user?.role === 'Reviewer');

  canSubmit = computed(() => this.status() === 'Draft' && this.isOwner());
  canReview = computed(() => this.status() === 'UnderReview' && this.isReviewer());
  canActivate = computed(() => this.status() === 'Approved' && this.isOwner());
  canTerminate = computed(() => this.status() === 'Active' && this.isOwner());

  hasActions = computed(() =>
    this.canSubmit() || this.canReview() || this.canActivate() || this.canTerminate());

  submit(): void {
    this.run(this.lifecycle.submit(this.contractId()), 'Contract submitted for review.');
  }

  approve(): void {
    this.run(this.lifecycle.approve(this.contractId()), 'Contract approved.');
  }

  activate(): void {
    this.run(this.lifecycle.activate(this.contractId()), 'Contract activated.');
  }

  openReason(action: ReasonAction): void {
    if (this.busy()) {
      return;
    }
    this.reasonFor.set(action);
    this.reason = '';
    this.errors.set([]);
    this.successMessage.set('');
  }

  cancelReason(): void {
    this.reasonFor.set(null);
  }

  confirmReason(): void {
    if (this.busy()) {
  return;
}
    const reason = this.reason.trim();
    if (!reason) {
      this.errors.set(['A reason is required.']);
      return;
    }

    const isReject = this.reasonFor() === 'reject';
    const request = isReject
      ? this.lifecycle.reject(this.contractId(), reason)
      : this.lifecycle.terminate(this.contractId(), reason);

    this.run(request, isReject ? 'Contract rejected.' : 'Contract terminated.');
  }

  // Shared by all actions: loading state, success message, errors
private run(request: Observable<void>, successText: string): void {
  if (this.busy()) {
    return;
  }

  this.loading.set(true);
  this.busyChange.emit(true);
  this.errors.set([]);
  this.successMessage.set('');

  request
    .pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => {
        this.loading.set(false);

        if (!this.destroyRef.destroyed) {
          this.busyChange.emit(false);
        }
      }),
    )
    .subscribe({
      next: () => {
        this.reasonFor.set(null);
        this.successMessage.set(successText);
        this.changed.emit();
      },
      error: (error) => {
        this.errors.set(apiErrorMessages(error));
      },
    });
}
}