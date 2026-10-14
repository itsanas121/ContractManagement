import { HttpErrorResponse } from '@angular/common/http';
import {
    ChangeDetectionStrategy,
    Component,
    DestroyRef,
    inject,
    signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
    Subject,
    catchError,
    combineLatest,
    finalize,
    of,
    startWith,
    switchMap,
} from 'rxjs';

import { ContractDetailsResponse } from '../contract.models';
import { ContractService } from '../contract.service';
import { apiErrorMessages } from '../lifecycle/api-errors';
import { CurrentUserService } from '../../../core/auth/current-user';
import { ContractAttachments } from '../contract-attachments/contract-attachments';
import { ContractActions } from '../lifecycle/contract-actions';


@Component({
    selector: 'app-contract-details',
    imports: [RouterLink, MatButtonModule, ContractAttachments, ContractActions],
    templateUrl: './contract-details.html',
    styleUrl: './contract-details.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContractDetails {
    private readonly route = inject(ActivatedRoute);
    private readonly contracts = inject(ContractService);
    private readonly destroyRef = inject(DestroyRef);
    private readonly currentUser = inject(CurrentUserService);
    private readonly reloadRequests = new Subject<void>();

    readonly contract = signal<ContractDetailsResponse | null>(null);
    readonly loading = signal(false);
    readonly errors = signal<string[]>([]);
    readonly canRetry = signal(false);
    readonly attachmentsBusy = signal(false);
    readonly lifecycleBusy = signal(false);

    get canEditAttachments(): boolean {
        const contract = this.contract();
        const user = this.currentUser.user;

        return (
            contract !== null &&
            contract.status === 'Draft' &&
            user?.role === 'ContractManager' &&
            user.id === contract.ownerId
        );
    }

    constructor() {
        combineLatest([
            this.route.paramMap,
            this.reloadRequests.pipe(startWith(undefined)),
        ])
            .pipe(
                switchMap(([params]) => {
                    const rawId = params.get('id');
                    const contractId = Number(rawId);

                    this.contract.set(null);
                    this.errors.set([]);
                    this.canRetry.set(false);

                    if (
                        !rawId ||
                        !/^\d+$/.test(rawId) ||
                        !Number.isSafeInteger(contractId) ||
                        contractId <= 0 ||
                        contractId > 2147483647
                    ) {
                        this.loading.set(false);
                        this.errors.set(['Invalid contract ID.']);
                        return of(null);
                    }

                    this.loading.set(true);

                    return this.contracts.getById(contractId).pipe(
                        catchError((error) => {
                            this.errors.set(apiErrorMessages(error));

                            this.canRetry.set(
                                !(error instanceof HttpErrorResponse) ||
                                ![403, 404].includes(error.status),
                            );

                            return of(null);
                        }),
                        finalize(() => this.loading.set(false)),
                    );
                }),
                takeUntilDestroyed(this.destroyRef),
            )
            .subscribe((contract) => {
                this.contract.set(contract);
            });
    }

    reload(): void {
        if (
            this.loading() ||
            this.attachmentsBusy() ||
            this.lifecycleBusy()
        ) {
            return;
        }

        this.reloadRequests.next();
    }

    attachmentsChanged(): void {
        this.attachmentsBusy.set(false);
        this.reload();
    }

    lifecycleChanged(): void {
        this.lifecycleBusy.set(false);
        this.reload();
    }
}