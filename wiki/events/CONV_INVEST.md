---
title: CONV_INVEST
author: J.W. Morsink
difficulty: medium
---

# CONV_INVEST

The `CONV_INVEST` event records the investment of cash into an [investment option](../option).
It updates both the total invested amount and the cash reserves for the option.

## Fields

| Field                   | Type                | Description                                                            | Value         |
|-------------------------|---------------------|------------------------------------------------------------------------|---------------|
| `Type`                  | string              | Identifies the event                                                   | `CONV_INVEST` |
| `Timestamp`             | DateTime (ISO-8601) | The timestamp of the event                                             |               |
| `Option`                | string              | The identifier for the investment option                               |               |
| `Invested_amount`       | decimal(20,4)       | The new total invested amount in the investment fund                   |               |
| `Cash_amount`           | decimal(20,4)       | The new total cash amount in the option's reserves                     |               |
| `Transaction_reference` | string              | An external reference for the investment transaction                   |               |

## Purpose

This event is essential for tracking investments and maintaining accurate records of both invested and cash amounts for each option.
