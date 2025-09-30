--- 
title: DONA_UPDATE_CHARITY
author: J.W. Morsink
---

# DONA_UPDATE_CHARITY

The `DONA_UPDATE_CHARITY` event records a [donor's](../donor) decision to change the beneficiary [charity](../charity) for a specific [donation](../donation).
This event updates the charity that will receive the profits from the donation.

## Fields

| Field       | Type                | Description                                      | Value                 |
|-------------|---------------------|--------------------------------------------------|-----------------------|
| `Type`      | string              | Identifies the event.                            | `DONA_UPDATE_CHARITY` |
| `Timestamp` | DateTime (ISO-8601) | The timestamp of the event.                      |                       |
| `Donation`  | string              | Identifies the donation to be changed.           |                       |
| `Charity`   | string              | The identifier for the new beneficiary charity.  |                       |

## Purpose

This event allows donors to redirect the profits of their donation to a different charity, ensuring flexibility and donor control.
Although this process is supported by the backend, the frontend currently lacks the functionality to generate this event.
