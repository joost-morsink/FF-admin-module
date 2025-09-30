--- 
title: META_NEW_OPTION
author: J.W. Morsink
---
# META_NEW_OPTION

The `META_NEW_OPTION` event creates a new [investment option](../option) for investing donations.
The event specifies how profits should be divided using three [fractions](../option_fractions): `Reinvestment_fraction`, `FutureFund_fraction`, and `Charity_fraction`.
These three fractions must add up to 1.
The `Bad_year_fraction` is a minimal yearly fraction of the total invested amount that should always be transferred, regardless of profits.
It does not apply to profits, but to the invested value itself.

## Fields

| Field                   | Type                | Description                                                                                                  | Value             |
|------------------------------|---------------------|--------------------------------------------------------------------------------------------------------------|-------------------|
| `Type`                       | string              | Identifies the event.                                                                                        | `META_NEW_OPTION` |
| `Timestamp`                  | DateTime (ISO-8601) | The time of the event.                                                                                       |                   |
| `Code`                       | string              | Identifies the investment option.                                                                            |                   |
| `Name`                       | string              | The name of the investment option.                                                                           |                   |
| `Currency`                   | string              | ISO-4217 currency code of the investment option.                                                             |                   |
| `Reinvestment_fraction`      | decimal(10,10)      | The fraction of profits to reinvest.                                                                         |                   |
| `FutureFund_fraction`        | decimal(10,10)      | The fraction of profits to donate to the future fund.                                                        |                   |
| `Charity_fraction`           | decimal(10,10)      | The fraction of profits to donate to the charity.                                                            |                   |
| `Bad_year_fraction`          | decimal(10,10)      | The minimal yearly fraction of the total option worth that should always be transferred.                     |                   |

## Purpose

This event is essential for onboarding new investment options and defining how profits and minimum transfers are handled.
