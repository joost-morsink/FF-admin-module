# Database

## Introduction

We will use two separate data storage technologies for two distinct purposes:

* Git

* Sql

We will use Git for storing all the events that have occured.
Git provides us with a hash code that we can use on our audit reports to reference a certain state in the history of the Future Fund's administration.

> #### TODO
> 
> We need to investigate the possibility of using SHA-256 hashes instead of SHA-1 hashes.
> If not possible we might need to create a content-based hash ourselves.

We will use PostgreSql to store all data relevant to the business domain and processes.
The database will contain all complex calculation logic as well, leaving only the presentation to the application layer.

> #### Assumption
> 
> We assume we will not change database engines in the nearby future. 
> This combined with the performance benefit of having computionally complex logic close to the data leads us to this choice.
> If we would prefer portability over performance, we would have chosen to implement this logic in the application.

## Event storage

The entire chain of events will be stored in Git.
Git repositories are easily backed up to external locations.

Every conversion day process cycle must output all events in a single file and use a timestamp as name.
Every line in the file should contain an event. 
The format used for the event is json.

The application and database is responsible for ensuring the monotonicity (in time) of events.

## Sql database

An Sql database is used to store all the relevant data to facilitate the business process.
Initially this will be a fairly minimum set of tables, but the model might grow in the future as new requirements emerge.

### Event tables

First of all the events need to be stored in the database, which will require an event table containing fields for all the possible events. 

We also would like to record rollback data in case events arrive in case monotonicity is not respected.
This will be recorded in a separate table.

### Main model

The main model should contain the current state of affairs according to all the events that have been processed.
This comprises:

* All donations
* All charities
* All investment funds, invested amounts and cash amounts
* All current ownership fractions of donations in investment funds
* All current allocations for charities
* All money transfers for charities, along with historic or calculated fractions for donations

A notion of a fraction set comes in handy to make it easier on the database to administer historic fractions on donations.

All timestamps are assumed to be UTC.

The relationships between the tables are shown in the following diagram:

![](./images/database.svg)

In this diagram, all relationships are directed from multiple to single.
The only exception from this rule is the relationships from `Allocation` and `Fund` to `Fractionset`:

* `Allocation` is owned by `Fractionset`

* `Fund` is owned by `Fractionset`

> #### TODO
> 
> Determine whether a simple decimal fraction suffices, or if we should adminster numerator/denominator pairs.

#### Donation

The table `Donation` contains all donations and relevant data:

| Field                 | Type    | Description                                                                                         |
| --------------------- | ------- | --------------------------------------------------------------------------------------------------- |
| Donation_Id           | N       | An internal primary key for the donation                                                            |
| Donation_Ext_Id       | AN      | The external id of the donation                                                                     |
| Donor_Id              | AN      | The external id of the donor                                                                        |
| Currency              | AN      | The currency of the donation                                                                        |
| Amount                | N(16,4) | The amount of the donation                                                                          |
| Exchanged_amount      | N(16,4) | The exchanged amount of the donation in the fund's currency                                         |
| Fund_Id               | N       | A reference to the investment fund                                                                  |
| Charity_Id            | N       | A reference to the charity                                                                          |
| Entered               | DT?     | The timestamp when the donation has been entered into the investment fund; empty if not yet entered |
| Exit_actual_valuation | N(16,4) | The actual valuation after the last `Exit`; initial value equal to `Amount`                         |
| Exit_ideal_valuation  | N(16,4) | The ideal valuation after the last `Exit`; initial value equal to `Amount`                          |



> #### Assumption
> 
> Given the situation there is only a single `Enter` between two `Exit` *and* it is positioned right after the `Exit`, we can certainly get away with including the valuation columns here.
> When we choose to deviate from this convention we will have to take a look at it again, and maybe redesign a bit.

#### Charity

The table `Charity` contains all charities.

| Field          | Type | Description                              |
| -------------- | ---- | ---------------------------------------- |
| Charity_Id     | N    | An internal primary key for the donation |
| Charity_Ext_Id | AN   | The external id of the charity           |
| Name           | AN   | The name of the charity                  |

#### Fund

The table `Fund` contains all investment funds, with current worth and current applicable fractions.

| Field                 | Type     | Description                                                                                        |
| --------------------- | -------- | -------------------------------------------------------------------------------------------------- |
| Fund_Id               | N        | An internal primary key for the investment fund                                                    |
| Fund_Ext_Id           | AN       | An external id of the investment fund                                                              |
| Reinvestment_fraction | N(10,10) | The fraction of the profits to reinvest                                                            |
| FutureFund_fraction   | N(10,10) | The fraction of the profits to donate to the future fund                                           |
| Charity_fraction      | N(10,10) | The fraction of the profits to donate to the charity                                               |
| Bad_year_fraction     | N(10,10) | The fraction of the total amount of money in the investment fund that should always be transferred |
| Currency              | AN       | The currency of the investment fund                                                                |
| Invested_amount       | N(20,4\) | The current amount of invested money in the investment fund                                        |
| Cash_amount           | N(20,4)  | The current amount of cash in the investment fund                                                  |
| Fractionset_Id        | N        | A reference to the current ownership fractions of the investment fund                              |
| Last_Exit             | DT?      | A timestamp of the last `Exit` on this investment fund                                             |

#### Allocation

The table `Allocation` contains all the money allocated to charities due to the `Exit` event.

| Field          | Type    | Description                                                          |
| -------------- | ------- | -------------------------------------------------------------------- |
| Allocation_Id  | N       | An internal primary key for the allocation                           |
| Timestamp      | DT      | The datetime of the allocation                                       |
| Fund_Id        | N       | A reference to the investment fund                                   |
| Charity_Id     | N       | A reference to the charity                                           |
| Fractionset_Id | N       | A reference to the fraction set of the allocation                    |
| Amount         | N(20,4) | The amount of money allocated in the currency of the investment fund |
| Transferred    | B       | True if the allocation has actually been transferred to the charity  |

#### Transfer

The table `Transfer` contains all the transactions of money transfers to charities.

| Field              | Type    | Description                                          |
| ------------------ | ------- | ---------------------------------------------------- |
| Transfer_Id        | N       | An internal primary key for the transfer             |
| Timestamp          | DT      | The timestamp of the transfer.                       |
| Charity_Id         | N       | A reference to the charity                           |
| Currency           | AN      | The currency of the transfer                         |
| Amount             | N(20,4) | The amount of the transfer                           |
| Exchanged_Currency | AN?     | The optional currency of the transfer after exchange |
| Exchanged_Amount   | N(20,4) | The amount of the transfer after currency exchange   |

#### Fractions

The `Fractionset` table groups multiple fractions into a set.

| Field          | Type | Description                                |
| -------------- | ---- | ------------------------------------------ |
| Fractionset_Id | N    | An internal primary key for a fraction set |
| Created        | DT   | A creation timestamp for the fraction set  |

The `Fraction` table contains the actual link to donations and the fraction the donation holds.

| Field          | Type     | Description                                       |
| -------------- | -------- | ------------------------------------------------- |
| Fraction_Id    | N        | An internal primary key for an ownership fraction |
| Fractionset_Id | N        | A reference to the fraction set                   |
| Donation_Id    | N        | A reference to the donation                       |
| Fraction       | N(20,20) | The actual fraction                               |

All fractions belonging to a fraction set should sum up to 1, or very close to it (rounding errors may occur).
We might have to correct for rounding errors at a later time.

### Database code

As mentioned earlier in this document, the Sql database will contain the logic for processing all events and making the necessary calculations.
In PostgreSql the most obvious choice is to use functions for all types of logic.

* Import Event, calls
  
  * Import specific event (x11)

* Process events, calls
  
  * Process event, calls
    * Process specific event (x11)

* Rollback events, calls
  
  * Rollback event, calls
    * Rollback specific event (x11)

* Calculate liquidation

* Create audit report

* Create export

This total 41 functions, most of which are fairly simple.
Some of the more complex functions might call even more functions, but that is an implementation detail and out of scope for this document.

#### Processing events

> #### TODO
> 
> Describe the steps to take when processing events.

#### Rolling events back

> #### TODO
> 
> Describe the steps to take when rolling events back.
