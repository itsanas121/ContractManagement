import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormGroupDirective,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { apiErrorMessages } from '../contracts/lifecycle/api-errors';
import { Party } from './party.models';
import { PartyService } from './party.service';

type PartyKind = 'individual' | 'organization';

const TEN_DIGITS = /^\d{10}$/;
const NOT_BLANK = /\S/;

@Component({
  selector: 'app-parties',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './parties.html',
  styleUrl: './parties.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Parties {
  private readonly partyService = inject(PartyService);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly formDirective = viewChild(FormGroupDirective);

  readonly kind = signal<PartyKind>('individual');
  readonly parties = signal<Party[]>([]);
  readonly loadingList = signal(true);
  readonly submitting = signal(false);
  readonly errors = signal<string[]>([]);
  readonly success = signal<string | null>(null);

  readonly individualForm = this.fb.group({
    name: ['', [Validators.required, Validators.pattern(NOT_BLANK), Validators.maxLength(200)]],
    nationalId: ['', [Validators.required, Validators.pattern(TEN_DIGITS)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    phone: ['', [Validators.required, Validators.pattern(NOT_BLANK), Validators.maxLength(30)]],
  });

  readonly organizationForm = this.fb.group({
    organizationName: ['', [Validators.required, Validators.pattern(NOT_BLANK), Validators.maxLength(200)]],
    registrationNumber: ['', [Validators.required, Validators.pattern(TEN_DIGITS)]],
    contactPerson: ['', [Validators.required, Validators.pattern(NOT_BLANK), Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    phone: ['', [Validators.required, Validators.pattern(NOT_BLANK), Validators.maxLength(30)]],
  });

  constructor() {
    this.loadParties();
  }

  setKind(kind: PartyKind): void {
    this.kind.set(kind);
    this.errors.set([]);
    this.success.set(null);
  }

  submit(): void {
    const form = this.kind() === 'individual' ? this.individualForm : this.organizationForm;
    if (form.invalid || this.submitting()) {
      form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errors.set([]);
    this.success.set(null);

    const request$ =
      this.kind() === 'individual'
        ? this.partyService.registerIndividual(this.trimmed(this.individualForm.getRawValue()))
        : this.partyService.registerOrganization(this.trimmed(this.organizationForm.getRawValue()));

    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (party) => {
        this.formDirective()?.resetForm();
        this.success.set(`Party "${party.name}" registered.`);
        this.submitting.set(false);
        this.loadParties();
      },
      error: (error) => {
        this.errors.set(apiErrorMessages(error));
        this.submitting.set(false);
      },
    });
  }

  fieldError(control: AbstractControl, digitsMessage = 'Invalid value.'): string | null {
    if (!control.touched || control.valid) {
      return null;
    }
    if (control.hasError('required')) return 'This field is required.';
    if (control.hasError('email')) return 'Enter a valid email.';
    if (control.hasError('maxlength')) {
      return `Maximum ${control.getError('maxlength').requiredLength} characters.`;
    }
    if (control.hasError('pattern')) {
      return control.getError('pattern').requiredPattern === TEN_DIGITS.toString()
        ? digitsMessage
        : 'This field cannot be blank.';
    }
    return 'Invalid value.';
  }

  private loadParties(): void {
    this.partyService
      .getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (list) => {
          this.parties.set(list);
          this.loadingList.set(false);
        },
        error: (error) => {
          this.errors.set(apiErrorMessages(error));
          this.loadingList.set(false);
        },
      });
  }

  private trimmed<T extends Record<string, string>>(value: T): T {
    return Object.fromEntries(Object.entries(value).map(([key, text]) => [key, text.trim()])) as T;
  }
}