---
title: CONV_INCREASE_CASH
author: J.W. Morsink
difficulty: medium
---

# CONV_INCREASE_CASH

The `CONV_INCREASE_CASH` event registers a non-donation increase in the cash amount of an [investment option](../option).
This event is typically used when investment costs are covered by a third party or when other sources add cash to the option.
It only affects the cash part of the investment option and does not represent a donation.

## Fields

| Field       | Type                | Description                                         | Value                  |
|-------------|---------------------|-----------------------------------------------------|------------------------|
| `Type`      | string              | Identifies the event                                | `CONV_INCREASE_CASH`   |
| `Timestamp` | DateTime (ISO-8601) | The timestamp of the event                          |                        |
| `Option`    | string              | The identifier for the investment option            |                        |
| `Amount`    | decimal(20,4)       | The amount of cash to increase (must be ≥ 0)        |                        |

## Purpose

This event allows the system to track increases in cash that are not the result of donations.
It ensures that the cash balance of an investment option is accurate and reflects all sources of funding.

## Validation

* The `Option` field must be provided.
* The `Amount` must not be negative.
