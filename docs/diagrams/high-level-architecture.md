# High-Level Architecture

The SVG export is the reference diagram.
The Mermaid source below is included for future editing.

[View the reference diagram](high-level-architecture.svg)

For editing instructions, see [Maintenance](README.md#maintenance).

## Mermaid source

```
---
title: High-Level Architecture
---
flowchart TB
    subgraph FE["Frontend - Angular app (separate folder: frontend/)"]
        direction TB
        FE1["Angular (responsive): Login, Contract List, Contract Detail, Summary, Admin"]
        FE2["Calls protected API endpoints. Shows only the allowedActions returned by the API"]
    end

    subgraph API["Api project - API Layer (composition root)"]
        direction TB
        API1["Authentication - JWT (email + password login)"]
        API2["Authorization - role policies"]
        API3["Request Validation - DTO shape"]
        API4["Exception Handling - maps exceptions to HTTP"]
        API5["Configuration - appsettings bound to ContractSettings (Options pattern)"]
        API6["BackgroundService - ExpirationBackgroundService, calls Application Layer in a new scope"]
    end

    subgraph APP["Application Layer (folder)"]
        direction TB
        APP1["Use cases - Submit, Approve, Reject, Activate, Terminate, Search, Summary"]
        APP2["Business Rules across entities"]
        APP3["Transactions - one SaveChanges per use case"]
        APP4["Audit - creates AuditLog entry"]
        APP5["Logging"]
        APP6["Interfaces: IAppDbContext, IClock, ICurrentUser"]
    end

    subgraph DOM["Domain Layer (folder)"]
        direction TB
        DOM1["Contract - lifecycle methods and invariants"]
        DOM2["Party, IndividualParty, OrganizationParty - Validate"]
        DOM3["Custom Exceptions"]
    end

    subgraph CORE["Core project"]
        direction TB
        APP
        DOM
    end

    subgraph INF["Infrastructure project - Infrastructure Layer"]
        direction TB
        INF1["EF Core - DbContext, configurations, migrations"]
        INF2["Implements IAppDbContext, IClock, password hashing, token creation"]
    end

    subgraph BE["Backend - 3 projects"]
        direction TB
        API
        CORE
        INF
    end

    FE -- HTTPS + JSON + JWT --> API
    API --> APP
    APP --> DOM
    INF -. implements interfaces of .-> APP
    INF --> DB[("Database - SQL")]

    APP:::layer
    DOM:::layer
    API:::layer
    INF:::layer
    FE:::layer

    classDef layer fill:#F5F8FC,stroke:#3B6EA5,color:#1B2A3A
```