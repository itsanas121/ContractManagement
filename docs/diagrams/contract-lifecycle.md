# Contract Lifecycle

The SVG export is the reference diagram.
The Mermaid source below is included for future editing.

[View the reference diagram](contract-lifecycle.svg)

For editing instructions, see [Maintenance](README.md#maintenance).

## Mermaid source

```text
---
title: CContract Lifecycle
---
stateDiagram
  direction LR
  classDef draftState fill:#E8F1FB,stroke:#3B6EA5,color:#1B2A3A;
  classDef goodState fill:#E6F4EA,stroke:#2E7D32,color:#1B2A3A;
  classDef badState fill:#FDECEA,stroke:#C62828,color:#1B2A3A;
  classDef endState fill:#F3F3F3,stroke:#757575,color:#1B2A3A;
  [*] --> Draft:Create Contract
  Draft --> UnderReview:Submit
  UnderReview --> Approved:Approve
  UnderReview --> Rejected:Reject
  Approved --> Active:Activate
  Active --> Expired:Expiration (System)
  Active --> Terminated:Terminate
  Expired --> [*]
  Terminated --> [*]
  Rejected --> [*]
  note right of Draft 
  Submit requires: title, valid dates, value > 0,
        1+ Party, 1+ Document, Company Active
  end note
  note right of Approved 
  Activate requires: StartDate reached, Company Active,
        1+ Party, (A) EndDate not in the past
  end note
  note right of Active 
  Warning only (no state change):
        EndDate within ExpirationWarningDays
  end note
  class Draft,UnderReview draftState
  class Approved,Active goodState
  class Rejected,Terminated badState
  class Expired endState
```