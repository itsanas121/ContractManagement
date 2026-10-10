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
| Auth | JWT with role-based authorization *(in progress)* |

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

### 4. Run the API

```bash
dotnet run --project src/ContractManagement.Api
```

Then open Swagger at `http://localhost:<port>/swagger` (the port is shown in the terminal).

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

## Key design decisions

- **Rich domain model:** entities protect their own state (`private set`) and expose business methods; invalid objects cannot be created.
- **Party inheritance:** `IndividualParty` and `OrganizationParty` share one table (Table-Per-Hierarchy).
- **Enums stored as text** so tables and audit entries are readable.
- **Optimistic concurrency** on contracts (`RowVersion`) to prevent conflicting updates.
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
- [ ] JWT login and role-based authorization
- [ ] Angular frontend
- [ ] Companies, users, parties, and contract features (API + UI)
- [ ] Contract search, expiration job, audit history, dashboard

## Team

- Anas Almehmadi
- Deem Alqasir
- Faisal Bawazir
