import { ChangeDetectorRef, Component, inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroupDirective, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { RouterLink } from '@angular/router';
import { UserService } from './user.service';
import { CompanyService } from '../companies/company.service';
import { CreateUserRequest, UserRole } from './user.models';
import { CompanyResponse } from '../companies/company.models';
import { finalize } from 'rxjs';
import { apiErrorMessages } from '../contracts/lifecycle/api-errors';

@Component({
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule],
  selector: 'app-users',
  styleUrl: './users.scss',
  templateUrl: './users.html',
})
export class Users implements OnInit {
  @ViewChild(FormGroupDirective)
  private formDirective?: FormGroupDirective;

  private readonly fb = inject(FormBuilder);
  private readonly userService = inject(UserService);
  private readonly companyService = inject(CompanyService);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly roles: UserRole[] = [
    'Administrator',
    'ContractManager',
    'Reviewer'
  ];

  companies: CompanyResponse[] = [];

  loadingCompanies = false;
  saving = false;

  companyLoadError = '';
  submitError = '';
  successMessage = '';

  userForm = this.fb.nonNullable.group({
    firstName: ['', [
      Validators.required,
      Validators.pattern(/\S/),
      Validators.maxLength(100)
    ]],
    lastName: ['', [
      Validators.required,
      Validators.pattern(/\S/),
      Validators.maxLength(100)
    ]],
    email: ['', [
      Validators.required,
      Validators.email,
      Validators.maxLength(256)
    ]],
    employeeNumber: ['', [
      Validators.required,
      Validators.pattern(/\S/),
      Validators.maxLength(50)
    ]],
    password: ['', Validators.required],
    role: ['', Validators.required],
    companyId: [0, Validators.min(1)],

    // Optional profile information
    jobTitle: [''],
    department: [''],
    phoneNumber: ['']
  });

  get activeCompanies(): CompanyResponse[] {
    return this.companies.filter(
      company => company.status === 'Active'
    );
  }

  ngOnInit(): void {
    this.loadCompanies();
  }

  loadCompanies(): void {
    this.loadingCompanies = true;
    this.companyLoadError = '';

    this.companyService.getAll()
      .pipe(finalize(() => {
        this.loadingCompanies = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: companies => {
          this.companies = companies;
        },
        error: error => {
          this.companyLoadError =
            apiErrorMessages(error).join(' ');
        }
      });
  }

  onSubmit(): void {
    if (this.userForm.invalid || this.saving) {
      this.userForm.markAllAsTouched();
      return;
    }

    const form = this.userForm.getRawValue();

    const jobTitle = form.jobTitle.trim();
    const department = form.department.trim();
    const phoneNumber = form.phoneNumber.trim();

    const request: CreateUserRequest = {
      firstName: form.firstName.trim(),
      lastName: form.lastName.trim(),
      email: form.email.trim(),
      employeeNumber: form.employeeNumber.trim(),
      password: form.password,
      role: form.role as UserRole,
      companyId: form.companyId,
      profile: jobTitle || department || phoneNumber
        ? {
          jobTitle: jobTitle || null,
          department: department || null,
          phoneNumber: phoneNumber || null
        }
        : null
    };

    this.saving = true;
    this.submitError = '';
    this.successMessage = '';

    this.userService.create(request)
      .pipe(finalize(() => {
        this.saving = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: user => {
          this.successMessage =
            `User "${user.firstName} ${user.lastName}" registered successfully.`;

          this.formDirective?.resetForm({
            firstName: '',
            lastName: '',
            email: '',
            employeeNumber: '',
            password: '',
            role: '',
            companyId: 0,
            jobTitle: '',
            department: '',
            phoneNumber: ''
          });
        },
        error: error => {
          this.submitError =
            apiErrorMessages(error).join(' ');
        }
      });
  }
}
