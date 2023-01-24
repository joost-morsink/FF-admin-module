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
  
  * Charity Update
  
  * Charity Partition

  * New Option
  
  * Update Option fractions

* Main events
  
  * New Donation
  
  * Cancel donation
  
  * Change charity for donation

  * Price info

* Conversion day events
  
  * Enter 
  
  * Invest
  
  * Calculate liquidation
  
  * Liquidate
  
  * Exit
  
  * Charity transfer

Events will generally happen in the order illustrated by the following picture:

![Events](./images/process-events.svg)

Metadata events in _red_ will generally only happen in the beginning of an iteration. 
Donations in _green_ can happen all the time.
Conversion day events in=_yellow_, out=_blue_ happen in the order shown.

The conversion day starts at the left, although the first ever conversion day event has been/will be the `Enter` event.
The reason for this is there is nothing to liquidate when there has never been anything invested.
The conversion day process makes a full cycle of all of the events, although we might change this in the future.

A full grammar for the conversion day events is as follows:

```
CALCULATE+ LIQUIDATE EXIT TRANSFER? ENTER+ INVEST
```

We might optimize the `Enter` and `Exit` events in the future to reduce transaction costs, if applicable.
The optimized grammar is as follows:

```
CALCULATE+ (LIQUIDATE ENTER EXIT | ENTER EXIT INVEST) TRANSFER?
```

### Other events

Note that there is no notion of a new donor. 
Donors will be known only to the online system, although an identifier is carried over to the offline system for tracing donations by same donors.
This also limits any GDPR considerations to the online system.

There will be more events in the future, as we only have considered regular operation of the Future Fund. 
We still have to specify and implement events for cases such as charity end-of-life (or mergers), but this is beyond the scope of the initial project.

## Event specification

Events are specified using a field by field description.

Every event has a timestamp to validate the sequence of events and automatically the scoping of handling the events.
The order of the events is:

- First, the session timestamp

- Then, the event timestamp

The reasoning behind this is that you can only make decisions based on the knowledge you have.
All events received 'out of sync' may only be applied after undoing everything already recorded with a higher timestamp and rerunning them in the correct order.
By ordering them by session timestamp first, this is only needed in case we introduce an 'out of sync' session.

To avoid ambiguity in the timestamps, they will all be serialized as UTC timestamps according to the ISO-8601 format (with a trailing 'Z').

### Meta and main events

#### New Charity

This event creates a new charity that can be chosen by a donor as a beneficiary for donations.

| Field     | Type               | Description             | Value            |
| --------- | ------------------ | ----------------------- | ---------------- |
| Type      | A                  | Identifies the event    | META_NEW_CHARITY |
| Timestamp | DateTime(ISO-8601) | The time of the event   |                  |
| Code      | AN                 | Identifies the charity  |                  |
| Name      | AN                 | The name of the charity |                  |

#### Charity update

This event updates information on the charity, like bank information. Only supplied data should be used for update.

| Field           | Type               | Description                                   | Value               |
| --------------- | ------------------ | --------------------------------------------- | ------------------- |
| Type            | A                  | Identifies the event                          | META_UPDATE_CHARITY |
| Timestamp       | DateTime(ISO-8601) | The time of the event                         |                     |
| Code            | AN                 | Identifies the charity                        |                     |
| Name            | AN?                | The name of the charity                       |                     |
| Bank_account_no | AN?                | The charity's bank account number             |                     |
| Bank_name       | AN?                | The charity's name, as registered by the bank |                     |
| Bank_bic        | AN?                | The charity's bank identification code        |                     |

#### Charity Partition

This events makes a charity into a 'donation theme' by specifying the distribution of the theme.
If the partitions are empty or contain only a self-reference, a donation theme is restored as an actual charity.

| Field           | Type               | Description                                   | Value                  |
| --------------- | ------------------ | --------------------------------------------- | ---------------------- |
| Type            | A                  | Identifies the event                          | META_CHARITY_PARTITION |
| Timestamp       | DateTime(ISO-8601) | The time of the event                         |                        |
| Code            | AN                 | Identifies the donation theme                 |                        |
| Partitions      | (AN, N(20,10))     | The partitions of the theme                   |                        |

#### New Option

This event creates a new investment option that can be used for investing the donations.
The three fractions (`Reinvestment_fraction` + `FutureFund_fraction` + `Charity_fraction`) in the event should add up to 1.
The `Bad_year_fraction` does not apply to profits, like the other three fractions, but instead it applies to the invested value of the donation.

| Field                 | Type                | Description                                                                                                  | Value           |
| --------------------- | ------------------- | ------------------------------------------------------------------------------------------------------------ | --------------- |
| Type                  | A                   | Identifies the event                                                                                         | META_NEW_OPTION |
| Timestamp             | DateTime (ISO-8601) | The time of the event                                                                                        |                 |
| Code                  | AN                  | Identifies the investment option                                                                             |                 |
| Name                  | AN                  | The name of the investment option                                                                            |                 |
| Currency              | AN                  | The ISO-4217 currency code of the investment option                                                          |                 |
| Reinvestment_fraction | N(10,10)            | The fraction of the profits to reinvest                                                                      |                 |
| FutureFund_fraction   | N(10,10)            | The fraction of the profits to donate to the future fund                                                     |                 |
| Charity_fraction      | N(10,10)            | The fraction of the profits to donate to the charity                                                         |                 |
| Bad_year_fraction     | N(10,10)            | The minimal fraction of the total amount of money in the investment option that should always be transferred |                 |

#### Update Option fractions

This event updates the reinvestment fraction for a fund.
The three fractions in the event should add up to 1.

| Field                 | Type                | Description                                                                                                  | Value                 |
| --------------------- | ------------------- | ------------------------------------------------------------------------------------------------------------ | --------------------- |
| Type                  | A                   | Identifies the event                                                                                         | META_UPDATE_FRACTIONS |
| Timestamp             | DateTime (ISO-8601) | The time of the event                                                                                        |                       |
| Code                  | AN                  | The identifier for the option                                                                                |                       |
| Reinvestment_fraction | N(10,10)            | The fraction of the profits to reinvest                                                                      |                       |
| FutureFund_fraction   | N(10,10)            | The fraction of the profits to donate to the future fund                                                     |                       |
| Charity_fraction      | N(10,10)            | The fraction of the profits to donate to the charity                                                         |                       |
| Bad_year_fraction     | N(10,10)            | The minimal fraction of the total amount of money in the investment option that should always be transferred |                       |

#### New Donation

This event represents some donation made by some donor to some charity through investment in some fund.
If the currency matches the investment fund's currency, the fields `Amount` and `Exchanged_amount` are equal, otherwise the result of currency exchange is recorded in the `Exhanged_amount` field.

| Field                 | Type                 | Description                                                                                             | Value    |
| --------------------- | -------------------- | ------------------------------------------------------------------------------------------------------- | -------- |
| Type                  | A                    | Identifies the event                                                                                    | DONA_NEW |
| Timestamp             | DateTime (ISO-8601)  | The timestamp of the event                                                                              |          |
| Execute_timestamp     | DateTime (ISO-8601)? | The timestamp the donation should be considered valid, if not present, consider it equal to `Timestamp` |          |
| Donation              | AN                   | Identifies the donation                                                                                 |          |
| Donor                 | AN                   | The identifier for the donor                                                                            |          |
| Charity               | AN                   | The identifier for the charity                                                                          |          |
| Option                | AN                   | The identifier for the investment option                                                                |          |
| Currency              | AN                   | An ISO-4217 currency code                                                                               |          |
| Amount                | N(16,4)              | The donated amount                                                                                      |          |
| Exchanged_amount      | N(16,4)              | The donated amount in the currency of the investment option                                             |          |
| Transaction_reference | AN                   | An external reference for the donation transaction                                                      |          |
| Exchange_reference    | AN?                  | An optional external reference for the exchange transaction                                             |          |

#### Cancel donation

This event represents revocation of a direct debit, and is only valid if the timestamp for the cancellation lies before the execution timestamp of the original donation.

| Field     | Type                | Description                                      | Value       |
| --------- | ------------------- | ------------------------------------------------ | ----------- |
| Type      | A                   | Identifies the event                             | DONA_CANCEL |
| Timestamp | DateTime (ISO-8601) | The timestamp of the event                       |             |
| Donation  | AN                  | Identifies the donation that should be cancelled |             |

#### Change charity for donation

This event represents the users decision to change the beneficiary charity of the donation's profits.

| Field     | Type                | Description                                      | Value               |
| --------- | ------------------- | ------------------------------------------------ | ------------------- |
| Type      | A                   | Identifies the event                             | DONA_UPDATE_CHARITY |
| Timestamp | DateTime (ISO-8601) | The timestamp of the event                       |                     |
| Donation  | AN                  | Identifies the donation that should be changed   |                     |
| Charity   | AN                  | The identifier for the charity                   |                     |


#### Price info

This event allows for the input of fund pricing info, so the donations' worth can be more easily plotted.

| Field           | Type                | Description                                                                                    | Value      |
| --------------- | ------------------- | ---------------------------------------------------------------------------------------------- | ---------- |
| Type            | A                   | Identifies the event                                                                           | PRICE_INFO |
| Timestamp       | DateTime (ISO-8601) | The timestamp of the event                                                                     |            |
| Option          | AN                  | The identifier for the option                                                                  |            |
| Invested_amount | N(20,4)             | The total invested amount of money in the fund, according to the current investment fund price |            |
| Cash_amount     | N(20,4)             | The total cash amount of money in the investment option                                        |            |

### Conversion day

#### Conversion day Enter

This event signifies the point in time at which all new donations are made part (in cash form) of the new investment option.

Although the `Invested_amount` does not seem to have anything to do with the `Enter` event, it is the only piece of data that is both not yet known and it influences the recalculation of ownership fractions.

| Field           | Type                | Description                                                                                                        | Value      |
| --------------- | ------------------- | ------------------------------------------------------------------------------------------------------------------ | ---------- |
| Type            | A                   | Identifies the event                                                                                               | CONV_ENTER |
| Timestamp       | DateTime (ISO-8601) | The timestamp of the event                                                                                         |            |
| Option          | AN                  | The identifier for the option                                                                                      |            |
| Invested_amount | N(20,4)             | The total **invested** amount of money in the investment option (possibly calculated by current price information) |            |

#### Conversion day Invest

This event represents the investment of an amount of cash into the actual investment fund.

| Field                 | Type                | Description                                                            | Value       |
| --------------------- | ------------------- | ---------------------------------------------------------------------- | ----------- |
| Type                  | A                   | Identifies the event                                                   | CONV_INVEST |
| Timestamp             | DateTime (ISO-8601) | The timestamp of the event                                             |             |
| Option                | AN                  | The identfier for the investment option                                |             |
| Invested_amount       | N(20,4)             | The new total invested amount of money in the investment fund          |             |
| Cash_amount           | N(20,4)             | The new total cash amount of money in the investment option's reserves |             |
| Transaction_reference | AN                  | An external reference for the investment transaction                   |             |

#### Conversion day Calculate

Although the Calculate event is an important event in the business process of the Future Fund, it plays no role in the recreation of history by replaying events and is therefore skipped.

#### Conversion day Liquidate

This event represents a liquidation from an investment fund, for the purpose of donating the withdrawn money to the charities. 
This event only pertains to the liquidation of invested money to the cash part of the investment option.

| Field                 | Type                | Description                                                            | Value          |
| --------------------- | ------------------- | ---------------------------------------------------------------------- | -------------- |
| Type                  | A                   | Identifies the event                                                   | CONV_LIQUIDATE |
| Timestamp             | DateTime (ISO-8601) | The timestamp of the event                                             |                |
| Option                | AN                  | The identifier for the investment option                               |                |
| Invested_amount       | N(20,4)             | The new total invested amount of money in the investment option        |                |
| Cash_amount           | N(20,4)             | The new total cash amount of money in the investment option's reserves |                |
| Transaction_reference | AN                  | An external reference for the withdrawal transaction                   |                |

#### Conversion day Exit

This event represents an allocation of money to charities from the cash amount of an investment option.
The actual transfer to the charities may be delayed.

| Field     | Type                | Description                             | Value     |
| --------- | ------------------- | --------------------------------------- | --------- |
| Type      | A                   | Identifies the event                    | CONV_EXIT |
| Timestamp | DateTime (ISO-8601) | The timestamp of the event              |           |
| Option    | AN                  | The identifier of the investment option |           |
| Amount    | N(20,4)             | The amount to be donated to charities   |           |

#### Conversion day Charity transfer

This event represents the actual transfer of allocated money to the charity

| Field                 | Type                | Description                                                                    | Value         |
| --------------------- | ------------------- | ------------------------------------------------------------------------------ | ------------- |
| Type                  | A                   | Identifies the event                                                           | CONV_TRANSFER |
| Timestamp             | DateTime (ISO-8601) | The timestamp of the event                                                     |               |
| Charity               | AN                  | The identifier of the charity                                                  |               |
| Currency              | AN                  | An ISO-4217 currency code                                                      |               |
| Amount                | N(20,4)             | The amount donated to the charity                                              |               |
| Exhanged_Currency     | AN                  | An ISO-4217 currency code for the currency after exchange (charity's currency) |               |
| Exchanged_Amount      | N(20,4)             | The amount donated to the charity in the exchanged currency.                   |               |
| Transaction_reference | AN                  | External reference code for transaction                                        |               |
| Exchange_reference    | AN?                 | An optional external reference for the exchange                                |               |
