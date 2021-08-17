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

* Metadata events
  
  * New Charity
  
  * New Fund
  
  * Update Fund reinvestment fraction

* Donation events
  
  * New Donation

* Conversion day events
  
  * Enter 
  
  * Invest
  
  * Calculate withdrawal
  
  * Withdraw
  
  * Gift

Events will generally happen in the order illustrated by the following picture:

![Events](./images/process-events.svg)

Metadata events in _red_ will generally only happen in the beginning of an iteration. 
Donations in _green_ can happen all the time.
Conversion day events in=_yellow_, out=_blue_ happen in the order shown.

A full grammar for the conversion day events is as follows:

```
(ENTER+ INVEST) | (CALCULATE WITHDRAW GIFT)
```

Note that there is no notion of a new donor. 
Donors will be known only to the online system, although an identifier is carried over to the offline system for tracing donations by same donors.
This also limits any GDPR considerations to the online system.

There will be more events in the future, as we only have consider regular operation of the Future Fund. 
We still have to specify and implement events for cases such as charity end-of-life (or mergers), but this is beyond the scope of the initial project.

## Event specification

Events are specified using a field by field description.

### Meta events and donations

#### New Charity

| Field | Type | Value            | Description          |
| ----- | ---- | ---------------- | -------------------- |
| Type  | A    | META_NEW_CHARITY | Identifies the event |
| ...   |      |                  |                      |

#### New Fund

| Field | Type | Value         | Description          |
| ----- | ---- | ------------- | -------------------- |
| Type  | A    | META_NEW_FUND | Identifies the event |
| ...   |      |               |                      |

#### Update Fund reinvestment fraction

| Field | Type | Value                             | Description          |
| ----- | ---- | --------------------------------- | -------------------- |
| Type  | A    | META_UPDATE_REINVESTMENT_FRACTION | Identifies the event |
| ...   |      |                                   |                      |

#### New Donation

| Field | Type | Value    | Description          |
| ----- | ---- | -------- | -------------------- |
| Type  | A    | DONA_NEW | Identifies the event |
| ...   |      |          |                      |

### Conversion day

#### Conversion day Enter

| Field | Type | Value      | Description          |
| ----- | ---- | ---------- | -------------------- |
| Type  | A    | CONV_ENTER | Identifies the event |
| ...   |      |            |                      |

#### Conversion day Invest

| Field | Type | Value       | Description          |
| ----- | ---- | ----------- | -------------------- |
| Type  | A    | CONV_INVEST | Identifies the event |
| ...   |      |             |                      |

#### Conversion day Calculate

| Field | Type | Value          | Description          |
| ----- | ---- | -------------- | -------------------- |
| Type  | A    | CONV_CALCULATE | Identifies the event |
| ...   |      |                |                      |

#### Conversion day Withdraw

| Field | Type | Value         | Description          |
| ----- | ---- | ------------- | -------------------- |
| Type  | A    | CONV_WITHDRAW | Identifies the event |
| ...   |      |               |                      |

#### Conversion day Gift

| Field | Type | Value     | Description          |
| ----- | ---- | --------- | -------------------- |
| Type  | A    | CONV_GIFT | Identifies the event |
| ...   |      |           |                      |
