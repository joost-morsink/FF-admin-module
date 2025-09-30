---
title: CONV_INFLATION
author: J.W. Morsink
difficulty: medium
---

# CONV_INFLATION

The `CONV_INFLATION` event requests an inflation correction for a specific [investment option](../option).
This event ensures that inflation is properly administered before profits are measured or distributed.
It directly influences the [ideal valuation](../models/ideal_option_valuations) of the option.

## Fields

| Field             | Type                | Description                                                      | Value            |
|-------------------|---------------------|------------------------------------------------------------------|------------------|
| `Type`            | string              | Identifies the event                                             | `CONV_INFLATION` |
| `Timestamp`       | DateTime (ISO-8601) | The timestamp of the event                                      |                  |
| `Option`          | string              | The identifier for the investment option                        |                  |
| `Invested_amount` | decimal(20,4)       | The total invested amount in the investment fund                |                  |
| `Inflation_factor`| decimal(20,4)       | The factor by which monetary amounts should be multiplied       |                  |

## Inflation Factor

The inflation factor is administered as a multiplier rather than a percentage.
This approach keeps calculation logic simple and consistent.
For example, a `2%` inflation is represented by a factor of `1.02`.

## Purpose

This event allows the system to adjust valuations for inflation, ensuring fair and accurate profit calculations.