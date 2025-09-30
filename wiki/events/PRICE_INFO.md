---
title: PRICE_INFO
author: J.W. Morsink
---

# PRICE_INFO

The `PRICE_INFO` event allows input of current pricing information for an investment fund.
This enables the system to update the value of [donations](../donation) and plot their worth over time.

## Fields

| Field             | Type                | Description                                                                                    | Value        |
|-------------------|---------------------|------------------------------------------------------------------------------------------------|--------------|
| `Type`            | string              | Identifies the event.                                                                          | `PRICE_INFO` |
| `Timestamp`       | DateTime (ISO-8601) | The timestamp of the event.                                                                    |              |
| `Option`          | string              | The identifier for the investment option.                                                      |              |
| `Invested_amount` | decimal(20,4)       | The total invested amount in the fund, according to the current investment fund price.         |              |

## Purpose

This event is essential for keeping fund valuations up to date and for providing accurate historical data for option and donation worth.
