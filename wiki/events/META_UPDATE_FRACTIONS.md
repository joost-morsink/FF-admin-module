--- 
title: META_UPDATE_FRACTIONS
author: J.W. Morsink
---

# META_UPDATE_FRACTIONS

The `META_UPDATE_FRACTIONS` event updates the reinvestment [fractions](../option_fractions) for an [investment option](../option).
The three fractions (`Reinvestment_fraction`, `FutureFund_fraction`, and `Charity_fraction`) must add up to 1.
The `Bad_year_fraction` specifies the minimal fraction of the total amount in the investment option that should always be transferred, regardless of profits.

## Fields

| Field                   | Type                | Description                                                                                                  | Value                   |
|-------------------------|---------------------|--------------------------------------------------------------------------------------------------------------|-------------------------|
| `Type`                  | string              | Identifies the event.                                                                                        | `META_UPDATE_FRACTIONS` |
| `Timestamp`             | DateTime (ISO-8601) | The time of the event.                                                                                       |                         |
| `Code`                  | string              | The identifier for the investment option.                                                                    |                         |
| `Reinvestment_fraction` | decimal(10,10)      | The fraction of profits to reinvest.                                                                         |                         |
| `FutureFund_fraction`   | decimal(10,10)      | The fraction of profits to donate to the future fund.                                                        |                         |
| `Charity_fraction`      | decimal(10,10)      | The fraction of profits to donate to the charity.                                                            |                         |
| `Bad_year_fraction`     | decimal(10,10)      | The minimal fraction of the total option amount that should always be transferred.                           |                         |

## Purpose

This event is essential for updating how profits and minimum transfers are handled for an investment option.
