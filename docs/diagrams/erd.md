# Contract Management System - ERD

The SVG export is the reference diagram.
The Mermaid source below is included for future editing.

[View the reference diagram](erd.svg)

For editing instructions, see [Maintenance](README.md#maintenance).

## Mermaid source

```text
---
title: Contract Management System - ERD
---
erDiagram
    Company ||--o{ User : employs
    Company ||--o{ Contract : owns
    User ||--o| UserProfile : has
    User ||--o{ Contract : "is Owner of"
    User |o--o{ Contract : approves
    User |o--o{ Contract : rejects
    User |o--o{ Contract : terminates
    Contract ||--o{ ContractParty : has
    Party ||--o{ ContractParty : "participates via"
    Contract ||--o{ ContractDocument : has
    User ||--o{ ContractDocument : uploads
    User |o--o{ AuditLog : performs
    Contract ||..o{ AuditLog : "is audited in"

    Company {
        int Id PK
        string Name
        string RegistrationNumber UK
        string Email
        string Phone
        string Address
        string Status "Active | Inactive"
        datetime CreatedDate
    }

    User {
        int Id PK
        int CompanyId FK
        string FirstName
        string LastName
        string Email UK
        string EmployeeNumber "UK with CompanyId"
        string Status "Active | Inactive"
        string Role "Administrator | ContractManager | Reviewer"
        string PasswordHash "assumption: login"
    }

    UserProfile {
        int UserId PK, FK
        string Phone "assumption: minimal"
        string JobTitle "assumption: minimal"
    }

    Party {
        int Id PK
        string PartyType "Individual | Organization (TPH of IndividualParty, OrganizationParty)"
        string Name
        string Email
        string Phone
        string NationalId UK "IndividualParty only"
        string RegistrationNumber UK "OrganizationParty only"
        string ContactPerson "OrganizationParty only"
    }

    Contract {
        int Id PK
        string ContractNumber UK
        string Title
        string Description "optional"
        int CompanyId FK
        int OwnerId FK
        decimal ContractValue "decimal(18,2)"
        date StartDate
        date EndDate
        string ContractType "Sales | Procurement | Service | Partnership | Other"
        string Status "Draft | UnderReview | Approved | Rejected | Active | Expired | Terminated"
        datetime CreatedDate
        int ApprovedById FK
        datetime ApprovedDate
        int RejectedById FK
        datetime RejectedDate
        string RejectionReason
        int TerminatedById FK
        datetime TerminationDate 
        string TerminationReason
        binary RowVersion "assumption: concurrency"
    }

    ContractParty {
        int ContractId PK, FK
        int PartyId PK, FK
        string PartyRole "Customer | Supplier | Partner | Consultant"
        datetime AddedDate
    }

    ContractDocument {
        int Id PK
        int ContractId FK
        string FileName
        string DocumentType
        string FilePath
        int UploadedById FK
        datetime UploadedDate
    }

    AuditLog {
        int Id PK
        string EntityType
        int EntityId "no FK, logical reference"
        string Action
        string OldValue
        string NewValue
        int PerformedById FK "null means System"
        datetime PerformedAt
    }
```