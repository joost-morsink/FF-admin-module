--- 
title: META_NEW_CHARITY
author: J.W. Morsink
---

# META_NEW_CHARITY

The `META_NEW_CHARITY` event creates a new [charity](../charity) in the system.
This charity can then be selected by a [donor](../donor) as a beneficiary for future [donations](../donation).

## Fields

| Field       | Type               | Description             | Value              |
|-------------|--------------------|-------------------------|--------------------|
| `Type`      | string             | Identifies the event.   | `META_NEW_CHARITY` |
| `Timestamp` | DateTime(ISO-8601) | The time of the event.  |                    |
| `Code`      | string             | Identifies the charity. |                    |
| `Name`      | string             | The name of the charity.|                    |

## Purpose

This event is essential for onboarding new charities and supporting donations made to them.