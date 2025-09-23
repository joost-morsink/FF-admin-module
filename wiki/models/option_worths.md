---
title: Model OptionWorths2
author: J.W. Morsink
archimate:
  layer: Application
  type: DataObject
  caption: Option worths
---

# Model OptionWorths2

This model keeps track of the worths of [investment options](../option).

The following information is tracked by this model:

* The invested amount (can fluctuate due to stock prices fluctuating). 
* The cash amout, is affected by some [events](../event).
* The ownership shares of donations in the investment option in its partitioned details. 
* Unentered donations, affected by donation events. This is not yet part of the actual option's worth, but the information is needed on some conversion day events.

```plantuml
@startyaml
NumberOfDonations: The number of donations
TotalUnentered: The total amount of money not yet entered into the options.
Worths: 
  {OptionId}:
    Id: A repetition of the option's Id
    Timestamp: The timestamp of the latest worth determination
    Invested: The invested amount
    Cash: The cash amount
    DonationFractionDivisor: The divisor for the registered donations' shares.
    DonationFractions:
      - Key: {DonationId}
        Value: The@startyam ownership fraction of the donation in the option.
      - ...
    UnenteredDonations:
      - Id: The id of the donation
        Timestamp: The time of donation
        ExecuteTimestamp: The time at which the donation may be considered made.
        OptionId: The id of the investment option
        CharityId: The id of the charity
        Amount: The amount of money donated, in the option's currency.
      - ...
    EnteringDonations:
      Divisor: The new DonationFractionDivisor
      Factor: The factor needed to convert from donated amount to share size.
      Donations:
        - Id: The id of the donation
          Timestamp: The time of donation
          ExecuteTimestamp: The time at which the donation may be considered made.
          OptionId: The id of the investment option
          CharityId: The id of the charity
          Amount: The amount of money donated, in the option's currency.
          OriginalCurrency: The currency of the original donation.
          OriginalAmount: The amount of money donated, in the original currency.
@endyaml
```

The details are indexed by donation id and are structured as follows:

```plantuml
@startyaml
Timestamp: The timestamp of the moment the donation entered the option.
Share: The share the donation has in the option's worth.
IsEntered: Calculated property indicating whether the donation has entered the option.
@endyaml
```

## Events

The `OptionWorths2` model is affected by the following events:

* [`DONA_NEW`](../events/DONA_NEW)
* [`DONA_CANCEL`](../events/DONA_CANCEL)
* [`META_NEW_OPTION`](../events/META_NEW_OPTION)
* [`CONV_ENTER`](../events/CONV_ENTER)
* [`CONV_INVEST`](../events/CONV_INVEST)
* [`CONV_LIQUIDATE`](../events/CONV_LIQUIDATE)
* [`CONV_EXIT`](../events/CONV_EXIT)
* [`CONV_INCREASE_CASH`](../events/CONV_INCREASE_CASH)
* [`CONV_INFLATION`](../events/CONV_INFLATION)
* [`PRICE_INFO`](../events/PRICE_INFO)

### DONA_NEW

When a [new donation](../donation) has been made, it is added to the `UnenteredDonations` collection of the corresponding investment option.

### DONA_CANCEL

When a donation is cancelled, it is removed from the `UnenteredDonations` collection or the `DonationFractions` [fraction set](../fraction_set).

### META_NEW_OPTION

A new option is registered, without any linked donations and with 0 invested and cash amounts.

### CONV_ENTER

This event is a so called [mega event](../model-partition#mega_event).
The `UnenteredDonations` are checked for donations that have an `ExecuteTimestamp` earlier than the timestamp of the event. 
Those donations are cleared from the `UnenteredDonations` and the details of those donations are assigned an ownership [share](../share) of the investment option.
The summed amount of the moved donations is transferred into the cash part of the investment option.

### CONV_INVEST

This event indicates that an investment has been made and that a cash amount has been transferred to the invested part of the investment option.
There is no change in the details nor the `UnenteredDonations`.

### CONV_LIQUIDATE

This event indicates that a liquidation of stocks has been made and that a part of the invested part of the investment option has been transferred to the cash part.

### CONV_EXIT

This event indicates the withdrawal of funds for the purpose of pay out to [charities](../charities). 
It only affects the cash part of the investment option.

> **Assumption:** [`CONV_EXIT`](../events/CONV_EXIT) always immediately follows a [`CONV_LIQUIDATE`](../events/CONV_LIQUIDATE), assuming no stock price change in between. 
>
> **Consequence**:  If no liquidation is needed for an exit, a 0 amount liquidation must be added to 'set' the invested amount to the proper value.

### CONV_INFLATION

Inflation correction events contain stock pricing information and affects the investment option's total worth.

### CONV_INCREASE_CASH

Investment costs may be covered by a third party, which increases the cash part of the investment option.
It only affects the cash part of the investment option.

### PRICE_INFO

This events adds stock pricing information to the event stream and affects the investment option's total worth.
Both invested and cash parts are present in the event and used to change the entry in the model.

## Usage

This model is used in the [`IdealOptionValuations` model](./ideal_option_valuations) as that model depends on the worth of the investment options.