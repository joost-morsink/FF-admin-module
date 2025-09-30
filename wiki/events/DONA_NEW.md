---
title: DONA_NEW
author: J.W. Morsink
---

# DONA_NEW

The `DONA_NEW` event records a [donation](../donation) made by a [donor](../donor) to a [charity](../charity) via an [investment option](../option).
If the donation currency matches the investment option's currency, the `Amount` and `Exchanged_amount` fields are equal.
If not, the `Exchanged_amount` field records the result of the currency exchange.

## Fields

| Field                   | Type                 | Description                                                                                             | Value      |
|-------------------------|----------------------|---------------------------------------------------------------------------------------------------------|------------|
| `Type`                  | string               | Identifies the event.                                                                                   | `DONA_NEW` |
| `Timestamp`             | DateTime (ISO-8601)  | The timestamp of the event.                                                                             |            |
| `Execute_timestamp`     | DateTime (ISO-8601)? | The timestamp the donation should be considered valid. If not present, use `Timestamp`.                 |            |
| `Donation`              | string               | Identifies the donation.                                                                                |            |
| `Donor`                 | string               | The identifier for the donor.                                                                           |            |
| `Charity`               | string               | The identifier for the charity.                                                                         |            |
| `Option`                | string               | The identifier for the investment option.                                                               |            |
| `Currency`              | string               | ISO-4217 currency code.                                                                                 |            |
| `Amount`                | decimal(16,4)        | The donated amount.                                                                                     |            |
| `Exchanged_amount`      | decimal(16,4)        | The donated amount in the currency of the investment option.                                            |            |
| `Transaction_reference` | string               | An external reference for the donation transaction.                                                     |            |
| `Exchange_reference`    | string (optional)    | An optional external reference for the exchange transaction.                                            |            |

## Purpose

This event is essential for tracking donations, handling currency exchange, and maintaining accurate records of donor, charity, and option relationships.
