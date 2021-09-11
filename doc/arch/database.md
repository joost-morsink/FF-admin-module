# Database

## Introduction

We will use two separate data storage technologies for two distinct purposes:

* Git

* Sql

We will use Git for storing all the events that have occured.
Git provides us with a hash code that we can use on our audit reports to reference a certain state in the history of the Future Fund's administration.

> #### Todo
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

The application and database are responsible for ensuring the monotonicity (in time) of events.

## Event tables

An Sql database is used to store all the relevant data to facilitate the business process.
Initially this will be a fairly minimum set of tables, but the model might grow in the future as new requirements emerge.

First of all the events need to be stored in the database, which will require an event table containing fields for all the possible events. 

We also would like to record rollback data in case events arrive in case monotonicity is not respected.
This will be recorded in a separate table.
This part can be postponed to a later stage.

### Event

| Field                     | Type     | Description                                                                                                  | Used in                                                                                                          |
| ------------------------- | -------- | ------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------- |
| Event_Id                  | N        | Identifies the even within the database, auto increment                                                      | All                                                                                                              |
| Type                      | AN       | Identifies the event type                                                                                    | All                                                                                                              |
| Timestamp                 | DT       | The timestamp of the event                                                                                   | All                                                                                                              |
| Name                      | AN       | An external name for an entity, may apply to different kind of entities based on the event type              | META_NEW_CHARITY, META_NEW_OPTION                                                                                |
| Option_currency           | AN       | An ISO-4217 currency code for the investment option                                                          | META_NEW_OPTION                                                                                                  |
| Reinvestment_fraction     | N(10,10) | The fraction of the profits to reinvest                                                                      | META_NEW_OPTION, META_UPDATE_FRACTIONS                                                                           |
| FutureFund_fraction       | N(10,10) | The fraction of the profits to donate to the future fund                                                     | META_NEW_OPTION, META_UPDATE_FRACTIONS                                                                           |
| Charity_fraction          | N(10,10) | The fraction of the profits to donate to the charity                                                         | META_NEW_OPTION, META_UPDATE_FRACTIONS                                                                           |
| Bad_year_fraction         | N(10,10) | The minimal fraction of the total amount of money in the investment option that should always be transferred | META_NEW_OPTION, META_UPDATE_FRACTIONS                                                                           |
| Donor_id                  | AN       | The external identifier for the donor                                                                        | DONA_NEW                                                                                                         |
| Charity_id                | AN       | The external identifier for the charity                                                                      | DONA_NEW, CONV_EXIT, CONV_TRANSFER                                                                               |
| Option_id                 | AN       | The external identifier for the investment option                                                            | META_NEW_OPTION, META_UPDATE_FRACTIONS, DONA_NEW, PRICE_INFO, CONV_ENTER, CONV_INVEST, CONV_LIQUIDATE, CONV_EXIT |
| Donation_currency         | AN       | An ISO-4217 currency code for the donation                                                                   | DONA_NEW                                                                                                         |
| Donation_Amount           | N(16,4)  | The donated amount                                                                                           | DONA_NEW                                                                                                         |
| Exchanged_donation_amount | N(16,4)  | The donated amount in the currency of the investment option                                                  | DONA_NEW                                                                                                         |
| Transaction_reference     | AN       | An external reference for the donation transaction                                                           | DONA_NEW, CONV_INVEST, CONV_LIQUIDATE                                                                            |
| Exchange_reference        | AN?      | An optional external reference for the exchange transaction                                                  | DONA_NEW, CONV_TRANSFER                                                                                          |
| Invested_amount           | N(20,4)  | The total invested amount of money in the fund, according to the current investment fund price               | PRICE_INFO, CONV_ENTER, CONV_INVEST, CONV_LIQUIDATE                                                              |
| Cash_amount               | N(20,4)  | The total cash amount of money in the investment option                                                      | PRICE_INFO, CONV_INVEST, CONV_LIQUIDATE                                                                          |
| Exit_amount               | N(20,4)  | The amount to be donated to charities                                                                        | CONV_EXIT                                                                                                        |
| Transfer_currency         | AN       | An ISO-4217 currency code for the transfer                                                                   | CONV_TRANSFER                                                                                                    |
| Transfer_amount           | N(20,4)  | The amount transferred to the charity before exchange                                                        | CONV_TRANSFER                                                                                                    |
| Exchanged_transfer_amount | N(20,4)  | The amount transferred to the charity after exchange                                                         | CONV_TRANSFER                                                                                                    |
| Processed                 | B        | Indicates whether the event has been processed                                                               | N/A                                                                                                              |

### Rollback

The rollback model will not yet be implemented, because we will consider this for the next phase.

## Main model

The main model should contain the current state of affairs according to all the events that have been processed.
This comprises:

* All donations
* All charities
* All investment options, invested amounts and cash amounts
* All current ownership fractions of donations in investment options
* All current allocations for charities
* All money transfers for charities, along with historic or calculated fractions for donations

A notion of a fraction set comes in handy to make it easier on the database to administer historic fractions on donations.

All timestamps are assumed to be UTC.

The relationships between the tables are shown in the following diagram:

![](./images/database.svg)

In this diagram, all relationships are directed from multiple to single.
The only exception from this rule is the relationships from `Allocation` and `Option` to `Fractionset`:

* `Allocation` is owned by `Fractionset`

* `Option` is owned by `Fractionset`

> #### Assumption
> 
> We will use decimal fractions for our administration, because we don't expect any significant rounding errors by using 20 significant decimals.
> We might need to renormalize fractions in a fraction set to sum to 1, or we might need to introduce rational fractions when rounding errors are becoming significant.
> We do not have strategy for determining whether we have significant errors or not.

### Donation

The table `Donation` contains all donations and relevant data:

| Field            | Type     | Description                                                                                           |
| ---------------- | -------- | ----------------------------------------------------------------------------------------------------- |
| Donation_Id      | N        | An internal primary key for the donation                                                              |
| Donation_Ext_Id  | AN       | The external id of the donation                                                                       |
| Donor_Id         | AN       | The external id of the donor                                                                          |
| Currency         | AN       | The currency of the donation                                                                          |
| Amount           | N(16,4)  | The amount of the donation                                                                            |
| Exchanged_amount | N(16,4)? | The exchanged amount of the donation in the option's currency                                         |
| Option_Id        | N        | A reference to the investment option                                                                  |
| Charity_Id       | N        | A reference to the charity                                                                            |
| Entered          | DT?      | The timestamp when the donation has been entered into the investment option; empty if not yet entered |

### Charity

The table `Charity` contains all charities.

| Field           | Type | Description                                                          |
| --------------- | ---- | -------------------------------------------------------------------- |
| Charity_Id      | N    | An internal primary key for the donation                             |
| Charity_Ext_Id  | AN   | The external id of the charity                                       |
| Name            | AN   | The name of the charity                                              |
| Bank_name       | AN   | The name of the charity as registered for the charity's bank account |
| Bank_account_no | AN   | The bank account number for the charity (IBAN for EU zone)           |
| Bank_Bic        | AN?  | The BIC for the charity's bank (Optional for EU zone)                |

### Option

The table `Option` contains all investment funds, with current worth and current applicable fractions.

| Field                 | Type     | Description                                                                                          |
| --------------------- | -------- | ---------------------------------------------------------------------------------------------------- |
| Option_Id             | N        | An internal primary key for the investment option                                                    |
| Option_Ext_Id         | AN       | An external id of the investment option                                                              |
| Reinvestment_fraction | N(10,10) | The fraction of the profits to reinvest                                                              |
| FutureFund_fraction   | N(10,10) | The fraction of the profits to donate to the future fund                                             |
| Charity_fraction      | N(10,10) | The fraction of the profits to donate to the charity                                                 |
| Bad_year_fraction     | N(10,10) | The fraction of the total amount of money in the investment option that should always be transferred |
| Currency              | AN       | The currency of the investment option                                                                |
| Invested_amount       | N(20,4)  | The current amount of invested money in the investment option                                        |
| Cash_amount           | N(20,4)  | The current amount of cash in the investment option                                                  |
| Fractionset_Id        | N        | A reference to the current ownership fractions of the investment option                              |
| Last_Exit             | DT?      | A timestamp of the last `Exit` on this investment option                                             |
| Exit_actual_valuation | N(16,4)  | The actual valuation after the last `Exit`; initial value equal to `Amount`                          |
| Exit_ideal_valuation  | N(16,4)  | The ideal valuation after the last `Exit`; initial value equal to `Amount`                           |

> #### Assumption
> 
> Given the situation there is only a single `Enter` between two `Exit` *and* it is positioned right after the `Exit`, we can certainly get away with including the valuation columns here.
> When we choose to deviate from this convention we will have to take a look at it again, and maybe redesign a bit.

### Allocation

The table `Allocation` contains all the money allocated to charities due to the `Exit` event.

| Field          | Type    | Description                                                            |
| -------------- | ------- | ---------------------------------------------------------------------- |
| Allocation_Id  | N       | An internal primary key for the allocation                             |
| Timestamp      | DT      | The datetime of the allocation                                         |
| Option_Id      | N       | A reference to the investment option                                   |
| Charity_Id     | N       | A reference to the charity                                             |
| Fractionset_Id | N       | A reference to the fraction set of the allocation                      |
| Amount         | N(20,4) | The amount of money allocated in the currency of the investment option |
| Transferred    | B       | True if the allocation has actually been transferred to the charity    |

### Transfer

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

### Fractions

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
| Fraction       | N(21,20) | The actual fraction                               |

All fractions belonging to a fraction set should sum up to 1, or very close to it (rounding errors may occur).
We might have to correct for rounding errors at a later time.

## Database code

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

## Processing events

Each and every event that is processed is first entered into the `Event` table. 
A primary key (auto incerement) is assigned to the event, and it is checked the timestamp is not before the last timestamp.

The metadata events and the '_New donation_' event perform a single insert/update on the database table that corresponds to the event.

The `Price info` event sets the `Invested_amount` field of the `Option`.

The data in the `Invest` event is used to subtract from `Cash_amount` and add to `Invested_amount`, conversely the data in the `Liquidate` event is used to subtract from the `Invested_amount` to the `Cash_amount`.

The charity transfer should set the `Transferred` flag to `true`.

### Enter

When the `Enter` event occurs, a candidate set of donations are selected by using the following criteria:

* The donation must correspond to the selected `Option`

* The donation's `Entered` timestamp must be `NULL`

* The donation must be at least 8 weeks old (because of cancellations of automatic collections)

* The total amount of the `Option` is recorded for recalculation of the fraction set.

Then the following changes should apply atomically:

* The sum of all these donations is added to the `Cash_amount` of the `Option`

* The `Invested_amount` is updated on the `Option` with event data

* The donations' `Entered` timestamp should be set to the timestamp of the event

* A new `Fractionset` is created

* Each `Fraction` of the old `Fractionset` is copied with a factor `(old amount/new amount)`

* Each donation is given a `Fraction` in the `Fractionset` of `(amount/new amount)`

### Calculate liquidation

This should be done after updating the `Invested_amount` either by processing a `Price_info` event or an `Enter` event.

We use the following columns for the situation during the last `Exit` (the one before the one we're preparing now):

* `Last_Exit`

* `Exit_actual_valuation`

* `Exit_ideal_valuation`

We can also calculate how much donations have been `Entered` in the `Option` since the last `Exit`.

We can now calculate the new ideal valuation based on profits made in the last period.
This ideal valuation is subjected to the fractions configured on the `Option` to determine how much money should be transferred.
The current cash amount can be subtracted from this amount to determine how much money should be liquidated from the investment fund.

> #### Assumption
> 
> Given the situation there is only a single `Enter` between two `Exit` *and* it is positioned right after the `Exit`, we let the `Enter`ed donations take part of the entire timespan between `Exit`s.
> So no fractional periods habve to be calculated.
> When we choose to deviate from this convention we will have to take a look at it again, and maybe redesign a bit.

> #### Warning
> 
> If the `Option` has a actual valuation below the ideal valuation on the last `Exit`, all newly `Enter`ed donations will be treated as if they also have a higher ideal valuation.
> If this is not what we aim to do, we need to move the columns to the donation level.

### Exit

The `Exit` event uses the `Fractionset` to determine how much of the amount in the event data should go to each charity allocation.
It atomically:

* reduces the `Cash_amount` of the `Option`

* Creates an `Allocation` and a `Fraction set` for each charity that has money allocated to it.
  Each `Fractionset` is calculated based on the donations that were made with the charity as its destination.
  Each applicable fraction is entered multiplied by `Fraction in fund / fraction sum of donations for charity`

The sum of the `Allocation`s should equal the `Cash_amount` reduction.

## Rolling events back

> #### Todo
> 
> Rolling back events requires more data to be stored in the model, and as such will only be documented in the next phase.
