import {
    ChangeDetectionStrategy,
    Component,
    DestroyRef,
    inject,
    signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
    AbstractControl,
    NonNullableFormBuilder,
    ReactiveFormsModule,
    ValidationErrors,
    ValidatorFn,
    Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import {
    CONTRACT_TYPES,
    ContractResponse,
    ContractType,
    CreateContractRequest,
} from '../contract.models';
import { ContractService } from '../contract.service';
import { apiErrorMessages } from '../lifecycle/api-errors';
import { CurrentUserService } from '../../../core/auth/current-user';
import { ContractAttachments } from '../contract-attachments/contract-attachments';

const positiveValue: ValidatorFn = (
    control: AbstractControl,
): ValidationErrors | null => {
    const value = control.value;

    if (value === null || value === undefined || value === '') {
        return null;
    }

    return Number.isFinite(Number(value)) && Number(value) > 0
        ? null
        : { positiveValue: true };
};

const validDateRange: ValidatorFn = (
    control: AbstractControl,
): ValidationErrors | null => {
    const startDate = control.get('startDate')?.value;
    const endDate = control.get('endDate')?.value;

    if (!startDate || !endDate) {
        return null;
    }

    return endDate > startDate ? null : { dateRange: true };
};

@Component({
    selector: 'app-contract-create',
    imports: [
        ReactiveFormsModule,
        RouterLink,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        ContractAttachments,
    ],
    templateUrl: './contract-create.html',
    styleUrl: './contract-create.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContractCreate {
    private readonly fb = inject(NonNullableFormBuilder);
    private readonly contracts = inject(ContractService);
    private readonly currentUser = inject(CurrentUserService);
    private readonly destroyRef = inject(DestroyRef);

    readonly contractTypes = CONTRACT_TYPES;
    readonly submitting = signal(false);
    readonly errors = signal<string[]>([]);
    readonly createdContract = signal<ContractResponse | null>(null);
    readonly attachmentsBusy = signal(false);


    readonly form = this.fb.group(
        {
            contractNumber: [
                '',
                [
                    Validators.required,
                    Validators.pattern(/\S/),
                    Validators.maxLength(50),
                ],
            ],
            title: [
                '',
                [
                    Validators.required,
                    Validators.pattern(/\S/),
                    Validators.maxLength(200),
                ],
            ],
            description: ['', Validators.maxLength(2000)],
            contractValue: this.fb.control<number | null>(
                null,
                [Validators.required, positiveValue],
            ),
            startDate: ['', Validators.required],
            endDate: ['', Validators.required],
            contractType: this.fb.control<ContractType | null>(
                null,
                Validators.required,
            ),
        },
        { validators: validDateRange },
    );

    get sessionError(): string | null {
        const user = this.currentUser.user;

        if (!user || !Number.isInteger(user.id) || user.id <= 0) {
            return 'Please sign in again.';
        }

        if (user.role !== 'ContractManager') {
            return 'Only Contract Managers can create contracts.';
        }

        if (!Number.isInteger(user.companyId) || user.companyId <= 0) {
            return 'Your session is missing company information. Please sign in again.';
        }

        return null;
    }

    submit(): void {
        if (this.submitting() || this.createdContract()) {
            return;
        }

        this.errors.set([]);

        const sessionError = this.sessionError;
        if (sessionError) {
            this.errors.set([sessionError]);
            return;
        }

        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        const user = this.currentUser.user;
        const values = this.form.getRawValue();

        if (
            !user ||
            values.contractValue === null ||
            values.contractType === null
        ) {
            return;
        }

        const request: CreateContractRequest = {
            contractNumber: values.contractNumber.trim(),
            title: values.title.trim(),
            description: values.description.trim() || null,
            companyId: user.companyId,
            ownerId: user.id,
            contractValue: values.contractValue,
            startDate: values.startDate,
            endDate: values.endDate,
            contractType: values.contractType,
        };

        this.submitting.set(true);

        this.contracts
            .create(request)
            .pipe(
                takeUntilDestroyed(this.destroyRef),
                finalize(() => this.submitting.set(false)),
            )
            .subscribe({
                next: (contract) => {
                    this.createdContract.set(contract);
                },
                error: (error) => {
                    this.errors.set(apiErrorMessages(error));
                },
            });
    }
    
    createAnother(): void {
        if (this.attachmentsBusy()) {
            return;
        }

        this.form.reset();
        this.errors.set([]);
        this.createdContract.set(null);
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

        if (control.hasError('positiveValue')) {
            return 'Contract value must be greater than zero.';
        }

        return 'Invalid value.';
    }
}