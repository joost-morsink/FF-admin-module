--- 
title: CONV_LIQUIDATE
author: J.W. Morsink
difficulty: medium
---

# CONV_LIQUIDATE

The `CONV_LIQUIDATE` event records the liquidation of invested funds from an [investment option](../option).
It moves money from the invested amount to the cash reserves, typically in preparation for a future [transfer](../transfer) to [charities](../charity).
This event does not itself transfer funds to charities, but makes them available for allocation and transfer.

## Fields

| Field                   | Type                | Description                                                            | Value            |
|-------------------------|---------------------|------------------------------------------------------------------------|------------------|
| `Type`                  | string              | Identifies the event                                                   | `CONV_LIQUIDATE` |
| `Timestamp`             | DateTime (ISO-8601) | The timestamp of the event                                             |                  |
| `Option`                | string              | The identifier for the investment option                               |                  |
| `Invested_amount`       | decimal(20,4)       | The new total invested amount in the investment option                 |                  |
| `Cash_amount`           | decimal(20,4)       | The new total cash amount in the option's reserves                     |                  |
| `Transaction_reference` | string              | An external reference for the withdrawal transaction                   |                  |

## Purpose

This event is essential for tracking the movement of funds within an investment option and ensuring that cash is available for future allocation and transfer.
