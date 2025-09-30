---
title: CONV_TRANSFER
author: J.W. Morsink
---

# CONV_TRANSFER

The `CONV_TRANSFER` event represents the actual transfer of allocated funds to a [charity](../charity).
It finalizes the payout process by moving money from the system to the charity.
Currency exchange may be involved if the charity receives funds in a different currency.

## Fields

| Field                   | Type                | Description                                                                    | Value           |
|-------------------------|---------------------|--------------------------------------------------------------------------------|-----------------|
| `Type`                  | string              | Identifies the event.                                                          | `CONV_TRANSFER` |
| `Timestamp`             | DateTime (ISO-8601) | The timestamp of the event.                                                    |                 |
| `Charity`               | string              | The identifier of the charity.                                                 |                 |
| `Currency`              | string              | ISO-4217 currency code of the original amount.                                 |                 |
| `Amount`                | decimal(20,4)       | The amount transferred to the charity in the original currency.                |                 |
| `Exchanged_currency`    | string              | ISO-4217 currency code after exchange (charity's currency).                    |                 |
| `Exchanged_amount`      | decimal(20,4)       | The amount transferred in the exchanged currency.                              |                 |
| `Transaction_reference` | string              | External reference code for the transaction.                                   |                 |
| `Exchange_reference`    | string (optional)   | Optional external reference for the exchange.                                  |                 |

## Purpose

This event is essential for tracking the final movement of funds to charities.
It ensures accurate records of all transfers, including those involving currency exchange.
