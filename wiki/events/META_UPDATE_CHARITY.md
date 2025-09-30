--- 
title: META_UPDATE_CHARITY
author: J.W. Morsink
---
# META_UPDATE_CHARITY

The `META_UPDATE_CHARITY` event updates information for an existing [charity](../charity).
Only the fields supplied in the event are updated; unspecified fields remain unchanged.
Typical updates include bank account details and charity name.

## Fields

| Field             | Type               | Description                                   | Value                 |
|-------------------|--------------------|-----------------------------------------------|-----------------------|
| `Type`            | string             | Identifies the event.                         | `META_UPDATE_CHARITY` |
| `Timestamp`       | DateTime(ISO-8601) | The time of the event.                        |                       |
| `Code`            | string             | Identifies the charity.                       |                       |
| `Name`            | string (optional)  | The name of the charity.                      |                       |
| `Bank_account_no` | string (optional)  | The charity's bank account number.            |                       |
| `Bank_name`       | string (optional)  | The charity's name as registered by the bank. |                       |
| `Bank_bic`        | string (optional)  | The charity's bank identification code.       |                       |

## Purpose

This event is essential for keeping charity records up to date, especially for financial transactions and compliance.