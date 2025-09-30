---
title: Model OptionWorthHistory
author: J.W. Morsink
archimate:
  layer: Application
  type: DataObject
  caption: Option worth history
---

# Model OptionWorthHistory

This model records the historical changes in the worth of each [investment option](../option) over time.
Each change is event-sourced, capturing both the previous and new state for every relevant event.

## Purpose

- Track the evolution of option worths, including invested and cash amounts, and divisors.
- Support auditing and historical reporting for option-related calculations.

## Structure

```yaml
Options:
  {OptionId}:
    - EventType: The type of event that triggered the change (e.g. CONV_*, PRICE_INFO, etc.)
      Timestamp: The time of the event
      Old:
        Cash: Previous cash amount
        Invested: Previous invested amount
        Unentered: Previous unentered donations
        CumulativeInterest: Previous cumulative interest
        IdealValue: Previous ideal value
        Value: Previous calculated value
        Divisor: Previous divisor for donation shares
      New:
        Cash: New cash amount
        Invested: New invested amount
        Unentered: New unentered donations
        CumulativeInterest: New cumulative interest
        IdealValue: New ideal value
        Value: New calculated value
        Divisor: New divisor for donation shares
```

## Relationships

- Depends on [`OptionWorths2`](./option_worths) for current worths and option state.
- Integrates cumulative interest from [`CumulativeInterest`](./cumulative_interest) and ideal valuations from [`IdealOptionValuations`](./ideal_option_valuations).
- Used by [`DonationRecords2`](./donation_records) and other models for historical analysis and reporting.

## Events

The following events trigger updates in the option worth history:

- [`META_NEW_OPTION`](../events/META_NEW_OPTION)
- [`CONV_ENTER`](../events/CONV_ENTER)
- [`CONV_INVEST`](../events/CONV_INVEST)
- [`CONV_LIQUIDATE`](../events/CONV_LIQUIDATE)
- [`CONV_EXIT`](../events/CONV_EXIT)
- [`CONV_INFLATION`](../events/CONV_INFLATION)
- [`PRICE_INFO`](../events/PRICE_INFO)
- [`CONV_INCREASE_CASH`](../events/CONV_INCREASE_CASH)

