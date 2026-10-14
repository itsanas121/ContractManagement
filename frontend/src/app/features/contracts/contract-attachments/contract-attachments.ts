import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormGroupDirective,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatStepperModule } from '@angular/material/stepper';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ErrorStateMatcher } from '@angular/material/core';

import { CurrentUserService } from '../../../core/auth/current-user';
import { Party } from '../../parties/party.models';
import { PartyService } from '../../parties/party.service';
import {
  AddedContractParty,
  AddContractDocumentRequest,
  AddContractPartyRequest,
  ContractResponse,
  DOCUMENT_TYPES,
  DocumentType,
  PARTY_ROLES,
  PartyRole,
  ContractPartyResponse,
  ContractDocumentResponse,
} from '../contract.models';
import { ContractService } from '../contract.service';
import { apiErrorMessages } from '../lifecycle/api-errors';

@Component({
  selector: 'app-contract-attachments',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatStepperModule,
  ],
  templateUrl: './contract-attachments.html',
  styleUrl: './contract-attachments.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContractAttachments {
  readonly contract = input.required<ContractResponse>();

  readonly savedParties = input<readonly ContractPartyResponse[]>([]);
  readonly savedDocuments = input<readonly ContractDocumentResponse[]>([]);
  readonly disabled = input(false);

  readonly changed = output<void>();
  readonly busyChange = output<boolean>();

  private readonly fb = inject(NonNullableFormBuilder);
  private readonly contracts = inject(ContractService);
  private readonly partyService = inject(PartyService);
  private readonly currentUser = inject(CurrentUserService);
  private readonly destroyRef = inject(DestroyRef);

  readonly partyRoles = PARTY_ROLES;
  readonly documentTypes = DOCUMENT_TYPES;
  readonly fieldErrorMatcher: ErrorStateMatcher = {
  isErrorState: (control) =>
    !!control && control.invalid && control.touched,
};

  readonly parties = signal<Party[]>([]);
  readonly loadingParties = signal(false);
  readonly loadErrors = signal<string[]>([]);

  readonly savingParty = signal(false);
  readonly savingDocument = signal(false);
  readonly partyErrors = signal<string[]>([]);
  readonly documentErrors = signal<string[]>([]);

  readonly addedParties = signal<AddedContractParty[]>([]);
  readonly addedDocuments = signal<AddContractDocumentRequest[]>([]);

  readonly busy = computed(
    () => this.savingParty() || this.savingDocument(),
  );

readonly hasParty = computed(
  () =>
    this.savedParties().length > 0 ||
    this.addedParties().length > 0,
);

readonly hasDocument = computed(
  () =>
    this.savedDocuments().length > 0 ||
    this.addedDocuments().length > 0,
);

  readonly availableParties = computed(() => {
    const linkedIds = new Set([
        ...this.savedParties().map((party) => party.partyId),
        ...this.addedParties().map((party) => party.partyId),
  ]);
  
  return this.parties().filter((party) => !linkedIds.has(party.id));
});

  readonly partyForm = this.fb.group({
    partyId: this.fb.control<number | null>(
      null,
      [Validators.required, Validators.min(1)],
    ),
    partyRole: this.fb.control<PartyRole | null>(
      null,
      Validators.required,
    ),
  });

  readonly documentForm = this.fb.group({
    fileName: [
      '',
      [
        Validators.required,
        Validators.pattern(/\S/),
        Validators.maxLength(260),
      ],
    ],
    documentType: this.fb.control<DocumentType | null>(
      null,
      Validators.required,
    ),
    filePath: ['', Validators.maxLength(500)],
  });

  constructor() {
    this.loadParties();
  }

  get accessError(): string | null {
    const user = this.currentUser.user;
    const contract = this.contract();

    if (!user || !Number.isInteger(user.id) || user.id <= 0) {
      return 'Please sign in again.';
    }

    if (user.role !== 'ContractManager') {
      return 'Only Contract Managers can add parties and documents.';
    }

    if (contract.ownerId !== user.id) {
      return 'Only the contract owner can add parties and documents here.';
    }

    if (contract.status !== 'Draft') {
      return 'Parties and documents can only be added to a Draft contract.';
    }

    return null;
  }

  loadParties(): void {
    if (this.loadingParties()) {
      return;
    }

    this.loadErrors.set([]);
    this.loadingParties.set(true);

    this.partyService
      .getAll()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loadingParties.set(false)),
      )
      .subscribe({
        next: (parties) => this.parties.set(parties),
        error: (error) => {
          this.loadErrors.set(apiErrorMessages(error));
        },
      });
  }

  addParty(formDirective: FormGroupDirective): void {
    if (this.busy()) {
      return;
    }

    this.partyErrors.set([]);

    const accessError = this.accessError;
    if (accessError) {
      this.partyErrors.set([accessError]);
      return;
    }

    if (this.partyForm.invalid) {
      this.partyForm.markAllAsTouched();
      return;
    }

    const values = this.partyForm.getRawValue();

    if (values.partyId === null || values.partyRole === null) {
      return;
    }

    const selectedParty = this.availableParties().find(
      (party) => party.id === values.partyId,
    );

    if (!selectedParty) {
      this.partyErrors.set(['Please select an available party.']);
      return;
    }

    const request: AddContractPartyRequest = {
      partyId: values.partyId,
      partyRole: values.partyRole,
    };

    this.savingParty.set(true);
    this.busyChange.emit(true);

    this.contracts
      .addParty(this.contract().id, request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.savingParty.set(false);
           if (!this.destroyRef.destroyed) {
          this.busyChange.emit(false);
           }
        }),
      )
      .subscribe({
        next: () => {
          this.addedParties.update((parties) => [
            ...parties,
            { ...request, name: selectedParty.name },
          ]);
          formDirective.resetForm();
           this.changed.emit();
        },
        error: (error) => {
          this.partyErrors.set(apiErrorMessages(error));
        },
      });
  }

  addDocument(formDirective: FormGroupDirective): void {
    if (this.busy()) {
      return;
    }

    this.documentErrors.set([]);

    const accessError = this.accessError;
    if (accessError) {
      this.documentErrors.set([accessError]);
      return;
    }

    if (!this.hasParty()) {
      this.documentErrors.set(['Add a party before this step.']);
      return;
    }

    if (this.documentForm.invalid) {
      this.documentForm.markAllAsTouched();
      return;
    }

    const values = this.documentForm.getRawValue();

    if (values.documentType === null) {
      return;
    }

    const request: AddContractDocumentRequest = {
      fileName: values.fileName.trim(),
      documentType: values.documentType,
      filePath: values.filePath.trim() || null,
    };

    this.savingDocument.set(true);
    this.busyChange.emit(true);

    this.contracts
      .addDocument(this.contract().id, request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.savingDocument.set(false);
          if (!this.destroyRef.destroyed) {
          this.busyChange.emit(false);
           }
        }),
      )
      .subscribe({
        next: () => {
          this.addedDocuments.update((documents) => [
            ...documents,
            request,
          ]);
          formDirective.resetForm();
          this.changed.emit();
        },
        error: (error) => {
          this.documentErrors.set(apiErrorMessages(error));
        },
      });
  }

  fieldError(control: AbstractControl): string | null {
    if (!control.touched || control.valid) {
      return null;
    }

    if (control.hasError('required')) {
      return 'This field is required.';
    }

    if (control.hasError('pattern')) {
      return 'This field cannot be blank.';
    }

    if (control.hasError('maxlength')) {
      const maxLength = control.getError('maxlength').requiredLength;
      return `Maximum ${maxLength} characters.`;
    }

    return 'Invalid value.';
  }
}