# Events

## Introduction

Because of the need for an audit trail we opt for an event sourcing solution:

* The event database directly fulfills the requirement of having an audit trail.

* The event database can be backed up separately and cheaply.

* Our offline system can be easily replaced by something new if the need arises.

This document identifies and specifies all the events that may occur in the Future Fund business model. 
It also specifies a database model for storage of the _current_ situation.

## Initially identified events

The Future Fund business domain contains of the following events:

* New Donation

* New Charity

* New Investment Option

* Update Investment Option reinvestment fraction

* Conversion day 'investments'

* Conversion day calculate 'gifting'

* Conversion day execute 'gifting' 

Note that there is no notion of a new donor. 
Donors will be known only to the online system, although an identifier is carried over to the offline system for tracing donations by same donors.
This also limits any GDPR considerations to the online system.

There will be more events in the future, as we only have consider regular operation of the Future Fund. 
We still have to specify and implement events for cases such as charity end-of-life (or mergers), but this is beyond the scope of the initial project.

## Event specification

Events are specified using a field by field description.

### New Donation

| Field | Type | Value        | Description          |
| ----- | ---- | ------------ | -------------------- |
| Type  | A    | NEW_DONATION | Identifies the event |
| ...   |      |              |                      |

### New Charity

| Field | Type | Value       | Description          |
| ----- | ---- | ----------- | -------------------- |
| Type  | A    | NEW_CHARITY | Identifies the event |
| ...   |      |             |                      |

### New Investment option

| Field | Type | Value                 | Description          |
| ----- | ---- | --------------------- | -------------------- |
| Type  | A    | NEW_INVESTMENT_OPTION | Identifies the event |
| ...   |      |                       |                      |

### Update investment option reinvestment fraction

| Field | Type | Value                        | Description          |
| ----- | ---- | ---------------------------- | -------------------- |
| Type  | A    | UPDATE_REINVESTMENT_FRACTION | Identifies the event |
| ...   |      |                              |                      |

### Conversion day investments

| Field | Type | Value                  | Description          |
| ----- | ---- | ---------------------- | -------------------- |
| Type  | A    | CONVERSION_INVESTMENTS | Identifies the event |
| ...   |      |                        |                      |

### Conversion day calculate gifting

| Field | Type | Value                        | Description          |
| ----- | ---- | ---------------------------- | -------------------- |
| Type  | A    | CONVERSION_CALCULATE_GIFTING | Identifies the event |
| ...   |      |                              |                      |

### Conversion day execute gifting

| Field | Type | Value                      | Description          |
| ----- | ---- | -------------------------- | -------------------- |
| Type  | A    | CONVERSION_EXECUTE_GIFTING | Identifies the event |
| ...   |      |                            |                      |
