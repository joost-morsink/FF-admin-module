---
title: CONV_INFLATION
author: J.W. Morsink
---

# CONV_INFLATION

This [event](../event) represents a request for inflation correction for a particular option.

Inflation should influence the [ideal valuation](../models/ideal_option_valuations) of an [investment option](../option), because of the need to administer inflation before measuring/determining profits.

| Field                   | Type                | Description                                                            | Value            |
| ----------------------- | ------------------- | ---------------------------------------------------------------------- | ---------------- |
| `Type`                  | A                   | Identifies the event                                                   | `CONV_INFLATION` |
| `Timestamp`             | DateTime (ISO-8601) | The timestamp of the event                                             |                  |
| `Option`                | AN                  | The identfier for the investment option                                |                  |
| `Invested_amount`       | N(20,4)             | The total invested amount of money in the investment fund              |                  |
| `Inflation_factor`      | N(20,4)             | The factor by which monetray amounts should be multiplied              |                  |

## Inflation factor

We administer an inflation factor instead of a percentage to keep as much logic as possible out of the [calculator](../calculator).
For instance, if there is a `2%` inflation, the factor is `1.02`.