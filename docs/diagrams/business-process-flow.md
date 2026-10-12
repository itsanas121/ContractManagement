# Contract Business Process

The SVG export is the reference diagram.
The Mermaid source below is included for future editing.

[View the reference diagram
](business-process-flow.svg)

For editing instructions, see [Maintenance](README.md#maintenance).
## Mermaid source
```text
---
title: Contract Business Process
---
flowchart TB
    subgraph ADM["Administrator"]
        direction TB
        A1["Create Company"]
        A2["Create Users"]
        A3["Register Parties (ContractManager can also, A)"]
        A1 --> A2 --> A3
    end

    subgraph MGR["ContractManager (A)"]
        direction TB
        M1["Create Contract (active User, active Company)"]
        M2["Add Parties and Documents"]
        M3["Submit"]
        R2["Activate"]
        R3["Terminate (reason required)"]
        M1 --> M2
        M2 --> M3
    end

    subgraph REV["Reviewer (A)"]
        direction TB
        R1{"Approve or Reject?"}
    end

    subgraph SYS["System"]
        direction TB
        V1{"Submit validation passes?"}
        G1{"Activation conditions met?"}
        S1["Expiration - EndDate passed"]
        W1["Expiration Warning - near EndDate (no state change)"]
        AU["Every action writes an AuditLog entry"]
    end

    S_Draft(["Draft"])
    S_UnderReview(["UnderReview"])
    S_Approved(["Approved"])
    S_Rejected(["Rejected"])
    S_Active(["Active"])
    S_Expired(["Expired"])
    S_Terminated(["Terminated"])

    A3 --> M1
    M1 --> S_Draft
    S_Draft --> M2
    M3 --> V1
    V1 -->|"No - 400 with errors"| S_Draft
    V1 -->|"Yes"| S_UnderReview
    S_UnderReview --> R1
    R1 -->|"Approve"| S_Approved
    R1 -->|"Reject with reason"| S_Rejected
    S_Approved --> R2
    R2 --> G1
    G1 -->|"No - error 409, no change"| S_Approved
    G1 -->|"Yes"| S_Active
    S_Active --> S1
    S_Active --> W1
    S_Active --> R3
    S1 --> S_Expired
    R3 --> S_Terminated

    classDef actor fill:#FFF8E1,stroke:#B28704,color:#1B2A3A
    classDef system fill:#EEF2F7,stroke:#5C6B7A,color:#1B2A3A
    classDef state fill:#E8F1FB,stroke:#3B6EA5,color:#1B2A3A
    classDef good fill:#E6F4EA,stroke:#2E7D32,color:#1B2A3A
    classDef bad fill:#FDECEA,stroke:#C62828,color:#1B2A3A
    classDef endState fill:#F3F3F3,stroke:#757575,color:#1B2A3A

    class A1,A2,A3,M1,M2,M3,R1,R2,R3 actor
    class V1,G1,S1,W1,AU system
    class S_Draft,S_UnderReview state
    class S_Approved,S_Active good
    class S_Rejected,S_Terminated bad
    class S_Expired endState
```