import { DatePipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { RouterLink } from '@angular/router';
import { CompanyService } from './company.service';
import { CompanyResponse, CreateCompanyRequest } from './company.models';
import { finalize } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  imports: [
    ReactiveFormsModule,
    RouterLink,
    DatePipe,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule],
  selector: 'app-companies',
  styleUrl: './companies.scss',
  templateUrl: './companies.html',
})
export class Companies implements OnInit{
  private readonly fb = inject(FormBuilder);
  private readonly companyService = inject(CompanyService);

  companies: CompanyResponse[] = []

  loadingCompanies = false;
  saving = false;

  loadError = '';
  submitError = '';
  successMessage = '';

  
  companyForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.pattern(/\S/)]],
    registrationNumber: ['', [Validators.required, Validators.pattern(/\S/)]],
    email: ['', Validators.email],
    phone: [''],
    address: ['']
  });

  // on initial load companies
  ngOnInit(): void {
    this.loadCompanies()
  }

  loadCompanies(): void {
    this.loadingCompanies = true;
    this.loadError = '';
    this.companyService.getAll()
      .pipe(finalize(() => this.loadingCompanies = false))
      .subscribe({
        next: (companies) => {
          this.companies = companies
        },
        error: (error: HttpErrorResponse) => {
          this.loadError = error.status === 403
            ? 'You do not have permission to view companies.'
            : 'Could not load companies. Please try again.';
        }

      });
  }

  onSubmit(): void {
    if (this.companyForm.invalid || this.saving) {
      this.companyForm.markAllAsTouched();
      return;
    }

    const form = this.companyForm.getRawValue();

    const request: CreateCompanyRequest = {
      name: form.name.trim(),
      registrationNumber: form.registrationNumber.trim(),
      email: form.email.trim() || null,
      phone: form.phone.trim() || null,
      address: form.address.trim() || null
    };

    this.saving = true;
    this.submitError = ''
    this.successMessage = '';

    this.companyService.create(request)
      .pipe(finalize(() => this.saving = false))
      .subscribe({
        next: (createdCompany) => {
          this.successMessage =
            `Company "${createdCompany.name}" created successfully.`;

          this.companyForm.reset();
          this.loadCompanies();
        },
        error: (error: HttpErrorResponse) => {
          const detail = error.error?.detail;

          this.submitError =
            typeof detail === 'string' && detail.length > 0
              ? detail
              : error.status === 403
                ? 'Only Administrators can create companies.'
                : 'Could not create company. Please try again.';
        }

      });

  }
}
