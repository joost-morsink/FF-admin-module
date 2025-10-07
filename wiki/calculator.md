---
title: Calculator
author: J.W. Morsink
difficulty: hard
archimate:
    layer: Application
    type: Service
    serves:
    - to: admin_ui
    - to: auto_import
    - to: website
    - to: payout#determine
---
# Calculator

The calculator module is responsible for all calculations for Give for Good.
It is modelled as state transitions due to events and may be dependent on other models so simplify calculations.

```arch(plantuml)
> component "Admin Module" as Admin {
    (admin_ui, auto_import) d (calculator, event_store);
    calculator d (event_store, model_cache);
> } 

calculator l website;
```

## Calculator core

The model is a cross product of all submodels.

$$
    M_t = \prod_i m_{i,t}
$$

Each submodel $m_{i,t}$ represents a distinct aspect of the system at time $t$.
The overall model $M_t$ is the product of these submodels, capturing the complete state.

Submodels can be updated in response to events:

$$
    m_t \times E \rightarrow m_{t+1}
$$

Some submodels depend on the state of other submodels, allowing for more complex updates:

$$
    M_t \times E \rightarrow m_{t+1}
$$

For models with dependencies, both the previous state and newly calculated values may be used:

$$
    M_t \times m_{i,t+1} \times E \rightarrow m_{j,t+1}
$$

Circular dependencies between submodels at $t+1$ are not allowed.

```plantuml
!include <archimate/Archimate>

Application_DataObject(ModelT, "Model @ t")
rectangle Other as "Other Models" {
    Application_DataObject(OModelT, "Other models @ t")
    Application_DataObject(OModelT1, "Other models @ t+1")
}
Business_Event(Event, "Event")
Application_Service(Calc, "Model calculator")
Application_DataObject(ModelT1, "Model @ t+1")

OModelT -r-> Calc
OModelT1 -r-> Calc
ModelT -> Calc
Event --> Calc
Calc -> ModelT1

OModelT -[hidden]- OModelT1

note bottom of OModelT1 : No recursiveness on \nt+1 model dependencies

url for Event is [[event]]
```

Because the calculation of the model is **entirely** dependent on the previous model and the event at the specific position in the sequence, all calculations are pure, and cacheable.

### Models

The `Index` model tracks the position of the current event in the sequence.
Each index corresponds to an event that resulted in the present state.
At the initial position ($t=0$), the system assumes the occurrence of the [NONE](./events/NONE.md) event.

Models are categorized as either processed or calculated:
- **Processed models** use a processor to apply event data and, if needed, data from other models. They may also implement [partitioning](./model_partition) to optimize storage and computation.
- **Calculated models** do not process event data directly. Instead, they use a calculator to derive their value from other models, which may themselves be processed or calculated.

Currently, calculated models are not cached; instead, they are parameterized.
Parameterization allows calculated models to efficiently reference partitioned models and minimize redundant computations.
This approach improves performance and flexibility when working with large or complex model structures.

```plantuml
skinparam component {
    style rectangle
    backgroundColor<<Calculator>> #ffffcc
    backgroundColor<<Partitioned>> #ffcccc
}
    component Donations2 << Partitioned >> 
    component Donors2 << Partitioned >>
    component OptionWorths2 << Partitioned >>
    component DonorDashboardStats2 << Calculator >>
    component DonationExistence << Calculator >>
    component DonationRecords2 << Calculator >>

    [Options] -[hidden]r-> [Donations2]

    [Options] ---> [AggregatedDonationsAndTransfers]
    [Options] --> [AmountsToTransfer]
    [Options] ---> [DonationStatistics]
    [Options] --> [MinimalExits]
    [Options] --> [IdealOptionValuations]
    [Options] --> [ValidationErrors]
    [Options] --> [MonthlyDonations]
    [Donations2] --> [AggregatedDonationsAndTransfers]
    [Donations2] --> [DonationStatistics]
    [Donations2] --> [ValidationErrors]
    [Donations2] --> [CharityFractionSets]
    [Donations2] --> [DonorDashboardStats2]
    [Donations2] -r-> [DonationExistence] 
    [Donations2] ----> [DonationRecords2] 

    [Donors2] --> [DonorDashboardStats2]
    [DonationRecords2] --> [DonorDashboardStats2]
    [Charities] --> [AmountsToTransfer]
    [Charities] --> [ValidationErrors]
    [CharityFractionSets] --> [AmountsToTransfer]
    [CharityFractionSets] --> [Allocations]
    [Index] -l-> [ValidationErrors]
    [Index] --> [AuditHistory]
    [HistoryHash] --> [AuditHistory]
    [OptionWorths2] --> [CumulativeInterest]
    [OptionWorths2] --> [DonationStatistics]
    [OptionWorths2] --> [IdealOptionValuations]
    [OptionWorths2] --> [OptionWorthHistory]
    [OptionWorths2] --> [CharityFractionSets]
    [OptionWorths2] --> [DonationRecords2]
    [IdealOptionValuations] --> [MinimalExits]
    [CumulativeInterest] --> [OptionWorthHistory]
    [OptionWorthHistory] --> [IdealOptionValuations]
    [AmountsToTransfer] --> [ValidationErrors]
    [CharityBalance] --> [ValidationErrors]

    [OptionWorthHistory] --> [DonationRecords2]

    url for AmountsToTransfer is [[models/amounts_to_transfer]]
    url for DonorDashboardStats2 is [[models/donor_dashboard_stats]]
    url for Donors2 is [[models/donors]]
    url for Index is [[models/index]]
    url for HistoryHash is [[models/history_hash]]
    url for Options is [[models/options]]
    url for ValidationErrors is [[models/validation_errors]]
    url for Donations2 is [[models/donations]]
    url for OptionWorths2 is [[models/option_worths]]
    url for IdealOptionValuations is [[models/ideal_option_valuations]]
    url for MinimalExits is [[models/minimal_exits]]
    url for MonthlyDonations is [[models/monthly_donations]]
    url for AggregatedDonationsAndTransfers is [[models/aggregated_donations_and_transfers]]
    url for DonationStatistics is [[models/donation_statistics]]
    url for CharityFractionSets is [[models/charity_fraction_sets]]
    url for DonorDashboardStats2 is [[models/donor_dashboard_stats]]
    url for DonationExistence is [[models/donation_existence]]
    url for DonationRecords2 is [[models/donation_records]]
    url for Charities is [[models/charities]]
    url for Allocations is [[models/allocations]]
    url for AuditHistory is [[models/audit_history]]
    url for CumulativeInterest is [[models/cumulative_interest]]
    url for OptionWorthHistory is [[models/option_worth_history]]
    url for CharityBalance is [[models/charity_balance]]
```

_A data flow graph for all the models._ 

## Caching

When a model is requested, the calculator minimizes computation by leveraging cached results.
It checks the [branch](./branch) for cached [`HistoryHash` models](./models/history_hash), which serve as unique identifiers for the entire event sequence.
This allows all derived model data to be efficiently indexed and retrieved using the history hash.

The calculator uses a caching strategy, delegating storage and retrieval of model states to the [Model cache](./model_cache).

## Theories

A theory enables the calculator to compute models based on a current state plus hypothetical events that have not yet been imported into the [Event store](./event_store).
The [`ValidationErrors` model](./models/validation_errors) is used to validate these events before import, ensuring consistency and correctness.
Theories also support calculations such as [`MinimalExits`](./models/minimal_exits) for investment options.

Results from theory-based calculations are not stored in the [Model cache](./model_cache).

## Partitioning

Partitioning splits a model into multiple segments, improving efficiency in storage, network transfer, and memory usage during [calculation](./calculator).
It is especially effective for models with many indexed entries, allowing dynamic partitioning based on entry count and size.
Each partitioned model includes header data for global information and to determine the required number of partitions.
The goal of choosing a paritioning scheme is to minimize the number of partitions affected by any single event, maximizing performance and scalability.

