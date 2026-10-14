# Contract Management System

A system for managing contracts between a company and external parties (customers, suppliers, partners), covering the full lifecycle:

**Draft → Under Review → Approved → Active → Expired / Terminated** (or **Rejected**)

Built as a team assignment in the AZEM Technical Internship Program at Saudi AZM.

> **Status:** in active development. See the [roadmap](#roadmap) below.

---

## Features

- **Companies and users** with roles (Administrator, Contract Manager, Reviewer)
- **Parties**: individuals and organizations, each with its own validation rules
- **Contracts** with parties (and their roles) and attached documents
- **Lifecycle operations** as explicit actions, never a generic status update:
  submit, approve, reject, activate, terminate, and automatic expiration
- **Business rules enforced in the domain**: e.g. a contract always starts as Draft, can only be submitted with at least one party and one document, and only a reviewer can approve or reject it
- **Audit trail** recording every important action (who, what, when, old and new values)
- **Consistent error responses** (`ProblemDetails`) with clear validation messages

## Tech stack

| Area | Technology |
|---|---|
| Backend | ASP.NET Core Web API (.NET 10) |
| Data | Entity Framework Core 10, SQL Server |
| API docs | Swagger (Swashbuckle) |
| Frontend | Angular 22 with Angular Material |
| Auth | JWT with role-based authorization |

## Project structure

```
ContractManagement/
├── frontend/                               # Angular application (see frontend/README.md)
├── src/
│   ├── ContractManagement.Core/            # Domain + Application
│   │   ├── Domain/                         # Entities, enums, domain exceptions (business rules)
│   │   └── Application/                    # Use cases and shared interfaces (Common/)
│   ├── ContractManagement.Infrastructure/  # EF Core DbContext, configurations, migrations, services
│   └── ContractManagement.Api/             # Controllers, exception handling, Program.cs
├── global.json                             # Pins the .NET 10 SDK
└── ContractManagement.slnx
```

Dependencies point inward: **Api → Infrastructure → Core**. The Core project defines interfaces (such as `IAppDbContext` and `IAuditService`) and Infrastructure implements them, so business logic does not depend on the database.

## Design diagrams

The reference design is in `docs/diagrams/`.

| Diagram | File |
|---|---|
| Entity-relationship diagram (ERD) | [erd.svg](docs/diagrams/erd.svg) |
| Business process flow | [business-process-flow.svg](docs/diagrams/business-process-flow.svg) |
| Contract lifecycle (states) | [contract-lifecycle.svg](docs/diagrams/contract-lifecycle.svg) |
| High-level architecture (Angular app + 3 backend projects) | [high-level-architecture.svg](docs/diagrams/high-level-architecture.svg) |

See the [design diagrams](docs/diagrams/README.md) .
Each diagram includes Mermaid source and an SVG export.


## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or a full instance)
- EF Core CLI tool:
  ```bash
  dotnet tool install --global dotnet-ef --version 10.*
  ```

### 1. Clone

```bash
git clone https://github.com/itsanas121/ContractManagement.git
cd ContractManagement
```

### 2. Configure the database connection

By default the API uses **SQL Server LocalDB** (see `appsettings.Development.json`). If you use LocalDB, start it:

```bash
sqllocaldb start MSSQLLocalDB
```

If you use a different SQL Server instance, override the connection string locally with **user secrets** (never commit it):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=ContractManagementDb;Trusted_Connection=True;TrustServerCertificate=True" --project src/ContractManagement.Api
```

### 3. Create the database

```bash
dotnet ef database update --project src/ContractManagement.Infrastructure --startup-project src/ContractManagement.Api
```

### 4. Configure JWT secret

Set the JWT signing key using user secrets. The JWT signing key must be at least 32 characters (256 bits):

```bash
dotnet user-secrets set "Jwt:Key" "your-very-long-random-secret-key-32chars-min" --project src/ContractManagement.Api
```

### 4.1. Development Database Seeding

The API automatically seeds development data on startup using the
`SeedData` configuration in `appsettings.Development.json`.

This allows developers to test authentication and role-based APIs
without manually inserting records into SQL Server.

#### Default Company

| Name | Registration Number | Status |
|---|---|---|
| CMS Development | DEV-CMS-001 | Active |

#### Default Users

| Email | Role | Employee Number |
|---|---|---|
| admin@cms.local | Administrator | DEV-ADMIN |
| manager@cms.local | ContractManager | DEV-MANAGER |
| reviewer@cms.local | Reviewer | DEV-REVIEWER |

#### Configure the Development Password

All seeded users share **one development password**, configured
locally through User Secrets.

Run from the repository root:

```bash
dotnet user-secrets set "SeedData:Password" "YOUR_STRONG_DEV_PASSWORD" --project src/ContractManagement.Api
```

Each developer can choose their own password for their local database.

The same configured password is used when creating all three accounts:

- `admin@cms.local`
- `manager@cms.local`
- `reviewer@cms.local`


**Recommendation:** Choose your development password before the
first startup and keep it consistent.

#### Configuration-Driven Seed Data

Companies and users are defined in:

`src/ContractManagement.Api/appsettings.Development.json`

Developers can add more companies and users by updating the
`SeedData:Companies` and `SeedData:Users` arrays.

Each user specifies the company they belong to using
`CompanyRegistrationNumber`.


#### Testing Authentication

Once the API starts, log in using any of the seeded accounts:

**Endpoint:** `POST /api/auth/login`

Example request:

```json
{
  "email": "admin@cms.local",
  "password": "YOUR_STRONG_DEV_PASSWORD"
}
```

A successful login returns a JWT.

Use the returned token in protected API requests:

`Authorization: Bearer YOUR_JWT_TOKEN`

**Role-based testing:**

- **Administrator:** Can access company management endpoints.
- **ContractManager:** Can access permitted contract management operations.
- **Reviewer:** Can access permitted contract review operations.

### 5. Run the API

```bash
dotnet run --project src/ContractManagement.Api
```

Then open Swagger at `http://localhost:<port>/swagger` (the port is shown in the terminal).

> To use the Angular app, run the **https** profile, since the frontend calls `https://localhost:7287`:
> `dotnet run --project src/ContractManagement.Api --launch-profile https`

> **Secrets** (database passwords, JWT signing keys) go in `dotnet user-secrets`, never in `appsettings` files.

## Frontend

The shared Angular application is in `frontend/`.

See [frontend setup instructions](frontend/README.md) for installation,
API configuration, folder conventions, and feature development.

## Contract lifecycle

| Action | Allowed from | Result |
|---|---|---|
| Submit | Draft | Under Review |
| Approve | Under Review | Approved |
| Reject (reason required) | Under Review | Rejected |
| Activate | Approved | Active |
| Terminate (reason required) | Active | Terminated |
| Expire (background job) | Active, after the end date | Expired |

Any other transition is rejected by the domain (e.g. Draft → Active, Approved → Draft).

## API endpoints

All endpoints except login require `Authorization: Bearer <token>`.

| Method | Endpoint | Role | Purpose |
|---|---|---|---|
| POST | `/api/auth/login` | Anyone | Log in and receive a JWT |
| GET | `/api/companies` | Administrator | List companies |
| POST | `/api/companies` | Administrator | Register a company |
| POST | `/api/companies/{id}/activate` | Administrator | Activate a company |
| POST | `/api/companies/{id}/deactivate` | Administrator | Deactivate a company |
| GET | `/api/users` | Administrator | List users |
| POST | `/api/users` | Administrator | Register a user (role, company, optional profile) |
| POST | `/api/users/{id}/activate` | Administrator | Activate a user |
| POST | `/api/users/{id}/deactivate` | Administrator | Deactivate a user (their token stops working immediately) |
| GET | `/api/parties` | Administrator, ContractManager | List parties |
| POST | `/api/parties/individual` | Administrator, ContractManager | Register an individual party |
| POST | `/api/parties/organization` | Administrator, ContractManager | Register an organization party |
| POST | `/api/contracts` | ContractManager | Create a contract (always Draft) |
| POST | `/api/contracts/{id}/parties` | ContractManager (owner) | Add a party with a role (Draft only) |
| POST | `/api/contracts/{id}/documents` | ContractManager (owner) | Add document metadata (Draft only) |
| POST | `/api/contracts/{id}/submit` | ContractManager (owner) | Draft → Under Review |
| POST | `/api/contracts/{id}/approve` | Reviewer | Under Review → Approved |
| POST | `/api/contracts/{id}/reject` | Reviewer | Under Review → Rejected (reason required) |
| POST | `/api/contracts/{id}/activate` | ContractManager (owner) | Approved → Active |
| POST | `/api/contracts/{id}/terminate` | ContractManager (owner) | Active → Terminated (reason required) |

Full request and response details are in Swagger (`/swagger`).

### Error responses

Errors use `ProblemDetails` (`title`, `status`, `detail`; validation errors also include `errors`).

| Status | Meaning |
|---|---|
| 400 | Missing/invalid field, or a business rule was broken |
| 401 | No token, or the token is invalid/expired |
| 403 | Logged in, but not allowed (wrong role, or not the contract owner) |
| 404 | The contract, party, company or user does not exist |
| 409 | Wrong contract status for this action, or the contract was changed by someone else |

## Assumptions and team decisions

Where the specification leaves a choice open, we decided:

- **Roles:** three roles: Administrator (companies, users, parties), ContractManager (creates and runs contracts), Reviewer (approves or rejects).
- **Ownership:** the user who creates a contract is its owner. Only the owner can edit it, submit it, activate it and terminate it.
- **Review:** a reviewer must be active, in the same company as the contract, and cannot review a contract they own.
- **Activation** is a manual action by the owner (the spec's "start date reached" is a condition checked at activation, not an automatic step).
- **Documents:** only metadata is stored; physical file upload is optional and not implemented.
- **Dates:** times are stored in UTC.
- **Frontend checks are convenience only:** hidden buttons and route guards improve the experience, but the API enforces every rule.

## Key design decisions

- **Rich domain model:** entities protect their own state (`private set`) and expose business methods; invalid objects cannot be created.
- **Party inheritance:** `IndividualParty` and `OrganizationParty` share one table (Table-Per-Hierarchy).
- **Enums stored as text** so tables and audit entries are readable.
- **Optimistic concurrency** on contracts (`RowVersion`) to prevent conflicting updates; a conflict returns 409.
- **Audit entries are saved in the same transaction** as the change they describe.

## Development workflow

We use **GitFlow**:

- `develop` is the default branch for day-to-day work; `main` holds stable releases.
- Each task gets its own branch from `develop`, e.g. `feature/S2-06-submit-contract`.
- Changes reach `develop` through pull requests reviewed by a teammate.
- Pull `develop` before creating a migration, and announce new migrations to the team.

## Roadmap

- [x] Domain model and business rules
- [x] Database and initial migration
- [x] Error handling and Swagger
- [x] Audit service
- [x] JWT login and role-based authorization
- [x] Companies, users, parties, create contract, parties/documents, lifecycle actions (API)
- [x] Screens: login, companies, parties, create contract, lifecycle actions, role-based route guards
- [ ] Users screen (in review)
- [ ] Contract details page and contract list
- [ ] Contract search, expiration job, audit history, dashboard
- [ ] Release v1.0

## Team

- Anas Almehmadi
- Deem Alqasir
- Faisal Bawazir