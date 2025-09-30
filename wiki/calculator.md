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
    M_t = \Pi_i m_{i,t}
$$

Simple submodels can be calculated based on the old state and the event to be processed:

$$
    m_t \times E \rightarrow m_{t+1}
$$

But they also may take advantage of the information in other submodels:

$$
    M_t \times E \rightarrow m_{t+1}
$$

For dependent models both the old value and the calculated values may be used:

$$
    M_t \times m_{i,t+1} \times E \rightarrow m_{j,t+1}
$$

These submodels are of course not allowed to have circular dependencies on the calculated (t+1)submodels.

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

The Index model keeps track of the index of the current event. 
Every index in the sequence has an event associated with it that lead to the current state.
For the event that preceded the start state (at t=0), the [NONE](./events/NONE.md) event is assumed to have happened.

Models can either be processed or calculated. 
Processed models use a processor to apply event data to the model, as well as other model data it depends on.
Processed models can also implement some form of [partitioning](./model_partition)
Calculated models don't use event data directly, but use a calculator to calculate its value based on the values of other models (either calculated or processed).

Currently, calculated models are not cached, but are parameterized.
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

    [Options] --> [AggregatedDonationsAndTransfers]
    [Options] --> [AmountsToTransfer]
    [Options] --> [DonationStatistics]
    [Options] --> [MinimalExits]
    [Options] --> [IdealOptionValuations]
    [Options] --> [ValidationErrors]
    [Options] --> [MonthlyDonations]
    [Donations2] --> [AggregatedDonationsAndTransfers]
    [Donations2] --> [DonationStatistics]
    [Donations2] --> [ValidationErrors]
    [Donations2] --> [CharityFractionSets]
    [Donations2] --> [DonorDashboardStats2]
    [Donations2] --> [DonationExistence] 
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

When making a request for some model, the calculator tries to minimize the work being done.
The [branch](./branch) is checked for cached [`HistoryHash` models](./models/history_hash) which can be used to retrieved cached models of other types.
Because the `HistoryHash` uniquely determines the entire historical sequence of events, all calculated data can be indexed by this hash.

The calculator has a caching strategy which drives the [Model cache](./model_cache) to store data on the calculator's behalf.

## Theories

A theory is the way the calculator supports calculating models based on a current situation plus some events that have not (yet) been imported into the [Event store](./event_store).
Using the [`ValidationErrors` model](./models/validation_errors) the set of events can be checked for errors on import on some existing sequence of events.
Theories are also used for calculating the [`MinimalExits`](./models/minimal_exits) for an investment option.

Results from theory requests are not cached in the [Model cache](./model_cache).

## Partitioning

> Paritioning is, at the time of writing, still a work in progress.

Partitioning is a system that splits a model into multiple entities for storage. 
Thereby optimizing network transfer and memory usage when [calculating](./calculator).

